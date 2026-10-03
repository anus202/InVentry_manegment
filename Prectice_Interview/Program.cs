using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prectice_Interview.Data;
using Prectice_Interview.Services;
using Prectice_Interview.UI;

namespace Prectice_Interview
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) => ShowFatal(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => Log.Error((Exception)e.ExceptionObject, "Unhandled");

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            var connectionString = config.GetConnectionString("Default")
                ?? throw new InvalidOperationException("ConnectionStrings:Default is missing in appsettings.json");

            var services = new ServiceCollection();
            services.AddDbContextFactory<ApplicationDbContext>(o => o.UseSqlServer(connectionString));

            services.AddSingleton<GenaricService>();
            services.AddSingleton<AuthService>();
            services.AddSingleton<SettingsService>();
            services.AddSingleton<InventoryService>();
            services.AddSingleton<ReportService>();

            foreach (var page in new[]
            {
                typeof(DashboardPage), typeof(PosPage), typeof(SalesPage), typeof(CustomersPage), typeof(ProductsPage), typeof(CategoriesPage),
                typeof(StockPage), typeof(PurchasesPage), typeof(SuppliersPage), typeof(ReportsPage), typeof(UsersPage), typeof(SettingsPage)
            })
                services.AddTransient(page);

            App.Services = services.BuildServiceProvider();

            try
            {
                using (var db = App.NewDb()) db.Database.Migrate();
                App.Get<SettingsService>().LoadAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Startup");
                MessageBox.Show("Could not connect to or create the database.\r\n\r\n" + (ex.InnerException?.Message ?? ex.Message) +
                                "\r\n\r\nCheck the connection string in appsettings.json and that SQL Server is running.",
                    "Inventory Management", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            while (true)
            {
                using (var login = new LoginForm())
                    if (login.ShowDialog() != DialogResult.OK) break;

                using var main = new MainForm();
                Application.Run(main);
                Session.SignOut();
                if (!main.LoggedOut) break;
            }
        }

        private static void ShowFatal(Exception ex)
        {
            Log.Error(ex, "UI thread");
            MessageBox.Show("An unexpected error occurred, but your data is safe.\r\n\r\n" + ex.Message + "\r\n\r\nDetails were saved to the logs folder.",
                "Inventory Management", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
