using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace Microcosm
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Empty
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Use Directory Browser?
            string useDirectoryBrowser = Environment.GetEnvironmentVariable("UseDirectoryBrowser");
            useDirectoryBrowser = useDirectoryBrowser?.ToLower();
            if (useDirectoryBrowser == "true")
            {
                app.UseDirectoryBrowser();
            }

            // Use Developer Exception Page
            app.UseDeveloperExceptionPage();

            // Use Routing
            app.UseRouting();

            // Use Default Files
            app.UseDefaultFiles();

            // Use Static Files
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ContentLog.Log
            });

            // Use Endpoints
            app.UseEndpoints(endpoints =>
            {
                // Portal UI
                endpoints.Map("/portal", async context =>
                {
                    // Html
                    string html = await File.ReadAllTextAsync("Portal.html");

                    // Message
                    string message = string.Empty;
                    if (context.Request.Query.ContainsKey("message"))
                    {
                        message = context.Request.Query["message"];
                        message = WebUtility.HtmlEncode(message);
                        html = html.Replace("{{message}}", message);
                    }
                    else
                    {
                        html = html.Replace("{{message}}", string.Empty);
                    }

                    // Log
                    StringBuilder logBuilder = new StringBuilder();
                    if (ContextHelper.AuthForm(context))
                    {
                        logBuilder.AppendLine($"<p>Uptime: {ContentLog.GetUptime()}</p>");
                        logBuilder.AppendLine($"<p>Count: {ContentLog.Lines.Count}</p>");
                        foreach (string line in ContentLog.Lines)
                        {
                            logBuilder.AppendLine($"<p>{line}</p>");
                        }
                    }
                    html = html.Replace("{{log}}", logBuilder.ToString());

                    // Response
                    await context.Response.WriteAsync(html);
                });

                // Portal Content POST
                endpoints.MapPost("/portal/content", async context =>
                {
                    // Auth
                    if (!ContextHelper.AuthForm(context))
                    {
                        ContextHelper.Deny(context, "/portal");
                        return;
                    }

                    // Run
                    IFormFile zipFile = context.Request.Form.Files["zip"];
                    if (zipFile != null)
                    {
                        if (!Directory.Exists("wwwroot"))
                        {
                            Directory.CreateDirectory("wwwroot");
                        }
                        if (context.Request.Form.ContainsKey("replace"))
                        {
                            DirectoryInfo directory = new DirectoryInfo("wwwroot");
                            foreach (FileInfo file in directory.GetFiles())
                            {
                                file.Delete();
                            }
                            foreach (DirectoryInfo dir in directory.GetDirectories())
                            {
                                dir.Delete(true);
                            }
                        }
                        using (FileStream fileStream = File.Create("wwwroot.zip"))
                        {
                            await zipFile.CopyToAsync(fileStream);
                        }
                        ZipFile.ExtractToDirectory("wwwroot.zip", "wwwroot", true);
                        ContextHelper.Redirect(context, "/portal", "Success!");
                        return;
                    }
                    else
                    {
                        ContextHelper.Redirect(context, "/portal", "No Zip File");
                        return;
                    }
                });
            });
        }
    }
}
