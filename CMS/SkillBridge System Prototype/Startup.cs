using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SkillBridge_System_Prototype.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkillBridge_System_Prototype.Util.MailKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using SkillBridge_System_Prototype.Models;
using Microsoft.AspNetCore.HttpOverrides;
using SkillBridge_System_Prototype.Services;
using System.Configuration;

namespace SkillBridge_System_Prototype
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
            /* BEFORE PUBLISHING ANYTHING, MAKE SURE THE APPROPRIATE DB CONNECTION IS UNCOMMENTED HERE, 
             * IF PUBLISHING TO PRODUCTION SITE, MAKE SURE HEADER REWRITE CODE ON LINE 168 IS UNCOMMENTED AS WELL */

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
#if DEBUG
                    //Configuration["ConnectionStrings:DefaultConnection"]
                    Configuration.GetValue<string>("SBConnectionStringTest")  // Current Test DB Server
#else
                    Configuration.GetValue<string>("SBConnectionStringProduction")  // Current Production DB Server
#endif
                    ));

            // Intake form context
            services.AddDbContext<Intake.Data.IntakeFormContext>(options =>
                options.UseSqlServer(
#if DEBUG
                    //Configuration["ConnectionStrings:DefaultConnection"]
                    Configuration.GetValue<string>("SBConnectionStringTest")  // Current Test DB Server
#else
                    Configuration.GetValue<string>("SBConnectionStringProduction") // Current Production DB Server
#endif
                    ));


#region Dependency Injection

            services.AddScoped<Intake.Data.ITemplateRepository, Intake.Data.TemplateRepository>();
            services.AddScoped<Intake.Data.IFormRepository, Intake.Data.FormRepository>();

#endregion


            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;

                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddHttpContextAccessor();
            services.AddTransient<IEmailSender, MailKitEmailSender>();// Add email sending using mailkit
            //Console.WriteLine("Configuration.GetValue<string>('SMTP - Account'): " + Configuration.GetValue<string>("SMTP-Account"));
            //Console.WriteLine("Configuration.GetValue<string>('SMTP - Password'): " + Configuration.GetValue<string>("SMTP-Password"));
            services.Configure<MailKitEmailSenderOptions>(options =>
            {
                options.Host_Address = Configuration["ExternalProviders:MailKit:SMTP:Address"];
                options.Host_Port = Convert.ToInt32(Configuration["ExternalProviders:MailKit:SMTP:Port"]);
                options.Host_Username = Configuration.GetValue<string>("SMTP-Account");//Configuration["ExternalProviders:MailKit:SMTP:Account"];
                options.Host_Password = Configuration.GetValue<string>("SMTP-Password");//Configuration["ExternalProviders:MailKit:SMTP:Password"];
                options.Sender_EMail = Configuration["ExternalProviders:MailKit:SMTP:SenderEmail"];
                options.Sender_Name = Configuration["ExternalProviders:MailKit:SMTP:SenderName"];
            });

            // Prevent wrong URL on live site
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                var url = context.Request.Path.Value;
                var domain = context.Request.Host.Value;

                //Console.WriteLine("domain: " + domain);
                //Console.WriteLine("url: " + url);

                // Rewrite to index
                if (url.Contains("/home/privacy"))
                {
                    // rewrite and continue processing
                    context.Request.Path = "/home/index";
                }

                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseForwardedHeaders();
                app.UseDatabaseErrorPage();
            }
            else
            {
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // MAKE SURE THIS CODE IS PUBLISHING IN RELEASE MODE FOR THIS TO TAKE EFFECT
#if !DEBUG
                app.Use(async (context, next) =>
                {
                    string originalHost = string.Empty;
                    if (context.Request.Headers.TryGetValue("X-Original-Host", out var traceValue))
                    {
                        originalHost = traceValue;
                    }

                    if (originalHost != "SkillBridge-Frontend.azurefd.us" && originalHost != "skillbridge-cms-test.azurewebsites.us")
                    {
                        //var originalHost = context.Request.Headers.GetValues("X-Original-Host").FirstOrDefault();
                        context.Request.Headers.Add("Host", "skillbridge.org");
                        context.Request.Headers.Add("OriginalHost", originalHost);   
                        //context.Request.Headers.Set("Host", originalHost);
                    }
                    await next.Invoke();
                });
#endif
                
                app.UseExceptionHandler("/Home/Error");
                app.UseForwardedHeaders();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Redirects a user to the MustChangePassword page when user has a MustChangePassword claim.
            app.UseMustChangePassword();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "intake-form-main",
                    pattern: "intake/{controller}/{action=introduction}/{zohoTicketId}",
                    defaults: new { area = "intake", controller = "form", action = "introduction" });

                endpoints.MapControllerRoute(
                    name: "intake-form-part",
                    pattern: "intake/{controller}/{zohoTicketId}/part{partId}/{id?}",
                    defaults: new { area = "intake", controller = "form", action = "part" });

                endpoints.MapAreaControllerRoute(
                    name: "intake",
                    areaName: "intake",
                    pattern: "intake/{controller=intakehome}/{action=index}/{zohoTicketId?}"
                );

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}",
                    defaults: new { @namespace = "SkillBridge_System_Prototype.Controllers" });

                endpoints.MapRazorPages();
            });
        }
    }
}
