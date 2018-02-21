using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TokenBasedAuth
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;

            var builder = new ConfigurationBuilder();
            builder.SetBasePath(env.ContentRootPath);
            builder.AddJsonFile("appsettings.json", false, true);
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtConfig>(Configuration.GetSection("JwtConfig"));

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<JwtConfig>>();

            var jwtConfig = options.Value;
            var bytes = Encoding.UTF8.GetBytes(jwtConfig.SigningKey);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtBearerOptions =>
                {
                    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateActor = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig.Issuer,
                        ValidAudience = jwtConfig.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(bytes)
                    };
                });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
        }
    }

    public class JwtConfig
    {
        public string SigningKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
