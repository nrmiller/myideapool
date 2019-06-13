using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyIdeaPool.Tools;
using MyIdeaPool.Models;

namespace MyIdeaPool
{
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyIdeaPool.Startup"/> class.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<JwtTokenHelper>();
            services.AddDbContext<IdeaPoolContext>(opt => opt.UseInMemoryDatabase(databaseName: "ideas_database"));
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
                // Not using HTTPS/SSL
                //app.UseHsts();
            }

            // Not using HTTPS/SSL
            //app.UseHttpsRedirection();

            app.UseMvc();
        }
    }
}
