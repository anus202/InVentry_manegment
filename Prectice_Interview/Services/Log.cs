using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public static class Log
    {
        private static readonly object Gate = new();
        public static string Folder => Path.Combine(AppContext.BaseDirectory, "logs");

        public static void Error(Exception ex, string? context = null)
        {
            try
            {
                lock (Gate)
                {
                    Directory.CreateDirectory(Folder);
                    var file = Path.Combine(Folder, $"{DateTime.Now:yyyy-MM-dd}.log");
                    File.AppendAllText(file, $"[{DateTime.Now:HH:mm:ss}] {context}\r\n{ex}\r\n\r\n");
                }
            }
            catch {  }
        }
    }
}
