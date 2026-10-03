using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class CategoriesPage : MasterPage<Category>
    {
        public override string PageTitle => "Categories";
        public override string PageSubtitle => "Group your products for easier browsing and reporting";
        protected override string EntityName => "Category";
        public CategoriesPage() => BuildUi();

        protected override List<ColumnDef<Category>> Columns() => new()
        {
            new("Name", c => c.Name, 2), new("Description", c => c.Description, 4),
            new("Products", c => { using var db = App.NewDb(); return db.Products.Count(p => p.CategoryId == c.Id); }, 1, true)
        };
        protected override async Task<List<Category>> LoadAsync()
        {
            await using var db = App.NewDb();
            return await db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        }
        protected override List<FieldDef> Fields(Category c) => new()
        {
            new() { Label = "Name", Required = true, Get = () => c.Name, Set = v => c.Name = (string)v! },
            new() { Label = "Description", Kind = FieldKind.Multiline, Get = () => c.Description, Set = v => c.Description = (string?)v }
        };
        protected override string? Validate(Category c, bool isNew)
        {
            using var db = App.NewDb();
            return db.Categories.Any(x => x.Name == c.Name && x.Id != c.Id) ? "A category with this name already exists." : null;
        }
        protected override bool CanDelete(Category c, out string? reason)
        {
            using var db = App.NewDb();
            var n = db.Products.Count(p => p.CategoryId == c.Id);
            reason = n > 0 ? $"This category is used by {n} product(s). Re-assign them first." : null;
            return n == 0;
        }
    }
}
