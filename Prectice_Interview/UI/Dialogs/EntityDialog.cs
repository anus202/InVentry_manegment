namespace Prectice_Interview.UI
{
    public class EntityDialog : DialogBase
    {
        private readonly List<FieldDef> _fields;
        private readonly Dictionary<FieldDef, Control> _controls = new();
        private readonly Func<string?>? _validate;

        public EntityDialog(string title, List<FieldDef> fields, Func<string?>? validate = null)
            : base(title, "Fill in the details below", new Size(540, 170 + fields.Sum(f => f.Kind == FieldKind.Multiline ? 112 : f.Kind == FieldKind.Bool ? 40 : 84)))
        {
            _fields = fields;
            _validate = validate;
            var maxH = Screen.FromControl(this).WorkingArea.Height - 80;
            if (Height > maxH) Height = maxH;

            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = Theme.Background };
            Body.Controls.Add(flow);

            foreach (var f in fields)
            {
                var host = new Panel { Width = 440, Height = f.Kind == FieldKind.Multiline ? 106 : f.Kind == FieldKind.Bool ? 34 : 80, Margin = new Padding(0, 0, 0, 6), BackColor = Theme.Background };
                Control c;
                var value = f.Get();
                if (f.Kind != FieldKind.Bool)
                    host.Controls.Add(new Label { Text = f.Label + (f.Required ? " *" : ""), Font = Theme.Small, ForeColor = Theme.Muted, AutoSize = false, Height = 20, Width = 440, Top = 0 });

                switch (f.Kind)
                {
                    case FieldKind.Bool:
                        c = new CheckBox { Text = f.Label, Checked = value is true, Font = Theme.Base, AutoSize = true, ForeColor = Theme.Text, Top = 6 };
                        break;
                    case FieldKind.Decimal:
                    case FieldKind.Int:
                        var n = UIx.Number(0, f.Max, f.Kind == FieldKind.Int ? 0 : 2, 440);
                        n.Top = 24; n.Height = 34;
                        n.Value = Convert.ToDecimal(value ?? 0m);
                        c = n;
                        break;
                    case FieldKind.Combo:
                        var cb = UIx.Combo(440);
                        cb.Top = 24;
                        cb.DisplayMember = "Value";
                        cb.Items.AddRange(f.Options!.Cast<object>().ToArray());
                        var idx = f.Options.FindIndex(o => o.Key == (int?)value);
                        cb.SelectedIndex = idx >= 0 ? idx : 0;
                        c = cb;
                        break;
                    default:
                        var tb = UIx.Input("", 440);
                        tb.Top = 24;
                        tb.Text = value?.ToString() ?? "";
                        tb.Password = f.Kind == FieldKind.Password;
                        if (f.Kind == FieldKind.Multiline) { tb.Multiline = true; tb.Height = 78; }
                        c = tb;
                        break;
                }
                if (f.Kind != FieldKind.Bool) c.Width = 440;
                host.Controls.Add(c);
                if (!string.IsNullOrEmpty(f.Hint) && f.Kind != FieldKind.Multiline)
                    host.Controls.Add(new Label { Text = f.Hint, Font = new Font("Segoe UI", 8f), ForeColor = Theme.Muted, AutoSize = true, Top = 64 });
                _controls[f] = c;
                flow.Controls.Add(host);
            }

            BtnSave.Click += OnSave;
            Shown += (_, _) => (_controls.Values.FirstOrDefault(x => x is InputBox) as InputBox)?.Focus();
        }

        private static int? ComboKey(Control c) =>
            (c as ComboBox)?.SelectedItem is KeyValuePair<int?, string> item ? item.Key : null;

        private void OnSave(object? sender, EventArgs e)
        {
            foreach (var f in _fields)
            {
                if (f.Kind == FieldKind.Combo && f.Required && ComboKey(_controls[f]) == null)
                { UIx.Error($"{f.Label} is required."); return; }
                if (f.Required && f.Kind is FieldKind.Text or FieldKind.Multiline or FieldKind.Password
                    && string.IsNullOrWhiteSpace((_controls[f] as InputBox)?.Text))
                { UIx.Error($"{f.Label} is required."); (_controls[f] as InputBox)?.Focus(); return; }
            }
            foreach (var f in _fields)
            {
                var c = _controls[f];
                object? v = f.Kind switch
                {
                    FieldKind.Bool => ((CheckBox)c).Checked,
                    FieldKind.Decimal => ((NumericUpDown)c).Value,
                    FieldKind.Int => (int)((NumericUpDown)c).Value,
                    FieldKind.Combo => ComboKey(c),
                    _ => ((InputBox)c).Text.Trim()
                };
                f.Set(v);
            }
            var err = _validate?.Invoke();
            if (err != null) { UIx.Error(err); return; }
            DialogResult = DialogResult.OK;
        }
    }
}
