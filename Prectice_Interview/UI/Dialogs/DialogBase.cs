namespace Prectice_Interview.UI
{
    public class DialogBase : Form
    {
        protected readonly Panel Body = new() { Dock = DockStyle.Fill, BackColor = Theme.Background, Padding = new Padding(24, 16, 24, 8), AutoScroll = true };
        protected readonly FlatButton BtnSave = UIx.Button("Save", BtnKind.Primary, width: 120);
        protected readonly FlatButton BtnCancel = UIx.Button("Cancel", BtnKind.Secondary, width: 100);

        public DialogBase(string title, string subtitle, Size size)
        {
            Text = title;
            Size = size;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Theme.Background;
            Font = Theme.Base;
            KeyPreview = true;

            var header = new Panel { Dock = DockStyle.Top, Height = 74, BackColor = Color.White, Padding = new Padding(24, 12, 24, 8) };
            header.Controls.Add(new Label { Text = subtitle, Font = Theme.Small, ForeColor = Theme.Muted, Dock = DockStyle.Bottom, Height = 22 });
            header.Controls.Add(new Label { Text = title, Font = Theme.H2, ForeColor = Theme.Text, Dock = DockStyle.Fill });
            header.Paint += (_, e) => e.Graphics.DrawLine(new Pen(Theme.Border), 0, header.Height - 1, header.Width, header.Height - 1);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 64, FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(24, 12, 24, 8), BackColor = Color.White
            };
            footer.Controls.Add(BtnSave);
            footer.Controls.Add(BtnCancel);
            BtnCancel.DialogResult = DialogResult.Cancel;
            CancelButton = BtnCancel;

            Controls.Add(Body);
            Controls.Add(footer);
            Controls.Add(header);
        }
    }
}
