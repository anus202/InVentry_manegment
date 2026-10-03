using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prectice_Interview.Data;
using Prectice_Interview.Form;
using Prectice_Interview.Service;

namespace Prectice_Interview
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var services = new ServiceCollection();

            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    "Server =.; Database = PrecticeInterviewDb; Trusted_Connection = True; TrustServerCertificate = True"
                ));

            // Service
            services.AddScoped<GenaricService>();

            // Form
            services.AddTransient<frmSignup>();

            var serviceProvider = services.BuildServiceProvider();

            ApplicationConfiguration.Initialize();

            var form = serviceProvider.GetRequiredService<frmSignup>();

            Application.Run(form);
        }
    }
}