using Api.Infrastructure.IoC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api
{
    public class Startup
    {
        private readonly string _corsPolicy = "AllowAllHeaders";

        public Container _dependencyInjectionContainer { get; } = new Container();
        public Bootstrapper _bootstrapper { get; } = new Bootstrapper();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors(opt =>
            {
                opt.AddPolicy(_corsPolicy,
                    builder =>
                    {
                        builder
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowAnyOrigin();
                    });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Driuthorn-Personal-Data-Analytics", Version = "v1" });
            });

            _dependencyInjectionContainer.Options.AllowOverridingRegistrations = true;
            _dependencyInjectionContainer.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();


            services.AddSimpleInjector(_dependencyInjectionContainer, opt =>
            {
                opt.AddAspNetCore()
                    .AddControllerActivation();

                opt.AddLogging();
            });

            services.AddSingleton(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors(_corsPolicy);
            _bootstrapper.Inject(_dependencyInjectionContainer);
            app.UseSimpleInjector(_dependencyInjectionContainer);
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Driuthorn-Personal-Data-Analytics");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller}/{action}");
            }); ;

            app.UseHttpsRedirection();

            _dependencyInjectionContainer.Verify();
        }
    }
}
