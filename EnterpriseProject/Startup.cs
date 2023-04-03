using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using EnterpriseProject.Data;
using EnterpriseProject.DbInitializer;
using EnterpriseProjectSendMailService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EnterpriseProject
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Đăng ký AppDbContext, sử dụng kết nối đến MS SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();
            
            // Đăng ký các dịch vụ của Identity
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            
            services.AddControllersWithViews();
            // Đăng ký Seed Date
            services.AddScoped<IDbInitializer, DbInitializer.DbInitializer>();

            // Cấu hình Cookie
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login"; // Url đến trang đăng nhập
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied"; // Trang khi User bị cấm truy cập
            });
           
            // Đăng ký dịch vụ Session
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian tồn tại của Session
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            
            // map ping mailsettings trong apsettings.js với class mailsettings
            services.AddOptions (); // Kích hoạt Options
            var mailsettings = Configuration.GetSection ("MailSettings"); // đọc config
            services.Configure<MailSettings> (mailsettings); // đăng ký để Inject
            // Đăng ký SendMailService với kiểu Transient, mỗi lần gọi dịch
            // vụ ISendMailService một đới tượng SendMailService tạo ra (đã inject config)
            services.AddTransient<ISendMailService, SendMailService.SendMailService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession(); // Đăng ký Middleware Session vào Pipeline

            app.UseAuthentication(); // bắt buộc đăng nhập thông tin (xác thực)
            app.UseAuthorization(); // phân quyền của User
            
            dbInitializer.Initialize(); 

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default", // đặt tên route
                    pattern: "{area=UnAuthenticated}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages(); // Đến Razor Page  
            });
        }
    }
}