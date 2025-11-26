#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import os
import sys
import zipfile
import argparse
import time
from pathlib import Path
from typing import Iterable, Tuple, Dict, Tuple as Tup

# -----------------------------
# Configuration
# -----------------------------

# Directories that should never be included in the archive.
# Keep 'Assets', 'Packages', and 'ProjectSettings' — they are essential for Unity.
IGNORE_DIRS = {
    "Library",
    "Temp",
    "Obj",
    "Logs",
    "UserSettings",
    ".git",
    ".github",
    ".gitlab",
    ".idea",
    ".vs",
    ".vscode",
    "Build",
    "Builds",
    "CrashReports",
    "MemoryCaptures",
    "Artifacts",
    ".cache",
    "__pycache__",
}

# File extensions to include. This is conservative but covers most Unity projects.
EXT = {
    # C#/text/config
    ".cs", ".json", ".yaml", ".yml", ".txt", ".md", ".asmdef", ".rsp",
    # Unity assets
    ".unity", ".prefab", ".mat", ".anim", ".controller",
    ".overrideController", ".spriteatlas",
    ".asset", ".guiskin", ".physicMaterial", ".physicsMaterial2D",
    ".shader", ".cginc", ".compute", ".shadergraph", ".uxml", ".uss",
    # Meta & settings (crucial for GUID links)
    ".meta",
    # Common art/audio that are safe to include (source assets)
    ".png", ".jpg", ".jpeg", ".psd", ".tga", ".bmp", ".gif",
    ".wav", ".mp3", ".ogg",
    ".ttf", ".otf",
}

# Markers that strongly suggest the file is truncated or a placeholder.
SUSPICIOUS_MARKERS = (
    "...",
    "…",
    "<<conversation too long; truncated>>",
)

# -----------------------------
# Helpers
# -----------------------------


def is_ignored_dir(dirname: str) -> bool:
    """Return True if dirname should be skipped entirely."""
    base = os.path.basename(dirname)
    return base in IGNORE_DIRS


def allow_file(path: str) -> bool:
    """Return True if file has an allowed extension."""
    return os.path.splitext(path)[1] in EXT


def guard_scan_text_file(path: str) -> None:
    """Fail fast if a .cs file looks truncated (contains markers like '...' or '…')."""
    if not path.lower().endswith(".cs"):
        return
    try:
        with open(path, "r", encoding="utf-8", errors="ignore") as f:
            txt = f.read()
        for m in SUSPICIOUS_MARKERS:
            if m in txt:
                raise RuntimeError(
                    f"Файл выглядит усечённым (обнаружен маркер '{m}'): {path}")
    except UnicodeDecodeError as e:
        raise RuntimeError(f"Не удалось прочитать как текст: {path}") from e


def iter_files(root: str) -> Iterable[str]:
    """Yield project files (filtered) under root."""
    for dirpath, dirnames, filenames in os.walk(root):
        # Filter directories in-place to prevent walking into them
        dirnames[:] = [d for d in dirnames if os.path.basename(
            d) not in IGNORE_DIRS]
        for name in filenames:
            full = os.path.join(dirpath, name)
            if allow_file(full):
                yield full


def zip_project(src_root: str, zip_path: str) -> Tuple[int, int]:
    """
    Zip the project rooted at src_root into zip_path.
    Returns (file_count, total_bytes).
    """
    src_root = os.path.abspath(src_root)
    file_count = 0
    total_bytes = 0
    Path(os.path.dirname(zip_path) or ".").mkdir(parents=True, exist_ok=True)

    with zipfile.ZipFile(zip_path, "w", compression=zipfile.ZIP_DEFLATED) as zipf:
        for full in iter_files(src_root):
            try:
                # Guard only .cs files for now
                guard_scan_text_file(full)

                # keep paths relative to project root
                arcname = os.path.relpath(full, src_root)
                zipf.write(full, arcname)
                file_count += 1
                try:
                    total_bytes += os.path.getsize(full)
                except OSError:
                    pass
            except FileNotFoundError:
                print(f"⚠️  Пропущен (исчез во время упаковки): {full}")
    return file_count, total_bytes


