using System;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TimeTracker.Data;
using TimeTracker.Extensions;
using TimeTracker.Models.Validation;

namespace TimeTracker
{
    public class Startup
    {
        public static Action<IConfiguration, DbContextOptionsBuilder> ConfigureDbContext = (configuration, options) =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TimeTrackerDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddControllers().AddFluentValidation(
                fv => fv.RegisterValidatorsFromAssemblyContaining<UserInputModelValidator>());

            services.AddJwtBearerAuthentication(Configuration);

            services.AddDbContext<TimeTrackerDbContext>(options => ConfigureDbContext(Configuration, options));

            services.AddOpenApi();

            services.AddVersioning();

            services.AddHealthChecks()
                .AddSqlite(Configuration.GetConnectionString("DefaultConnection"));

            // In Startup.ConfigureServices method
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // Inject our custom error handling middleware into ASP.NET Core pipeline
            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseOpenApi();

            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });

            // In Startup.Configure method
            // NOTE: this is just for demo purpose! Usually, you should limit access to a specific origin.
            app.UseCors(
                builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        }
    }
}
