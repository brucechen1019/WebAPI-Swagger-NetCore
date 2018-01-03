using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebAPI_Swagger_NetCore.Swagger;

namespace WebAPI_Swagger_NetCore
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
            services.AddMvc();

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });

                // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
                c.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = string.Format(CultureInfo.InvariantCulture, "https://login.microsoftonline.com/{0}/oauth2/authorize", Configuration["AzureAd:TenantId"]),
                    Scopes = new Dictionary<string, string>
                        {
                            { "user_impersonation", "Access Bruce-WebAPI-NetCore" }
                        }
                });
                // Enable operation filter based on AuthorizeAttribute
                c.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Configure the app to use Jwt Bearer Authentication
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.Authority = string.Format(CultureInfo.InvariantCulture, "https://sts.windows.net/{0}/", Configuration["AzureAd:TenantId"]);
                jwtOptions.Audience = Configuration["AzureAd:WebApiApp:ClientId"];
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.ConfigureOAuth2(
                    Configuration["AzureAd:SwaggerApp:ClientId"],
                    Configuration["AzureAd:SwaggerApp:ClientSecret"],
                    Configuration["AzureAd:SwaggerApp:RedirectUri"],
                    "Bruce-WebAPI-NetCore-Swagger",
                    additionalQueryStringParameters: new Dictionary<string, string>()
                    {
                       { "resource",Configuration["AzureAd:WebApiApp:ClientId"]}
                    });
            });

            app.UseAuthentication();

            app.UseMvc();
        }
    }

}
