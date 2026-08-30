# Facebook & Social Integration — Design & Knowledge Base

> Module สำหรับ **glur.cafe.page** (ASP.NET Core Razor Pages + EF Core + SQLite)
> อัปเดตล่าสุด: 2026-06-13

---

## Overview

โมดูลนี้แบ่งออกเป็น 4 หมวดหลักตามลักษณะงาน:

| หมวด | Feature | Admin Page |
|---|---|---|
| 💬 สื่อสาร | Messenger Chat Plugin | `/Admin/Facebook/Messenger` |
| 📊 Tracking | Facebook Pixel + Conversions API | `/Admin/Facebook/Pixel` |
| 🖼️ แสดงผล | Facebook Page Feed + Instagram Feed | `/Admin/Facebook/Feed` |
| 🔗 SEO/Share | Open Graph Meta Tags | Settings (Group: SEO) |

---

## Folder Structure

```
Pages/Admin/Facebook/
├── Index.cshtml            # Dashboard: สรุปสถานะ integrations ทั้งหมด
├── Index.cshtml.cs
├── Messenger.cshtml        # Messenger Chat Plugin settings
├── Messenger.cshtml.cs
├── Pixel.cshtml            # Facebook Pixel + Conversions API
├── Pixel.cshtml.cs
├── Feed.cshtml             # Facebook Page Feed + Instagram Feed
└── Feed.cshtml.cs
```

---

## Sidebar Admin Menu (เพิ่ม section ใหม่)

เพิ่มใน `_AdminLayout.cshtml` หลัง section "การเงิน":

```html
<div class="nav-section-label mt-3">Facebook & Social</div>
<a href="/Admin/Facebook" class="sidebar-link ...">
    <i class="bi bi-facebook"></i><span>Facebook Overview</span>
</a>
<a href="/Admin/Facebook/Messenger" class="sidebar-link ...">
    <i class="bi bi-chat-dots-fill"></i><span>Messenger Chat</span>
</a>
<a href="/Admin/Facebook/Pixel" class="sidebar-link ...">
    <i class="bi bi-graph-up-arrow"></i><span>Pixel & Conversions</span>
</a>
<a href="/Admin/Facebook/Feed" class="sidebar-link ...">
    <i class="bi bi-grid-3x2-gap-fill"></i><span>Social Feed</span>
</a>
```

---

## SiteSettings Keys ที่ต้องเพิ่ม

| Key | Group | Type | Description |
|---|---|---|---|
| `FacebookPageId` | Facebook | string | Facebook Page ID (ตัวเลข เช่น `123456789`) |
| `FacebookAppId` | Facebook | string | App ID จาก developers.facebook.com |
| `FacebookPixelId` | Facebook | string | Pixel ID จาก Meta Events Manager |
| `FacebookConversionsToken` | Facebook | string (secret) | Access Token สำหรับ Conversions API |
| `FacebookInstagramUserId` | Facebook | string | Instagram Business User ID |
| `FacebookInstagramToken` | Facebook | string (secret) | Instagram Graph API Long-lived Access Token |
| `MessengerEnabled` | Facebook | bool string | `"true"` / `"false"` |
| `MessengerThemeColor` | Facebook | hex string | เช่น `#1877F2` |
| `MessengerGreeting` | Facebook | string | ข้อความต้อนรับใน chat เช่น `สวัสดี! มีอะไรให้ช่วยไหม?` |
| `PixelEnabled` | Facebook | bool string | `"true"` / `"false"` |
| `FbPageFeedEnabled` | Facebook | bool string | `"true"` / `"false"` |
| `FbPageFeedPostCount` | Facebook | int string | จำนวน posts ที่แสดง เช่น `"5"` |
| `InstagramFeedEnabled` | Facebook | bool string | `"true"` / `"false"` |
| `InstagramFeedCount` | Facebook | int string | จำนวนรูปที่แสดง เช่น `"9"` |
| `OgImage` | SEO | string | URL รูป Open Graph (1200x630px แนะนำ) |
| `OgSiteName` | SEO | string | ชื่อเว็บ เช่น `GLUR CAFE` |
| `OgDescription` | SEO | string | คำอธิบายสำหรับ share เช่น `โรงคั่วกาแฟ...` |

