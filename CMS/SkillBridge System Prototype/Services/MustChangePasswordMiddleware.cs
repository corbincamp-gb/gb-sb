using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SkillBridge_System_Prototype.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SkillBridge_System_Prototype.Services
{
    public class MustChangePasswordMiddleware
    {
        private readonly RequestDelegate _next;

        //private readonly UserManager<ApplicationUser> _userManager;

        public MustChangePasswordMiddleware(RequestDelegate next/*, UserManager<ApplicationUser> userManager*/)
        {
            _next = next;
            //_userManager = userManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //string UserName = context.User.Identity.Name;
            //var userid = _userManager.GetUserId(context.User); // Get user id:
            //var user = await _userManager.FindByIdAsync(userid);

            //string id = user.ProgramId.ToString();

            // Get user from login email
            //user = await _userManager.FindByEmailAsync(Input.Email);

            // If email doesnt work, try username instead
            //if (user == null)
            //{
                //user = await _userManager.FindByNameAsync(Input.Email);
            //}

            if (context.User != null)
            {
                // Force user to password screen after login
                //if (context.User.MustChangePassword)
                //{
                if (context.User.Identity.IsAuthenticated)
               // context.Request.Path != new PathString("/account/logout") &&
                //)
                {
                    if(context.Request.Path != new PathString("/Identity/Account/Manage/ChangePassword"))
                    {
                        if(((ClaimsIdentity)context.User.Identity).HasClaim(c => c.Type == "http://userswithoutidentity/claims/mustchangepassword"))
                        {

                            var returnUrl = context.Request.Path.Value == "/" ? "" : "?returnUrl=" + HttpUtility.UrlEncode(context.Request.Path.Value);
                            context.Response.Redirect("/Identity/Account/Manage/ChangePassword"/* + returnUrl*/);
                        }
                    }
                    
                    //return LocalRedirect("/Identity/Account/Manage/ChangePassword");
                }
            }

            /*if (context.User.Identity.IsAuthenticated && context.User.Identity.MustChangePassword)
            {
                var returnUrl = context.Request.Path.Value == "/" ? "" : "?returnUrl=" + HttpUtility.UrlEncode(context.Request.Path.Value);
                context.Response.Redirect("/Identity/Account/Manage/ChangePassword" + returnUrl);
            }*/
            await _next(context).ConfigureAwait(true);
        }
    }

    public static class MustChangePasswordMiddlewareExtensions
    {
        public static IApplicationBuilder UseMustChangePassword(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MustChangePasswordMiddleware>();
        }
    }
}


