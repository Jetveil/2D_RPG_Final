import os
import time
import zipfile

watched_folder = "./Assets/"
output_zip = "./LatestCode.zip"

def zip_folder(folder_path, zip_path):
    with zipfile.ZipFile(zip_path, 'w') as zipf:
        for foldername, subfolders, filenames in os.walk(folder_path):
            for filename in filenames:
                if filename.endswith(".cs"):
                    filepath = os.path.join(foldername, filename)
                    arcname = os.path.relpath(filepath, folder_path)
                    zipf.write(filepath, arcname)

if __name__ == "__main__":
    last_modified = 0
    while True:
        current_modified = max(os.path.getmtime(os.path.join(dp, f)) 
                               for dp, dn, filenames in os.walk(watched_folder) 
                               for f in filenames if f.endswith(".cs"))
        if current_modified != last_modified:
            zip_folder(watched_folder, output_zip)
            print("🔁 ZIP обновлён!")
            last_modified = current_modified
        time.sleep(2)