---

## Page Designs

### 1. `/Admin/Facebook` — Overview Dashboard

```
┌─────────────────────────────────────────────────────────────┐
│  Facebook & Social Integration                              │
├──────────────────┬──────────────────┬───────────────────────┤
│  💬 Messenger    │  📊 Pixel        │  🖼️ Social Feed        │
│  ─────────────  │  ──────────────  │  ───────────────────  │
│  ✅ เปิดใช้งาน  │  ✅ เปิดใช้งาน  │  ⚠️ Token หมดอายุ    │
│  Page: 12345..  │  ID: 98765..     │  Instagram: expired   │
│  [จัดการ →]     │  [จัดการ →]     │  [จัดการ →]           │
├──────────────────┴──────────────────┴───────────────────────┤
│  🔗 Open Graph                                              │
│  ─────────────────                                          │
│  ✅ OG Image ตั้งค่าแล้ว | OG Description: มีค่า          │
│  [ไปที่ Settings SEO →]                                     │
└─────────────────────────────────────────────────────────────┘
```

**Status badges:**
- ✅ สีเขียว = ตั้งค่าครบ + เปิดใช้งาน
- ⚠️ สีเหลือง = ตั้งค่าแล้ว แต่มีปัญหา (token หมดอายุ, ปิดอยู่)
- ❌ สีแดง = ยังไม่ตั้งค่า

---

### 2. `/Admin/Facebook/Messenger` — Messenger Chat Plugin

```
┌─────────────────────────────────────────────────────────────┐
│  ← Facebook  /  💬 Messenger Chat Plugin                    │
├─────────────────────────────────────────────────────────────┤
│  [ℹ️] ต้องใช้ Facebook App ที่เปิด "Messenger" product     │
│       และ Whitelist domain ของเว็บไซต์ใน App Settings      │
├─────────────────────────────────────────────────────────────┤
│  การตั้งค่า                                                 │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  เปิดใช้งาน Messenger Chat    [Toggle ● ON]          │  │
│  │  ─────────────────────────────────────────────────── │  │
│  │  Facebook Page ID     [________________________]     │  │
│  │  Facebook App ID      [________________________]     │  │
│  │  Theme Color          [#1877F2] [████ preview]       │  │
│  │  ข้อความต้อนรับ       [สวัสดี! มีอะไรให้ช่วยไหม?]  │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                             │
│  วิธีหา Page ID:                                           │
│  [🔗 ลิงก์ไปยัง facebook.com/YOUR_PAGE/about]             │
│                                              [💾 บันทึก]   │
└─────────────────────────────────────────────────────────────┘
```

**Frontend Effect:** ฝัง Facebook Customer Chat SDK + `<div class="fb-customerchat">` ใน `_Layout.cshtml`

```html
<!-- Messenger Chat Plugin (ถ้า MessengerEnabled = true) -->
@if (_settings["MessengerEnabled"] == "true" && !string.IsNullOrEmpty(_settings["FacebookPageId"]))
{
    <div id="fb-root"></div>
    <div class="fb-customerchat"
         attribution="setup_tool"
         page_id="@_settings["FacebookPageId"]"
         theme_color="@_settings["MessengerThemeColor"]"
         logged_in_greeting="@_settings["MessengerGreeting"]"
         logged_out_greeting="@_settings["MessengerGreeting"]">
    </div>
    <script>
      window.fbAsyncInit = function() {
        FB.init({ xfbml: true, version: 'v20.0' });
      };
      (function(d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) return;
        js = d.createElement(s); js.id = id;
        js.src = 'https://connect.facebook.net/th_TH/sdk/xfbml.customerchat.js';
        fjs.parentNode.insertBefore(js, fjs);
      }(document, 'script', 'facebook-jssdk'));
    </script>
}
```

---

### 3. `/Admin/Facebook/Pixel` — Pixel & Conversions API

