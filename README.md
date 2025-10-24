# 🖥️ Kiosk7

**Kiosk7** is a secure, standalone **Windows kiosk browser** built with **C# (.NET)** using **Microsoft Visual Studio** and the **WebView2 engine**.  
It locks the computer into a single full-screen website view, blocking all keyboard shortcuts, system keys, and desktop access — ideal for terminals, receptions, or self-service points.

---

## 🚀 Features
- Full-screen browser restricted to specific URLs  
- Configurable **start URL**, **exit PIN**, and **domain allowlist**  
- On-screen keyboard for touchscreen kiosks  
- Optional visible EXIT button (for testing)  
- **Completely locked environment** — user cannot escape using Ctrl + Alt + Del, Alt + Tab, or Windows key  
- Fully **portable** — no installations required, runs directly from the folder  
- Built and tested in **Microsoft Visual Studio**

---

## ⚙️ Configuration

Edit your `settings.cfg` file to define your startup settings:  
https://www.bbc.com  

This example loads the BBC homepage when the kiosk launches.

When first opened, a **setup window** allows you to:
- Enter **Start URL**
- Set **Exit PIN**
- Define **Allowed domains**
- Optionally enable the red **EXIT button** or remember settings for next time  

---

## 📸 Screenshots

### 🪟 Setup Window  
When launched, Kiosk7 prompts the user to configure the startup URL, PIN, and optional features.  

<p align="left">
  <img src="https://github.com/user-attachments/assets/050fb9e2-1acc-4c2c-bb18-8f683d42242c" width="420" alt="Kiosk Setup Window">
</p>

### 🌐 Locked Kiosk Mode  
Once started, the browser runs in full-screen mode. The user can only exit using the configured PIN or on-screen controls.

<p align="left">
  <img src="https://github.com/user-attachments/assets/9b06871d-b8f3-4b99-8558-80071cd485b2" width="420" alt="Kiosk Browser Locked View">
</p>

---

## 📦 Distribution Notes

To run Kiosk7 on another system, copy the entire folder structure:  

Kiosk7/  
├─ Kiosk7.exe  
├─ settings.cfg  
├─ runtimes/  
│  └─ win-x64/native/WebView2Loader.dll  
├─ D3DCompiler_47_cor3.dll  
├─ vcruntime140_cor3.dll  
├─ PresentationNative_cor3.dll  
├─ PenImc_cor3.dll  
└─ wpfgfx_cor3.dll  

All necessary runtime files are included — simply copy the folder to any Windows 10/11 system and launch **Kiosk7.exe**.

---

## 💾 Download

📥 **Download the executable package:**  
The `.exe` file was too large to upload directly here, but you can download it using the link below:  

[➡️ **Download Kiosk7.exe (Google Drive)**](https://drive.google.com/file/d/1WCVjuryNo53_Ye4vxg85dvdVUjrmhbCc/view?usp=drive_link)

⚠️ **Important:**  
Make sure the downloaded `.exe` file is placed **in the same folder** as the `.dll` and configuration files listed above before running.  
If you move the executable outside this folder, the application will fail to start.

---

## 💡 Development Notes
Developed in **Microsoft Visual Studio** using **C# (.NET 6)** and **WinForms**.  
Implements kiosk lockdown logic, WebView2 browser embedding, exit PIN validation, and keyboard interception.  
Designed to run as a lightweight standalone application for any Windows system.

---

## 🧑‍💻 Author
Developed by **Christos Paraskevopoulos**  
📧 christos1129@gmail.com  
© 2025 — Part of the *DevOps Automation Portfolio*
