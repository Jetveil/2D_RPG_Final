import os
import time
import zipfile

# Что включаем
WATCHED_FOLDERS = ["./Assets/", "./ProjectSettings/", "./Packages/"]
EXT = {".cs", ".anim", ".controller", ".prefab",
       ".unity", ".inputactions", ".asset", ".mat", ".json"}

# Что игнорируем (на всякий случай, если путь случайно добавят)
IGNORE_DIRS = {"/Library/", "/Temp/", "/Obj/", "/Logs/", "/UserSettings/"}

OUTPUT_ZIP = "./LatestCode.zip"
POLL_INTERVAL = 2


def should_include(file_path: str) -> bool:
    # игнорируем технические каталоги по подстроке
    norm = file_path.replace("\\", "/")
    if any(part in norm for part in IGNORE_DIRS):
        return False
    return os.path.splitext(file_path)[1] in EXT


def iter_files():
    for root in WATCHED_FOLDERS:
        for foldername, subfolders, filenames in os.walk(root):
            for filename in filenames:
                full = os.path.join(foldername, filename)
                if should_include(full):
                    yield root, full


def zip_project(zip_path: str):
    with zipfile.ZipFile(zip_path, "w", compression=zipfile.ZIP_DEFLATED) as zipf:
        for root, full in iter_files():
            arcname = os.path.relpath(full, root)
            # В архиве сохраняем структуру относительно своей корневой папки
            # чтобы было видно, из какой части проекта файл
            arcroot = os.path.basename(os.path.normpath(root))
            zipf.write(full, os.path.join(arcroot, arcname))


def latest_mtime() -> float:
    mtimes = []
    for _, full in iter_files():
        try:
            mtimes.append(os.path.getmtime(full))
        except FileNotFoundError:
            pass
    return max(mtimes) if mtimes else 0.0


if __name__ == "__main__":
    last_modified = 0.0
    while True:
        current_modified = latest_mtime()
        if current_modified > last_modified:
            zip_project(OUTPUT_ZIP)
            print("🔁 ZIP обновлён!")
            last_modified = current_modified
        time.sleep(POLL_INTERVAL)
