using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public class UsersPage : MasterPage<AppUser>
    {
        public override string PageTitle => "Users";
        public override string PageSubtitle => "Control who can sign in and what they can do";
        protected override string EntityName => "User";
        public UsersPage() => BuildUi();

        private static readonly List<KeyValuePair<int?, string>> Roles = new()
        {
            new((int)UserRole.Admin, "Admin — full access"),
            new((int)UserRole.Manager, "Manager — no user/settings access"),
            new((int)UserRole.Cashier, "Cashier — sales & customers only")
        };

        protected override List<ColumnDef<AppUser>> Columns() => new()
        {
            new("Full name", u => u.FullName, 2), new("Username", u => u.Username, 1.5f), new("Role", u => u.Role.ToString(), 1),
            new("Status", u => u.IsActive ? "Active" : "Disabled", 1),
            new("Last login", u => u.LastLoginDate == null ? "Never" : Fmt.DateTimeText(u.LastLoginDate.Value), 1.8f)
        };
        protected override async Task<List<AppUser>> LoadAsync()
        {
            await using var db = App.NewDb();
            return await db.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync();
        }
        protected override List<FieldDef> Fields(AppUser u) => new()
        {
            new() { Label = "Full name", Required = true, Get = () => u.FullName, Set = v => u.FullName = (string)v! },
            new() { Label = "Username", Required = true, Get = () => u.Username, Set = v => u.Username = (string)v! },
            new() { Label = "Role", Kind = FieldKind.Combo, Required = true, Options = Roles, Get = () => (int?)(int)u.Role, Set = v => u.Role = (UserRole)(int)v! },
            new() { Label = u.Id == 0 ? "Password" : "New password", Kind = FieldKind.Password, Required = u.Id == 0, Get = () => "", Set = v => u.NewPassword = (string?)v,
                    Hint = u.Id == 0 ? "At least 6 characters" : "Leave blank to keep the current password" },
            new() { Label = "Account is active", Kind = FieldKind.Bool, Get = () => u.IsActive, Set = v => u.IsActive = (bool)v! }
        };
        protected override string? Validate(AppUser u, bool isNew)
        {
            using var db = App.NewDb();
            if (db.Users.Any(x => x.Username == u.Username && x.Id != u.Id)) return "This username is already taken.";
            if (!string.IsNullOrEmpty(u.NewPassword) && u.NewPassword.Length < 6) return "Password must be at least 6 characters.";
            if (u.Id == Session.UserId && (!u.IsActive || u.Role != UserRole.Admin)) return "You cannot disable or demote your own account.";
            var otherAdmins = db.Users.Count(x => x.Id != u.Id && x.IsActive && x.Role == UserRole.Admin);
            if ((!u.IsActive || u.Role != UserRole.Admin) && otherAdmins == 0) return "At least one active Admin must remain.";
            return null;
        }
        protected override Task SaveAsync(AppUser u, bool isNew)
        {
            if (!string.IsNullOrEmpty(u.NewPassword)) u.PasswordHash = AuthService.Hash(u.NewPassword);
            u.NewPassword = null;
            return isNew ? Svc.AddAsync(u) : Svc.UpdateAsync(u);
        }
        protected override bool CanDelete(AppUser u, out string? reason)
        {
            using var db = App.NewDb();
            reason = null;
            if (u.Id == Session.UserId) reason = "You cannot delete your own account.";
            else if (u.Role == UserRole.Admin && !db.Users.Any(x => x.Id != u.Id && x.IsActive && x.Role == UserRole.Admin)) reason = "At least one active Admin must remain.";
            return reason == null;
        }
    }
}