```
┌─────────────────────────────────────────────────────────────┐
│  ← Facebook  /  📊 Pixel & Conversions API                  │
├─────────────────────────────────────────────────────────────┤
│  ── Facebook Pixel (Browser-side) ──────────────────────── │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  เปิดใช้งาน Pixel      [Toggle]                      │  │
│  │  Pixel ID               [________________________]   │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                             │
│  ── Conversions API (Server-side) ──────────────────────── │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Access Token    [________________________] [👁 Show]  │  │
│  │  Events ที่ส่ง:                                       │  │
│  │  ☑ PageView (ทุก request)                            │  │
│  │  ☑ Contact (เมื่อ submit ฟอร์มติดต่อ)               │  │
│  │  ☑ ViewContent (เมื่อดูหน้า Index)                   │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                             │
│  ── ทดสอบ ────────────────────────────────────────────── │
│  [▶ ส่ง Test Event "PageView"]                            │
│  สถานะล่าสุด:  ✅ ส่งสำเร็จ — 2026-06-13 10:30          │
│                                              [💾 บันทึก]   │
└─────────────────────────────────────────────────────────────┘
```

**Conversions API Flow (Server-side):**
```
Contact Form Submit → IndexModel.OnPostContactAsync()
                   → FacebookConversionsService.SendEventAsync("Contact", ...)
                   → POST https://graph.facebook.com/v20.0/{pixel_id}/events
                        Authorization: Bearer {access_token}
                        Body: { data: [{ event_name, event_time, user_data, ... }] }
```

**New Service:** `Services/FacebookConversionsService.cs`

---

### 4. `/Admin/Facebook/Feed` — Social Feed (2 tabs)

```
┌─────────────────────────────────────────────────────────────┐
│  ← Facebook  /  🖼️ Social Feed                              │
├─────────────────────────────────────────────────────────────┤
│  [📘 Facebook Page Feed]  [📸 Instagram Feed]  ← tabs       │
├─────────────────────────────────────────────────────────────┤
│  TAB: Facebook Page Feed                                    │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  เปิดใช้งาน Page Feed   [Toggle]                     │  │
│  │  Facebook Page ID        [ดึงจาก Messenger Settings] │  │
│  │  Facebook App ID         [ดึงจาก Messenger Settings] │  │
│  │  จำนวน Posts ที่แสดง    [5  ▼] (3/5/10)            │  │
│  │  Width                   [auto / 500px]              │  │
│  │  แสดง Tabs               [☑ timeline  ☐ events]     │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                             │
│  [ดูตัวอย่าง] (iframe preview)                              │
├─────────────────────────────────────────────────────────────┤
│  TAB: Instagram Feed                                        │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  เปิดใช้งาน Instagram Feed  [Toggle]                 │  │
│  │  Instagram User ID           [__________________]    │  │
│  │  Access Token (Long-lived)   [__________________][👁] │  │
│  │  จำนวนรูปที่แสดง             [9  ▼] (6/9/12)        │  │
│  │  Layout                      [Grid ▼] (Grid/Row)    │  │
│  │  ─────────────────────────────────────────────────── │  │
│  │  สถานะ Token:  ✅ ใช้งานได้ (หมดอายุ: ไม่หมด)      │  │
│  │  [🔄 ทดสอบเชื่อมต่อ]  [+ Refresh Token]             │  │
│  └───────────────────────────────────────────────────────┘  │
│                                              [💾 บันทึก]   │
└─────────────────────────────────────────────────────────────┘
```

**Instagram Feed แสดงบนหน้าเว็บ (`Index.cshtml`):**
```
section#instagram (ระหว่าง gallery กับ contact)
┌──────────────────────────────────────────────────────────┐
│  ติดตามเราบน Instagram  @glurcafe                       │
│  ┌────┐ ┌────┐ ┌────┐                                   │
│  │ 📸 │ │ 📸 │ │ 📸 │  ← grid 3 columns               │
│  └────┘ └────┘ └────┘  (hover = แสดง likes/caption)    │
│  ┌────┐ ┌────┐ ┌────┐                                   │
│  │ 📸 │ │ 📸 │ │ 📸 │                                   │
│  └────┘ └────┘ └────┘                                   │
│  ┌────┐ ┌────┐ ┌────┐                                   │
│  │ 📸 │ │ 📸 │ │ 📸 │                                   │
│  └────┘ └────┘ └────┘                                   │
│              [ดูเพิ่มเติมบน Instagram →]                 │
└──────────────────────────────────────────────────────────┘
```

