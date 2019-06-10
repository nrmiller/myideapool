using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MyIdeaPool.Models;

namespace MyIdeaPool
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
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //.AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters()
            //    {
            //        ValidateIssuer = true, // Validate the server that issued the token.
            //        ValidateAudience = true, // Validate that the audience is authorized to receive the token.
            //        ValidateLifetime = true, // Validate that the token is not expired.
            //        ValidateIssuerSigningKey = true, // Validate the issuer's signing key.
            //        ValidIssuer = Configuration["Jwt:Issuer"], // JWT is issued by the ASP.NET webserver.
            //        ValidAudience = Configuration["Jwt:Issuer"], // ASP.NET webserver processes the JWT on requests.
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
            //    };
            //});
            /*
            services.AddDefaultIdentity<User>();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;        // Must contain at least 8 characters.
                options.Password.RequireUppercase = true;   // Must contain at least 1 uppercase letter.
                options.Password.RequireLowercase = true;   // Must contain at least 1 lowercase letter.
                options.Password.RequireDigit = true;       // Must contain at least 1 number.
            });
            */

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(sp => sp.GetRequiredService<IHttpContextAccessor>().HttpContext);
            services.AddScoped<JwtTokenHelper>();
            services.AddDbContext<IdeaContext>(opt => opt.UseInMemoryDatabase(databaseName: "ideas_database"));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            // We are not using ASP.NET framework for authentication as we need
            // to grab the JWT from the X-Access-Token request header.
            //app.UseAuthentication();
            app.UseMvc();
        }
    }
}
