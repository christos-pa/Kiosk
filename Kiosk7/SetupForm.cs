// SetupForm.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Kiosk7
{
    public partial class SetupForm : Form
    {
        public string SelectedUrl { get; private set; } = "";
        public string SelectedPin { get; private set; } = "";
        public List<string> SelectedAllowlist { get; private set; } = new();
        public bool SelectedShowExit { get; private set; } = false;
        public bool RememberSettings { get; private set; } = false;

        public SetupForm(string defaultUrl, string defaultPin, IEnumerable<string>? defaultAllowlist = null, bool defaultShowExit = false)
        {
            InitializeComponent();

            // Header icon at runtime
            headerIcon.Image = System.Drawing.SystemIcons.Information.ToBitmap();

            // Prefill fields
            tbUrl.Text = string.IsNullOrWhiteSpace(defaultUrl) ? "https://www.bbc.com" : defaultUrl;
            tbPin.Text = string.IsNullOrWhiteSpace(defaultPin) ? "1234" : defaultPin;
            tbAllow.Text = string.Join(Environment.NewLine, (defaultAllowlist ?? Array.Empty<string>()));
            cbShowExit.Checked = defaultShowExit;
            cbRemember.Checked = false; // user chooses
        }

        private void btnSaveStart_Click(object? sender, EventArgs e)
        {
            var url = tbUrl.Text.Trim();
            var pin = tbPin.Text.Trim();

            // Validate URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out var u) ||
                (u.Scheme != Uri.UriSchemeHttp && u.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show(this,
                    "Please enter a valid URL that starts with http:// or https://",
                    "Invalid URL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                tbUrl.Focus();
                tbUrl.SelectAll();
                return;
            }

            if (string.IsNullOrWhiteSpace(pin))
            {
                MessageBox.Show(this, "Please enter a PIN.", "PIN required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbPin.Focus();
                return;
            }

            // Parse allowlist (split by comma or new lines)
            var parts = tbAllow.Text
                .Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            SelectedUrl = url;
            SelectedPin = pin;
            SelectedAllowlist = parts;
            SelectedShowExit = cbShowExit.Checked;
            RememberSettings = cbRemember.Checked;

            DialogResult = DialogResult.OK;
        }
    }
}
