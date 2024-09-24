using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.HttpOverrides;
using SkillBridge.CMS.Services;
using System.Text.Json;
using SkillBridge.Business.Command;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Query;
using SkillBridge.Business.Query.DataDownload;
using SkillBridge.Business.Util.SMTP;
using SkillBridge.CMS.Areas.Intake.Data;
using Taku.Core;
using SkillBridge.Business.Repository;
using Taku.Core.Command;
using SkillBridge.Business.Mapping;


namespace SkillBridge.CMS
{
    
    public class Startup
    {
        private readonly bool _dev;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _dev = env.IsDevelopment();
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //var connStr = $"SBConnectionString{(_dev ? "Test" : "Production")}";
            var connStr = $"SBConnectionStringProduction";

            /* BEFORE PUBLISHING ANYTHING, MAKE SURE THE APPROPRIATE DB CONNECTION IS UNCOMMENTED HERE, 
             * IF PUBLISHING TO PRODUCTION SITE, MAKE SURE HEADER REWRITE CODE ON LINE 168 IS UNCOMMENTED AS WELL */

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetValue<string>(connStr)));

            // Intake form context
            services.AddDbContext<Intake.Data.IntakeFormContext>(options =>
                options.UseSqlServer(Configuration.GetValue<string>(connStr)));


            #region Dependency Injection
            // commands
            services.AddScoped<ISerializeObjectCommand, SerializeObjectCommand>();
            services.AddScoped<IRenderDropDownJsFileCommand, RenderDropDownJsFileCommand>();


            // Mappings
            services.AddScoped<IDropDownDataMapping, DropDownDataMapping>();
            services.AddScoped<IRelatedOrganizationMapping, RelatedOrganizationMapping>();
            services.AddScoped<IRelatedOrganizationCollectionMapping, RelatedOrganizationCollectionMapping>();

            // Repositories
            services.AddScoped<ITemplateRepository, Intake.Data.TemplateRepository>();
            services.AddScoped<IFormRepository, FormRepository>();
          

            // Queries
            services.AddScoped<IDeliveryMethodQuery, DeliveryMethodQuery>();
            services.AddScoped<IMilitaryBranchCollectionQuery, MilitaryBranchCollectionQuery>();
            services.AddScoped<IOpportunityCollectionQuery, OpportunityCollectionQuery>();
            services.AddScoped<IProgramOrganizationCollectionQuery, ProgramOrganizationCollectionQuery>();
            services.AddScoped<IRelatedOrganizationCollectionQuery, RelatedOrganizationCollectionQuery>();
            services.AddScoped<IDropdownDataQuery, DropdownDataQuery>();

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
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddHttpContextAccessor();
            //services.AddTransient<IEmailSender, SMTP>();// Add email sending using mailkit
            
             var smtp = JsonSerializer.Deserialize<SMTPOptions>(Configuration.GetValue<string>("SMTP"));
           
            services.Configure<SMTPOptions>(options =>
            {
                options.Server = smtp.Server;
                options.Port = smtp.Port;
                options.Account = smtp.Account;
                options.Password = smtp.Password;
                options.SenderEmail = smtp.SenderEmail;
                options.SenderName = smtp.SenderName;
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

            if (_dev)
            {
                app.UseDeveloperExceptionPage();
                app.UseForwardedHeaders();
                app.UseDatabaseErrorPage();
            }
            else
            {
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // MAKE SURE THIS CODE IS PUBLISHING IN RELEASE MODE FOR THIS TO TAKE EFFECT
                app.Use(async (context, next) =>
                {
                    string originalHost = string.Empty;
                    if (context.Request.Headers.TryGetValue("X-Original-Host", out var traceValue))
                    {
                        originalHost = traceValue;
                    }

                    if (originalHost != "skillbridge-cms-test.azurewebsites.us")
                    {
                        //var originalHost = context.Request.Headers.GetValues("X-Original-Host").FirstOrDefault();
                        //context.Request.Headers.Add("Host", "skillbridge.org");
                        context.Request.Headers.Add("OriginalHost", originalHost);   
                        //context.Request.Headers.Set("Host", originalHost);
                    }
                    await next.Invoke();
                });
                
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
                    pattern: "intake/{controller=IntakeHome}/{action=index}/{zohoTicketId?}"
                );

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}",
                    defaults: new { @namespace = "SkillBridge.CMS.Controllers" });

                endpoints.MapRazorPages();
            });
        }
    }
}
