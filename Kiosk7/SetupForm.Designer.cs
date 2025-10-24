// SetupForm.Designer.cs
using System.Windows.Forms;

namespace Kiosk7
{
    partial class SetupForm
    {
        private System.ComponentModel.IContainer components = null;

        private PictureBox headerIcon;
        private Label headerTitle;
        private Label headerSubtitle;

        private Label lblUrl;
        private TextBox tbUrl;

        private Label lblPin;
        private TextBox tbPin;   // visible PIN

        private Label lblAllow;
        private TextBox tbAllow; // multiline allowlist

        private CheckBox cbShowExit;
        private CheckBox cbRemember;

        private Button btnCancel;
        private Button btnSaveStart;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            headerIcon = new PictureBox();
            headerTitle = new Label();
            headerSubtitle = new Label();

            lblUrl = new Label();
            tbUrl = new TextBox();

            lblPin = new Label();
            tbPin = new TextBox();

            lblAllow = new Label();
            tbAllow = new TextBox();

            cbShowExit = new CheckBox();
            cbRemember = new CheckBox();

            btnCancel = new Button();
            btnSaveStart = new Button();

            SuspendLayout();

            // Form
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(720, 420);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Kiosk Setup";
            TopMost = true;

            // Header icon
            headerIcon.Location = new System.Drawing.Point(16, 16);
            headerIcon.Size = new System.Drawing.Size(32, 32);
            headerIcon.SizeMode = PictureBoxSizeMode.StretchImage;

            // Header title
            headerTitle.AutoSize = true;
            headerTitle.Location = new System.Drawing.Point(56, 16);
            headerTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            headerTitle.Text = "Configure Kiosk";

            // Header subtitle
            headerSubtitle.AutoSize = true;
            headerSubtitle.Location = new System.Drawing.Point(58, 48);
            headerSubtitle.Size = new System.Drawing.Size(480, 15);
            headerSubtitle.Text = "Enter the website, PIN, and allowed domains";

            // URL
            lblUrl.AutoSize = true;
            lblUrl.Location = new System.Drawing.Point(16, 78);
            lblUrl.Text = "Start URL";
            tbUrl.Location = new System.Drawing.Point(16, 96);
            tbUrl.Size = new System.Drawing.Size(688, 23);
            tbUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // PIN (visible)
            lblPin.AutoSize = true;
            lblPin.Location = new System.Drawing.Point(16, 130);
            lblPin.Text = "Exit PIN (visible)";
            tbPin.Location = new System.Drawing.Point(16, 148);
            tbPin.Size = new System.Drawing.Size(180, 23);
            tbPin.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            tbPin.UseSystemPasswordChar = false; // <- visible

            // Allowlist
            lblAllow.AutoSize = true;
            lblAllow.Location = new System.Drawing.Point(16, 184);
            lblAllow.Text = "Allowlist (comma or new line)";
            tbAllow.Location = new System.Drawing.Point(16, 202);
            tbAllow.Size = new System.Drawing.Size(688, 150);
            tbAllow.Multiline = true;
            tbAllow.ScrollBars = ScrollBars.Vertical;
            tbAllow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            // Checkboxes
            cbShowExit.AutoSize = true;
            cbShowExit.Location = new System.Drawing.Point(16, 362);
            cbShowExit.Text = "Show red EXIT button (for testing)";
            cbShowExit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            cbRemember.AutoSize = true;
            cbRemember.Location = new System.Drawing.Point(260, 362);
            cbRemember.Text = "Remember these settings for next time";
            cbRemember.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // Buttons
            btnCancel.Text = "Cancel";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Size = new System.Drawing.Size(90, 28);
            btnCancel.Location = new System.Drawing.Point(514, 360);
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            btnSaveStart.Text = "Save Start";
            btnSaveStart.Size = new System.Drawing.Size(90, 28);
            btnSaveStart.Location = new System.Drawing.Point(614, 360);
            btnSaveStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSaveStart.Click += btnSaveStart_Click;

            // Accept/Cancel
            AcceptButton = btnSaveStart;
            CancelButton = btnCancel;

            // Add controls
            Controls.Add(headerIcon);
            Controls.Add(headerTitle);
            Controls.Add(headerSubtitle);
            Controls.Add(lblUrl);
            Controls.Add(tbUrl);
            Controls.Add(lblPin);
            Controls.Add(tbPin);
            Controls.Add(lblAllow);
            Controls.Add(tbAllow);
            Controls.Add(cbShowExit);
            Controls.Add(cbRemember);
            Controls.Add(btnCancel);
            Controls.Add(btnSaveStart);

            ResumeLayout(false);
            PerformLayout();
        }
    }
}
