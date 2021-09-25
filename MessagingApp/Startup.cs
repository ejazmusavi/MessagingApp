using MessagingApp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessagingApp
{
    public class Startup
    {
        private IWebHostEnvironment _env;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var file = System.IO.Path.Combine(_env.WebRootPath, @"data/message.db");
            var con = "Data Source="+file;
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(con));
            services.AddDatabaseDeveloperPageExceptionFilter();
            //services.AddEntityFrameworkSqlite().AddDbContext<ApplicationDbContext>();
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false).AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<IdentityUser> _user, RoleManager<IdentityRole> _role)
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
            SeedUser(_user, _role).Wait();
        }

        private async Task SeedUser(UserManager<IdentityUser> _user, RoleManager<IdentityRole> _role)  {
            if(!_role.Roles.Any())
                {
                await _role.CreateAsync(new IdentityRole { Name = "Admin" });
            }

            if (!_user.Users.Any())
            {
                var user = new IdentityUser { UserName = "admin", Email = "admin@app.com" };
               var result=await _user.CreateAsync(user);
                if (result.Succeeded)
                {
                  var res=  await _user.AddPasswordAsync(user, "Abc123!@#");
                    await _user.AddToRoleAsync(user, "Admin");
                }
            }

        
        }
    }
}
