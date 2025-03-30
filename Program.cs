using System.Security.Claims;
using McqTask.Helpers;
using McqTask.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace McqTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var environment = builder.Environment.EnvironmentName; // Get the current environment

            // ✅ Use DbContextFactory for other parts of the app (like Controllers)
            builder.Services.AddDbContextFactory<ExamContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

            // ✅ Use AddDbContext for Identity (Required)
            builder.Services.AddDbContext<ExamContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));


            // ✅ Configure Identity (This now works)
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ExamContext>() // This requires AddDbContext, not AddDbContextFactory
                .AddDefaultTokenProviders();


            // ✅ Add Controllers and Views
            builder.Services.AddControllersWithViews();

            // ✅ Add Authentication Middleware
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // ✅ Add Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(240);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddScoped<StudentService>();

            var app = builder.Build();

            // ✅ Configure Error Handling
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();


            // ✅ Enable Authentication and Authorization
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
       

            // ✅ Define Routes
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
