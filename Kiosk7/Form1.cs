using System;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Kiosk7
{
    public partial class Form1 : Form
    {
        private Config _cfg;                 // settings.cfg
        private bool _allowClose = false;    // set true only after correct PIN
        private KeyboardForm? _keyboard;     // on-screen keyboard instance

        // ---- WinAPI: hide/show taskbar & start ----
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private IntPtr _hTaskbar;
        private IntPtr _hSecondaryTaskbar;
        private IntPtr _hStartButton;

        private void HideSystemUI()
        {
            // Primary taskbar
            _hTaskbar = FindWindow("Shell_TrayWnd", null);
            if (_hTaskbar != IntPtr.Zero) ShowWindow(_hTaskbar, SW_HIDE);

            // Secondary taskbars (multi-monitor)
            _hSecondaryTaskbar = FindWindow("Shell_SecondaryTrayWnd", null);
            if (_hSecondaryTaskbar != IntPtr.Zero) ShowWindow(_hSecondaryTaskbar, SW_HIDE);

            // Start button (may not be found on Win11)
            _hStartButton = FindWindow("Button", "Start");
            if (_hStartButton != IntPtr.Zero) ShowWindow(_hStartButton, SW_HIDE);
        }

        private void ShowSystemUI()
        {
            if (_hTaskbar != IntPtr.Zero) ShowWindow(_hTaskbar, SW_SHOW);
            if (_hSecondaryTaskbar != IntPtr.Zero) ShowWindow(_hSecondaryTaskbar, SW_SHOW);
            if (_hStartButton != IntPtr.Zero) ShowWindow(_hStartButton, SW_SHOW);
        }

        // ---- Global low-level keyboard hook (blocks Win keys, Alt+Tab, etc.) ----
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")] private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")] private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        // Virtual keys
        private const int VK_TAB = 0x09;
        private const int VK_ESCAPE = 0x1B;
        private const int VK_SPACE = 0x20;
        private const int VK_F4 = 0x73;
        private const int VK_F11 = 0x7A;
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;
        private const int VK_APPS = 0x5D; // menu key

        // Modifiers
        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12; // Alt

        private IntPtr _kbHook = IntPtr.Zero;
        private LowLevelKeyboardProc _kbProc; // keep delegate alive

        private static bool IsKeyDown(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);

                bool alt = IsKeyDown(VK_MENU);
                bool ctrl = IsKeyDown(VK_CONTROL);
                bool shift = IsKeyDown(VK_SHIFT);

                // Block OS/nav combos (note: Ctrl+Alt+Del cannot be blocked from user mode)
                bool block = false;

                // Windows keys & menu key
                if (vkCode == VK_LWIN || vkCode == VK_RWIN || vkCode == VK_APPS)
                    block = true;

                // Alt+Tab, Alt+Esc, Alt+F4, Alt+Space
                if (alt && (vkCode == VK_TAB || vkCode == VK_ESCAPE || vkCode == VK_F4 || vkCode == VK_SPACE))
                    block = true;

                // F11 (fullscreen)
                if (vkCode == VK_F11) block = true;

                // Ctrl+Esc (Start), Ctrl+Shift+Esc (Task Manager)
                if ((ctrl && vkCode == VK_ESCAPE) || (ctrl && shift && vkCode == VK_ESCAPE))
                    block = true;

                if (block)
                    return (IntPtr)1; // swallow
            }
            return CallNextHookEx(_kbHook, nCode, wParam, lParam);
        }

        private void InstallKeyboardHook()
        {
            _kbProc = KeyboardHookCallback;
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule!;
            _kbHook = SetWindowsHookEx(WH_KEYBOARD_LL, _kbProc, GetModuleHandle(curModule.ModuleName), 0);
        }

        private void UninstallKeyboardHook()
        {
            if (_kbHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_kbHook);
                _kbHook = IntPtr.Zero;
            }
        }

        // Send keys to the WebView (letters/numbers/space, {BACKSPACE})
        private void SendToWeb(string keySpec)
        {
            try
            {
                webView21.Focus();
                SendKeys.SendWait(keySpec);
            }
            catch { /* ignore transient focus issues */ }
        }

        public Form1()
        {
            InitializeComponent();

            // Load settings.cfg from the EXE folder
            _cfg = Config.Load(AppContext.BaseDirectory);

            // Ask the user every launch
            using (var setup = new SetupForm(_cfg.Url, _cfg.Pin))
            {
                var result = setup.ShowDialog(this);
                if (result != DialogResult.OK)
                {
                    Close();
                    return;
                }
                _cfg.Url = setup.SelectedUrl;
                _cfg.Pin = setup.SelectedPin;
            }

            // Fullscreen kiosk window
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            KeyPreview = true;

            // Fill the window with the browser
            webView21.Dock = DockStyle.Fill;

            // ---- EXIT button (top-left), only if enabled in settings.cfg ----
            if (_cfg.ShowExitButton)
            {
                var exitBtn = new Button
                {
                    Name = "ExitButton",
                    Text = "\U0001F513  EXIT",
                    UseCompatibleTextRendering = true,
                    Font = new Font("Segoe UI Emoji", 16f, FontStyle.Bold),
                    BackColor = Color.Red,
                    ForeColor = Color.White,
                    Size = new Size(160, 56),
                    FlatStyle = FlatStyle.Flat,
                    TabStop = false,
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left
                };
                exitBtn.FlatAppearance.BorderSize = 0;
                exitBtn.Location = new Point(12, 12);

                exitBtn.Click += (_, __) =>
                {
                    using var dlg = new PinPrompt();
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        if (dlg.PinText == _cfg.Pin)
                        {
                            _allowClose = true;
                            ShowSystemUI();
                            UninstallKeyboardHook();
                            Application.Exit();
                        }
                        else
                        {
                            SystemSounds.Hand.Play();
                        }
                    }
                };

                Controls.Add(exitBtn);
                exitBtn.BringToFront();
            }

            // ---- Keyboard toggle button (top-right) ----
            var kbBtn = new Button
            {
                Name = "KeyboardButton",
                Text = "\u2328  Keyboard", // ? Keyboard
                UseCompatibleTextRendering = true,
                Font = new Font("Segoe UI Emoji", 16f, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Size = new Size(160, 56),
                FlatStyle = FlatStyle.Flat,
                TabStop = false,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            kbBtn.FlatAppearance.BorderSize = 0;
            Controls.Add(kbBtn);
            kbBtn.BringToFront();
            kbBtn.Location = new Point(ClientSize.Width - kbBtn.Width - 12, 12);
            this.Resize += (_, __) =>
            {
                if (!IsDisposed && Controls.Contains(kbBtn))
                    kbBtn.Location = new Point(ClientSize.Width - kbBtn.Width - 12, 12);
            };
            kbBtn.Click += (_, __) =>
            {
                if (_keyboard == null || _keyboard.IsDisposed)
                    _keyboard = new KeyboardForm(SendToWeb) { TopMost = true, Owner = this };

                if (_keyboard.Visible) _keyboard.Hide();
                else { _keyboard.Show(this); _keyboard.BringToFront(); }
            };

            // Keep kiosk dominant & hide system UI after it appears
            Shown += (_, __) =>
            {
                HideSystemUI();
                InstallKeyboardHook();
                Activate();
                TopMost = true;
            };

            Activated += (_, __) => { TopMost = true; };
            Deactivate += (_, __) => { Activate(); }; // pull focus back

            // Prevent Alt+F4 close unless the PIN was confirmed
            FormClosing += (s, e) =>
            {
                if (!_allowClose)
                {
                    using var dlg = new PinPrompt();
                    if (dlg.ShowDialog(this) == DialogResult.OK && dlg.PinText == _cfg.Pin)
                    {
                        _allowClose = true;
                        ShowSystemUI();
                        UninstallKeyboardHook();
                    }
                    else
                    {
                        e.Cancel = true;
                        SystemSounds.Hand.Play();
                    }
                }
            };

            // F10 quick exit prompt (optional)
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F10)
                {
                    e.Handled = true;
                    using var dlg = new PinPrompt();
                    if (dlg.ShowDialog(this) == DialogResult.OK && dlg.PinText == _cfg.Pin)
                    {
                        _allowClose = true;
                        ShowSystemUI();
                        UninstallKeyboardHook();
                        Application.Exit();
                    }
                    else
                    {
                        SystemSounds.Hand.Play();
                    }
                }
            };

            webView21.PreviewKeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F10)
                {
                    e.IsInputKey = true;
                    BeginInvoke(new Action(() =>
                    {
                        using var dlg = new PinPrompt();
                        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.PinText == _cfg.Pin)
                        {
                            _allowClose = true;
                            ShowSystemUI();
                            UninstallKeyboardHook();
                            Application.Exit();
                        }
                        else
                        {
                            SystemSounds.Hand.Play();
                        }
                    }));
                }
            };

            Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await webView21.EnsureCoreWebView2Async(null);

            // Handle popups / new windows
            webView21.CoreWebView2.NewWindowRequested += (s, ev) =>
            {
                var uri = ev.Uri;
                if (!_cfg.IsAllowed(uri))
                {
                    ev.Handled = true;   // block external popups
                    return;
                }

                // Allowed ? open in the same WebView instead of a new window
                ev.Handled = true;
                webView21.CoreWebView2.Navigate(uri);
            };

            // Cancel all downloads in kiosk
            webView21.CoreWebView2.DownloadStarting += (s, ev) => ev.Cancel = true;

            // Block navigating to non-allowed URLs (top-level navigations)
            webView21.CoreWebView2.NavigationStarting += (s, ev) =>
            {
                if (!_cfg.IsAllowed(ev.Uri))
                    ev.Cancel = true;
            };

            // Lock down browser UI
            var s = webView21.CoreWebView2.Settings;
            s.AreDefaultContextMenusEnabled = false;
            s.AreDevToolsEnabled = false;
            s.IsStatusBarEnabled = false;
            s.IsZoomControlEnabled = false;

            // Also block right-click menu via JavaScript
            webView21.CoreWebView2.NavigationCompleted += async (_, __) =>
            {
                try
                {
                    await webView21.ExecuteScriptAsync(
                        "document.addEventListener('contextmenu', e => e.preventDefault(), {capture:true});"
                    );
                }
                catch { /* ignore transient nav errors */ }
            };

            // Start page from settings.cfg (or from setup input)
            webView21.CoreWebView2.Navigate(_cfg.Url);
        }
    }
}
