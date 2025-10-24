using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Kiosk7
{
    // On-screen keyboard (numbers, letters, SPACE, BACKSPACE)
    // Grid-based so all keys stay visible at any size.
    public class KeyboardForm : Form
    {
        private readonly Action<string> _send;

        public KeyboardForm(Action<string> sendKeyCallback)
        {
            _send = sendKeyCallback;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.Black;
            // Bigger default size so it looks substantial
            Size = new Size(1280, 460);
            MinimumSize = new Size(900, 360);

            Shown += (_, __) =>
            {
                var screen = Screen.FromControl(this).WorkingArea;
                Left = screen.Left + (screen.Width - Width) / 2;
                Top = screen.Top + screen.Height - Height - 20;
            };

            BuildLayout();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Padding = new Padding(12),
                ColumnCount = 1,
                RowCount = 5,
            };
            // 5 rows with fixed height that scales ok on most DPIs
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76)); // numbers
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76)); // Q row
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76)); // A row
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76)); // Z row
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // bottom row (space/backspace)
            Controls.Add(root);

            string[] r1 = "1 2 3 4 5 6 7 8 9 0".Split(' ');
            string[] r2 = "Q W E R T Y U I O P".Split(' ');
            string[] r3 = "A S D F G H J K L".Split(' ');
            string[] r4 = "Z X C V B N M".Split(' ');

            root.Controls.Add(MakeRowTable(r1), 0, 0);
            root.Controls.Add(MakeRowTable(r2), 0, 1);
            root.Controls.Add(MakeRowTable(r3), 0, 2);
            root.Controls.Add(MakeRowTable(r4), 0, 3);
            root.Controls.Add(MakeBottomRow(), 0, 4);
        }

        private Control MakeRowTable(string[] keys)
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ColumnCount = keys.Length,
                RowCount = 1,
                Margin = new Padding(0, 6, 0, 6)
            };
            for (int i = 0; i < keys.Length; i++)
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / keys.Length));

            foreach (var k in keys)
            {
                var btn = MakeKey(k, k);
                btn.Dock = DockStyle.Fill;
                row.Controls.Add(btn);
            }
            return row;
        }

        private Control MakeBottomRow()
        {
            // 10 equal columns: SPACE spans 7, BACKSPACE spans 3
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ColumnCount = 10,
                RowCount = 1,
                Margin = new Padding(0, 6, 0, 0)
            };
            for (int i = 0; i < 10; i++)
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f));

            var space = MakeKey("SPACE", " ");
            space.Dock = DockStyle.Fill;
            row.Controls.Add(space, 0, 0);
            row.SetColumnSpan(space, 7);

            var back = MakeKey("⌫", "{BACKSPACE}");
            back.Dock = DockStyle.Fill;
            row.Controls.Add(back, 7, 0);
            row.SetColumnSpan(back, 3);

            return row;
        }

        private Button MakeKey(string label, string sendSpec)
        {
            var btn = new Button
            {
                Text = label,
                Tag = sendSpec,
                Margin = new Padding(6),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                UseCompatibleTextRendering = true
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.Click += (_, __) =>
            {
                _send((string)btn.Tag);
            };
            return btn;
        }
    }
}