**Facebook Page Feed แสดงบนหน้าเว็บ:**
```
section#page-feed (ด้านล่าง stats)
┌──────────────────────────────────────────────────────────┐
│  อัปเดตล่าสุดจาก Facebook Page                         │
│  ┌──────────────────────────────────────────────────┐   │
│  │  [Facebook Page Plugin iframe]                   │   │
│  │   (fb-page widget แสดง posts/timeline)           │   │
│  └──────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────┘
```

---

### 5. Open Graph Tags (ใน Settings — Group: SEO)

เพิ่ม Keys ใน Settings page และฝัง `<meta>` ใน `_Layout.cshtml`:

```html
<!-- Open Graph -->
<meta property="og:type"        content="website" />
<meta property="og:url"         content="https://glurcafe.com/" />
<meta property="og:title"       content="@(ViewData["OgTitle"] ?? _settings["OgSiteName"])" />
<meta property="og:description" content="@(ViewData["OgDescription"] ?? _settings["OgDescription"])" />
<meta property="og:image"       content="@_settings["OgImage"]" />
<meta property="og:site_name"   content="@_settings["OgSiteName"]" />
<meta property="fb:app_id"      content="@_settings["FacebookAppId"]" />
```

---

## Instagram Graph API — Token Guide

| Step | คำอธิบาย |
|---|---|
| 1 | สร้าง Facebook App ที่ developers.facebook.com |
| 2 | เพิ่ม Instagram Basic Display หรือ Instagram Graph API product |
| 3 | สร้าง User Access Token → แปลงเป็น Long-lived Token (60 วัน) |
| 4 | ใช้ `GET /me/media?fields=id,media_type,thumbnail_url,permalink,media_url` |
| 5 | Refresh token ก่อนหมดอายุ (ปุ่ม Refresh Token ใน Feed settings) |

> ⚠️ Instagram Long-lived Token หมดอายุใน **60 วัน** ต้อง refresh ก่อนหมด

---

## Conversions API — Endpoint

```
POST https://graph.facebook.com/v20.0/{PIXEL_ID}/events
     ?access_token={FACEBOOK_CONVERSIONS_TOKEN}

Body:
{
  "data": [{
    "event_name": "Contact",
    "event_time": 1718265600,
    "action_source": "website",
    "event_source_url": "https://glurcafe.com/",
    "user_data": {
      "client_ip_address": "...",
      "client_user_agent": "..."
    }
  }]
}
```

---

## Implementation Checklist

- [ ] เพิ่ม SiteSettings keys ใหม่ใน `DbSeeder.cs`
- [ ] อัปเดต `_AdminLayout.cshtml` — เพิ่ม sidebar section "Facebook & Social"
- [ ] สร้าง `Pages/Admin/Facebook/Index.cshtml` — Overview Dashboard
- [ ] สร้าง `Pages/Admin/Facebook/Messenger.cshtml` — Messenger Settings
- [ ] สร้าง `Pages/Admin/Facebook/Pixel.cshtml` — Pixel + Conversions API
- [ ] สร้าง `Pages/Admin/Facebook/Feed.cshtml` — Page Feed + Instagram Feed
- [ ] สร้าง `Services/FacebookConversionsService.cs` — HTTP call ไปยัง Meta API
- [ ] อัปเดต `_Layout.cshtml` — เพิ่ม OG tags + Messenger SDK + Pixel base code
- [ ] อัปเดต `Index.cshtml` — เพิ่ม section Instagram Feed + Facebook Page Feed
- [ ] อัปเดต `Pages/Admin/Settings/Index.cshtml` — เพิ่ม Group "SEO" + "Facebook"
- [ ] Register `FacebookConversionsService` ใน `Program.cs` (HttpClient)
- [ ] ทดสอบ Pixel ด้วย Meta Pixel Helper extension
- [ ] ทดสอบ Messenger Chat บน domain จริง (ต้อง whitelist)
