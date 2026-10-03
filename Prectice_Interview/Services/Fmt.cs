using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public static class Fmt
    {
        public static string Currency { get; set; } = "Rs.";
        public static string Money(decimal v) => $"{Currency} {v.ToString("N2", CultureInfo.InvariantCulture)}";
        public static string Date(DateTime d) => d.ToString("dd MMM yyyy");
        public static string DateTimeText(DateTime d) => d.ToString("dd MMM yyyy  hh:mm tt");
    }
}