def build_snapshot(root: str) -> Dict[str, Tup[int, int]]:
    """
    Собрать "снимок" проекта:
    словарь {относительный_путь: (mtime_ns, size)}.
    Нужен, чтобы понимать, что что-то изменилось.
    """
    snapshot: Dict[str, Tup[int, int]] = {}
    root = os.path.abspath(root)

    for full in iter_files(root):
        rel = os.path.relpath(full, root)
        try:
            st = os.stat(full)
        except FileNotFoundError:
            # файл могли удалить прямо во время обхода
            continue
        snapshot[rel] = (st.st_mtime_ns, st.st_size)

    return snapshot


def human_size(num: int) -> str:
    for unit in ("Б", "КБ", "МБ", "ГБ"):
        if num < 1024.0:
            return f"{num:.1f} {unit}"
        num /= 1024.0
    return f"{num:.1f} ТБ"


def pack_once(root: str, out_zip: str) -> Dict[str, Tup[int, int]] | None:
    """
    Один раз упаковать проект и вернуть снимок состояния файлов.
    Если что-то пошло не так (усечённый .cs и т.п.) — вернуть None.
    """
    print(f"📦 Упаковка проекта:\n  Корень: {root}\n  ZIP:    {out_zip}")
    try:
        count, total = zip_project(root, out_zip)
    except RuntimeError as e:
        print(f"❌ Остановлено: {e}")
        return None

    print(f"✅ Упаковано файлов: {count}")
    print(f"📏 Суммарный размер исходников: {human_size(total)}")
    print("Готово.")
    return build_snapshot(root)


def watch_and_pack(root: str, out_zip: str, interval: float) -> int:
    """
    Следить за изменениями в проекте и перепаковывать ZIP.
    """
    root = os.path.abspath(root)
    out_zip = os.path.abspath(out_zip)

    snapshot = pack_once(root, out_zip)
    if snapshot is None:
        return 1

    print(
        f"👀 Наблюдение за изменениями каждые {interval:.1f} с. Нажми Ctrl+C для выхода.")
    while True:
        try:
            time.sleep(interval)
            new_snapshot = build_snapshot(root)
            if new_snapshot != snapshot:
                print("✏️  Обнаружены изменения. Перепаковка...")
                snapshot = pack_once(root, out_zip)
                if snapshot is None:
                    # если при перепаковке нашли битый .cs — выходим, чтобы не молча молоть ошибки
                    return 1
        except KeyboardInterrupt:
            print("\n⏹ Остановлено пользователем.")
            return 0


def parse_args(argv=None):
    p = argparse.ArgumentParser(
        description="Упаковать Unity-проект в ZIP с проверками на усечённые .cs файлы."
    )
    p.add_argument(
        "--root",
        default=".",
        help="Корень проекта (по умолчанию текущая папка)."
    )
    p.add_argument(
        "--out",
        default="LatestCode.zip",
        help="Путь к выходному ZIP (по умолчанию LatestCode.zip)."
    )
    p.add_argument(
        "--once",
        action="store_true",
        help="Упаковать один раз и выйти (без слежения за изменениями)."
    )
    p.add_argument(
        "--interval",
        type=float,
        default=2.0,
        help="Интервал проверки изменений в секундах (по умолчанию 2.0)."
    )
    return p.parse_args(argv)


def main(argv=None) -> int:
    args = parse_args(argv)
    root = os.path.abspath(args.root)
    out_zip = os.path.abspath(args.out)

    if not os.path.isdir(root):
        print(f"❌ Корень проекта не найден: {root}")
        return 2

    if args.once:
        # Один раз запаковали и вышли
        snapshot = pack_once(root, out_zip)
        return 0 if snapshot is not None else 1
    else:
        # Режим “демона”: следим за изменениями и перепаковываем
        return watch_and_pack(root, out_zip, args.interval)


if __name__ == "__main__":
    exit_code = main()
    # На Windows при запуске двойным кликом окно мгновенно закроется.
    # Дадим возможность прочитать вывод.
    if os.name == "nt":
        try:
            input("Нажмите Enter для выхода...")
        except EOFError:
            pass
    sys.exit(exit_code)
