using System.Drawing;
using System.Windows.Forms;

namespace Kiosk7
{
    public class PinPrompt : Form
    {
        private readonly TextBox _tb;
        public string PinText => _tb.Text;

        public PinPrompt()
        {
            Text = "Exit kiosk";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = MaximizeBox = false;
            Size = new Size(300, 140);
            TopMost = true;                 // ensure on top of kiosk

            var lbl = new Label { Text = "Enter PIN:", Left = 15, Top = 18, Width = 260 };
            _tb = new TextBox { Left = 15, Top = 45, Width = 260, UseSystemPasswordChar = true };

            var ok = new Button { Text = "OK", Left = 115, Width = 75, Top = 80, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Cancel", Left = 200, Width = 75, Top = 80, DialogResult = DialogResult.Cancel };

            Controls.Add(lbl); Controls.Add(_tb); Controls.Add(ok); Controls.Add(cancel);
            AcceptButton = ok; CancelButton = cancel;
        }
    }
}
