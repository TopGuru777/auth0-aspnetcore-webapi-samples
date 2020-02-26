using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth0.AuthenticationApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UserInfo
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
            // If accessing this API from a browser you'll need to add a CORS policy, see https://docs.microsoft.com/en-us/aspnet/core/security/cors

            string domain = $"https://{Configuration["Auth0:Domain"]}/";
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = domain;
                    options.Audience = Configuration["Auth0:Audience"];

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            // Grab the raw value of the token, and store it as a claim so we can retrieve it again later in the request pipeline
                            // Have a look at the ValuesController.UserInformation() method to see how to retrieve it and use it to retrieve the
                            // user's information from the /userinfo endpoint
                            if (context.SecurityToken is JwtSecurityToken token)
                            {
                                if (context.Principal.Identity is ClaimsIdentity identity)
                                {
                                    identity.AddClaim(new Claim("access_token", token.RawData));
                                }
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddSingleton(x =>
                new AuthenticationApiClient(new Uri($"https://{Configuration["Auth0:Domain"]}/")));

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
