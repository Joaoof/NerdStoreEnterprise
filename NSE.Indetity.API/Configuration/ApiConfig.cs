using Microsoft.AspNetCore.Builder;

namespace NSE.Indetity.API.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
        {
            services.AddControllers();

            return services;
        }



        public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app)
        {
            var builder = WebApplication.CreateBuilder();

            // Configure the HTTP request pipeline

            app.UseRouting();

            app.UseHttpsRedirection();

            app.UseIdentityConfiguration();
                
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            return app;
        }
    }
    
}
