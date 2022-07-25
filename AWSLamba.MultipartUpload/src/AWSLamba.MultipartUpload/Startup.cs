using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;

namespace AWSLamba.MultipartUpload
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            // as needed, define the regions, profile etc., of AWS S3 information, using AWSOptions 
            var s3Option = new AWSOptions();
            s3Option.Region = Amazon.RegionEndpoint.USEast1;
            services.AddAWSService<IAmazonS3>(s3Option, ServiceLifetime.Singleton);

            
            services.AddCors(options=>
            {
                options.AddPolicy("mypolicy", policy =>
                {
                    policy.WithOrigins(Configuration["AllowedOrigins"].Split(","));
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();

                });
            });            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseCors("mypolicy");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                });
            });
        }
    }
}