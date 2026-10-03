using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public static class Session
    {
        public static AppUser? User { get; private set; }
        public static int? UserId => User?.Id;
        public static string UserName => User?.FullName ?? "System";
        public static bool IsAdmin => User?.Role == UserRole.Admin;
        public static bool IsManagerOrAdmin => User?.Role is UserRole.Admin or UserRole.Manager;
        public static void SignIn(AppUser u) => User = u;
        public static void SignOut() => User = null;
    }
}
