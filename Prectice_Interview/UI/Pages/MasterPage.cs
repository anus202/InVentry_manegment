using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public abstract class MasterPage<T> : PageBase where T : class, new()
    {
        protected readonly DataGridView Grid = new() { Dock = DockStyle.Fill };
        protected readonly InputBox Search = UIx.Input("Search...", 280);
        protected readonly FlowLayoutPanel Toolbar = new() { Dock = DockStyle.Top, Height = 56, Padding = new Padding(0, 0, 0, 10), WrapContents = false, BackColor = Theme.Background };
        protected readonly FlatButton BtnAdd = UIx.Button("＋  Add New", BtnKind.Primary, width: 130);
        protected readonly FlatButton BtnEdit = UIx.Button("Edit", BtnKind.Secondary, width: 90);
        protected readonly FlatButton BtnDelete = UIx.Button("Delete", BtnKind.Danger, width: 100);
        protected readonly FlatButton BtnExport = UIx.Button("Export CSV", BtnKind.Secondary, width: 120);
        protected readonly Label CountLabel = UIx.Label("", Theme.Small, Theme.Muted);
        protected readonly GenaricService Svc = App.Get<GenaricService>();
        private List<T> _items = new();
        private List<ColumnDef<T>> _columns = new();
        private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = 250 };

        protected abstract string EntityName { get; }
        protected abstract List<ColumnDef<T>> Columns();
        protected abstract Task<List<T>> LoadAsync();
        protected abstract List<FieldDef> Fields(T item);
        protected virtual string? Validate(T item, bool isNew) => null;
        protected virtual bool CanDelete(T item, out string? reason) { reason = null; return true; }
        protected virtual bool CanEdit => true;
        protected virtual Task SaveAsync(T item, bool isNew) => isNew ? Svc.AddAsync(item) : Svc.UpdateAsync(item);
        protected virtual Task AfterSaveAsync(T item, bool isNew) => Task.CompletedTask;
        protected virtual void StyleRow(DataGridViewRow row, T item) { }
        protected virtual bool Filter(T item) => true;
        protected virtual IEnumerable<Control> ExtraToolbar() => Array.Empty<Control>();
        protected virtual string? CloneForEdit(T src) => null;

        protected T? Selected => Grid.CurrentRow?.Tag as T;

        protected void BuildUi()
        {
            Padding = new Padding(24, 8, 24, 20);
            UIx.StyleGrid(Grid);
            _columns = Columns();
            foreach (var c in _columns) Grid.Col(c.Header, c.Weight, c.Right);

            Search.Margin = new Padding(0, 0, 12, 0);
            BtnAdd.Margin = new Padding(0, 0, 8, 0);
            BtnEdit.Margin = new Padding(0, 0, 8, 0);
            BtnDelete.Margin = new Padding(0, 0, 8, 0);
            Toolbar.Controls.Add(Search);
            Toolbar.Controls.Add(BtnAdd);
            Toolbar.Controls.Add(BtnEdit);
            Toolbar.Controls.Add(BtnDelete);
            foreach (var c in ExtraToolbar()) { c.Margin = new Padding(0, 0, 8, 0); Toolbar.Controls.Add(c); }
            BtnExport.Margin = new Padding(0, 0, 8, 0);
            Toolbar.Controls.Add(BtnExport);
            CountLabel.Margin = new Padding(8, 10, 0, 0);
            Toolbar.Controls.Add(CountLabel);

            var card = new Card { Dock = DockStyle.Fill, Padding = new Padding(1) };
            card.Controls.Add(Grid);
            Controls.Add(card);
            Controls.Add(Toolbar);

            BtnAdd.Click += async (_, _) => await Guard(() => EditAsync(null));
            BtnEdit.Click += async (_, _) => await Guard(EditSelectedAsync);
            BtnDelete.Click += async (_, _) => await Guard(DeleteAsync);
            BtnExport.Click += (_, _) => CsvExporter.Export(Grid, EntityName);
            Grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0 && CanEdit) await Guard(EditSelectedAsync); };
            _searchTimer.Tick += (_, _) => { _searchTimer.Stop(); Render(); };
            Search.TextChanged += (_, _) => { _searchTimer.Stop(); _searchTimer.Start(); };
            Grid.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter && CanEdit) { e.Handled = true; await Guard(EditSelectedAsync); }
                if (e.KeyCode == Keys.Delete) { e.Handled = true; await Guard(DeleteAsync); }
            };
            BtnAdd.Visible = BtnEdit.Visible = BtnDelete.Visible = CanEdit;
        }

        public override async Task RefreshAsync()
        {
            await Guard(async () =>
            {
                _items = await LoadAsync();
                Render();
            });
        }

        private void Render()
        {
            var term = Search.Text.Trim();
            var keepId = Grid.CurrentRow?.Index ?? 0;
            Grid.SuspendLayout();
            Grid.Rows.Clear();
            var shown = 0;
            foreach (var it in _items.Where(Filter))
            {
                var vals = _columns.Select(c => c.Value(it)).ToArray();
                if (term.Length > 0 && !vals.Any(v => (v?.ToString() ?? "").Contains(term, StringComparison.OrdinalIgnoreCase))) continue;
                var idx = Grid.Rows.Add(vals);
                Grid.Rows[idx].Tag = it;
                StyleRow(Grid.Rows[idx], it);
                shown++;
            }
            if (Grid.Rows.Count > 0) Grid.CurrentCell = Grid.Rows[Math.Min(keepId, Grid.Rows.Count - 1)].Cells[0];
            Grid.ResumeLayout();
            CountLabel.Text = $"{shown} {EntityName.ToLower()}{(shown == 1 ? "" : "s")}";
        }

        private Task EditSelectedAsync()
        {
            if (Selected == null) { UIx.Error($"Select a {EntityName.ToLower()} first."); return Task.CompletedTask; }
            return EditAsync(Selected);
        }

        protected async Task EditAsync(T? existing)
        {
            var isNew = existing == null;
            if (!isNew && existing == null) return;
            var item = existing ?? new T();
            using var dlg = new EntityDialog($"{(isNew ? "Add" : "Edit")} {EntityName}", Fields(item), () => Validate(item, isNew));
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK)
            {
                if (!isNew) await RefreshAsync();
                return;
            }
            await SaveAsync(item, isNew);
            await AfterSaveAsync(item, isNew);
            await RefreshAsync();
            UIx.Toast(this, $"{EntityName} saved");
        }

        private async Task DeleteAsync()
        {
            var item = Selected;
            if (item == null) { UIx.Error($"Select a {EntityName.ToLower()} first."); return; }
            if (!CanDelete(item, out var reason)) { UIx.Error(reason ?? "This record cannot be deleted."); return; }
            if (!UIx.Confirm($"Delete this {EntityName.ToLower()}? This cannot be undone from the app.")) return;
            await Svc.DeleteAsync(item);
            await RefreshAsync();
            UIx.Toast(this, $"{EntityName} deleted");
        }
    }
}
