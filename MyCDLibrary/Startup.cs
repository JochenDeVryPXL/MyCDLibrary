using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCDLibrary.Entities;
using MyCDLibrary.Services;
using System;
using Microsoft.AspNetCore.Identity;
using MyCDLibrarySecurity.Data;

namespace MyCDLibrary
{
    public class Startup
    {
        private IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IQuoter, Quoter>();
            //services.AddScoped<IAlbumData, InMemoryAlbumData>();
            services.AddScoped<IAlbumData, SqlAlbumData>();
            services.AddMvc();
            services.AddDbContext<MyCDLibraryContext>(options =>
                options.UseSqlServer(_configuration.GetConnectionString("MyCDLibraryDatabase")));
            services.AddControllers();
            services.AddDbContext<AlbumSecurityContext>(options =>
            {
                options.UseSqlServer(_configuration.GetConnectionString("SecurityDatabase"));
            });
            services.AddIdentity<User, Role>(options =>
                {
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                    options.Lockout.MaxFailedAccessAttempts = 4;
                    options.Lockout.AllowedForNewUsers = true;

                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;

                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddEntityFrameworkStores<AlbumSecurityContext>()
                .AddDefaultTokenProviders();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IQuoter quoter)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWelcomePage(new WelcomePageOptions
            {
                Path = "/welcome"
            });

            //app.UseFileServer();
            //app.UseStaticFiles(); //this is included in app.UseFileserver()

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapGet("/quote", async context =>
                {
                    var greeting = quoter.GetQuoteOfTheDay();
                    if (env.IsDevelopment())
                    {
                        await context.Response.WriteAsync(env.EnvironmentName + "\n" + greeting);
                    }
                    else
                    {
                        await context.Response.WriteAsync(greeting);
                    }
                });
                endpoints.MapGet("/error", async context =>
                {
                    throw new Exception("This is an intentional error page of the Cd Library web app. Good god!");
                });
                endpoints.MapControllerRoute("Default", "{controller}/{action}/{id?}", new {controller = "Album", action = "Index"}); // enables use of MVC via conventional routes, template defined here
                endpoints.MapControllers(); // enables use of attribute based routing, is used when attribute like [Route("[Controller]/[Action]/{id:int?}")] is added to a controller
            });

            app.UseFileServer();
        }
    }
}
