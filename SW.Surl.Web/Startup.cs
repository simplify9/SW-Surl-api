using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SW.CqApi;
using SW.HttpExtensions;
using SW.Logger;
using SW.Surl.Sdk;

namespace SW.Surl.Web
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
            services.AddControllers(); 
            //services.AddApiClient<SurlClient, SurlClientOptions>();

            services.AddAuthentication().
                AddJwtBearer(configureOptions =>
                {
                    configureOptions.RequireHttpsMetadata = false;
                    configureOptions.SaveToken = true;
                    configureOptions.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = Configuration["Token:Issuer"],
                        ValidAudience = Configuration["Token:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Token:Key"]))
                    };

                });

            services.AddCqApi(options =>
            {
                options.UrlPrefix = "api";
                //options.ProtectAll = true;

            });

            services.AddDbContext<SurlDbContext>(c =>
            {
                c.EnableSensitiveDataLogging(true);
                // created_on is timestamp without time zone in the database but the model maps DateTime
                // to timestamptz (Npgsql 6+), which EF 8 ignored and EF 9+ reports as a pending change and
                // throws on in Migrate(). Pinning the column type would make Npgsql refuse the UTC
                // CreatedOn values the audit writes, so keep EF 8's behaviour.
                c.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
                c.UseSnakeCaseNamingConvention();
                c.UseNpgsql(Configuration.GetConnectionString(SurlDbContext.ConnectionString), b =>
                {
                    b.MigrationsHistoryTable("_ef_migrations_history", SurlDbContext.Schema);
                    b.UseAdminDatabase("defaultdb");
                });

            });

            services.AddHostedService<ExpiredUrlCleanUpService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseHttpAsRequestContext();
            app.UseRequestContextLogEnricher();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapCqApi();
            });
        }
    }
}
