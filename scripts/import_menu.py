"""Import โครงเมนูจาก POS Excel เข้า SQLite (glurcafe.db)

สร้าง MenuFolder ตามหมวดหมู่ + MenuItem ตามเมนู (ชื่อ/ราคา/แนะนำ)
ยังไม่ใส่รูป — อัปโหลดรูปทีหลังผ่านหลังบ้าน /Admin/Menu

วิธีใช้:  python scripts/import_menu.py [path-to-xlsx]
รันใหม่ได้ปลอดภัย: ถ้ามีหมวด/เมนูอยู่แล้วจะข้าม (ไม่ซ้ำ)
"""
import sqlite3
import sys
from datetime import datetime

import openpyxl

XLSX = sys.argv[1] if len(sys.argv) > 1 else r"C:/Users/nutkamonc/Desktop/Export_Menu_Branch_2023681.xlsx"
DB = r"C:/Users/nutkamonc/source/repos/glur.cafe.page/glur.cafe.page/glurcafe.db"

# ลำดับแสดงผลหมวด + ไอคอน (Bootstrap Icons)
CATEGORY_ORDER = [
    ("แนะนำเมล็ดกาแฟ",        "bi-stars"),
    ("Coffee ( เมล็ดร้าน)",    "bi-cup-hot"),
    ("Coffee คั่วกลาง (เมล็ดร้าน)", "bi-cup-hot"),
    ("Coffee Inter Menu",     "bi-cup-hot"),
    ("Coffee Thai Menu",      "bi-cup-hot"),
    ("Matcha",                "bi-cup"),
    ("Tea-Milk",              "bi-cup"),
    ("Cocoa",                 "bi-cup"),
    ("Soda",                  "bi-cup-straw"),
    ("Fruit juice",           "bi-cup-straw"),
    ("Drink",                 "bi-cup-straw"),
    ("Snacks",                "bi-cookie"),
    ("Food",                  "bi-egg-fried"),
    ("เมล็ดกาแฟ",             "bi-bag"),
    ("เบียร์",                "bi-cup"),
    ("บวกเพิ่ม",              "bi-plus-circle"),
    ("อื่นๆ",                 "bi-three-dots"),
]
# หมวดที่ซ่อนไว้ก่อน (ออฟชั่นเสริม ไม่ใช่เมนูขายหน้าร้านโดยตรง)
INACTIVE_FOLDERS = {"บวกเพิ่ม"}

now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

wb = openpyxl.load_workbook(XLSX, read_only=True)
rows = list(wb["เมนู"].iter_rows(values_only=True))[1:]  # ข้าม header

conn = sqlite3.connect(DB)
cur = conn.cursor()

# สร้างตารางถ้ายังไม่มี (กันรันก่อนเปิดแอปครั้งแรก)
cur.execute("""CREATE TABLE IF NOT EXISTS MenuFolders (
    Id INTEGER NOT NULL CONSTRAINT PK_MenuFolders PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL, IconClassName TEXT NOT NULL DEFAULT 'bi-cup-hot',
    DisplayOrder INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1, CreatedAt TEXT NOT NULL)""")
cur.execute("""CREATE TABLE IF NOT EXISTS MenuItems (
    Id INTEGER NOT NULL CONSTRAINT PK_MenuItems PRIMARY KEY AUTOINCREMENT,
    FolderId INTEGER NOT NULL, FilePath TEXT NOT NULL,
    MediaType TEXT NOT NULL DEFAULT 'image', Title TEXT NULL, Price TEXT NULL,
    IsRecommended INTEGER NOT NULL DEFAULT 0,
    DisplayOrder INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1, CreatedAt TEXT NOT NULL,
    CONSTRAINT FK_MenuItems_MenuFolders_FolderId
        FOREIGN KEY (FolderId) REFERENCES MenuFolders (Id) ON DELETE CASCADE)""")
try:
    cur.execute("ALTER TABLE MenuItems ADD COLUMN IsRecommended INTEGER NOT NULL DEFAULT 0")
except sqlite3.OperationalError:
    pass  # มีคอลัมน์อยู่แล้ว

def get_or_create_folder(name, icon, order, active=1):
    cur.execute("SELECT Id FROM MenuFolders WHERE Name = ?", (name,))
    row = cur.fetchone()
    if row:
        return row[0]
    cur.execute(
        "INSERT INTO MenuFolders (Name, IconClassName, DisplayOrder, IsActive, CreatedAt) VALUES (?,?,?,?,?)",
        (name, icon, order, active, now))
    return cur.lastrowid

icon_map = dict(CATEGORY_ORDER)
created_folders = 0
created_items = 0

# สร้างหมวดตามลำดับ
order = 10
folder_ids = {}
for name, icon in CATEGORY_ORDER:
    active = 0 if name in INACTIVE_FOLDERS else  1
    fid = get_or_create_folder(name, icon, order, active)
    folder_ids[name] = fid
    order += 10

# เพิ่มเมนู
for r in rows:
    cat, name, price, rec = r[1], r[2], r[6], r[9]
    if not name:
        continue
    cat = cat if cat in folder_ids else "อื่นๆ"
    fid = folder_ids[cat]
    cur.execute("SELECT Id FROM MenuItems WHERE FolderId = ? AND Title = ?", (fid, str(name)))
    if cur.fetchone():
        continue
    cur.execute("SELECT COALESCE(MAX(DisplayOrder), 0) + 1 FROM MenuItems WHERE FolderId = ?", (fid,))
    next_order = cur.fetchone()[0]
    price_val = None
    try:
        if price not in (None, ""):
            price_val = str(float(price)).rstrip("0").rstrip(".")
    except ValueError:
        price_val = None
    cur.execute(
        "INSERT INTO MenuItems (FolderId, FilePath, MediaType, Title, Price, IsRecommended, DisplayOrder, IsActive, CreatedAt) "
        "VALUES (?, '', 'image', ?, ?, ?, ?, 1, ?)",
        (fid, str(name).strip(), price_val, 1 if rec == "Yes" else 0, next_order, now))
    created_items += 1

conn.commit()
total_folders = cur.execute("SELECT COUNT(*) FROM MenuFolders").fetchone()[0]
total_items = cur.execute("SELECT COUNT(*) FROM MenuItems").fetchone()[0]
print(f"เสร็จ: หมวดทั้งหมด {total_folders} | เมนูทั้งหมด {total_items} (เพิ่มใหม่ {created_items})")
conn.close()
