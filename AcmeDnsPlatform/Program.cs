
using AcmeDnsPlatform.Api;
using AcmeDnsPlatform.Provider.BunnyDns;
using AcmeDnsPlatform.Provider.LibSql;
using AcmeDnsPlatform.Provider.Sha384Provider;
using AcmeDnsPlatform.Provider.Sqlite3;
using Microsoft.AspNetCore.Rewrite;

namespace AcmeDnsPlatform
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddSingleton<IHashFunction, Sha384>();
            builder.Services.AddSingleton<IPlatformDnsManagement, BunnyDns>();
            builder.Services.AddSingleton<IPlatformAccountManagement, LibSql>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(configure =>
            {
                configure.EnableAnnotations();
            });

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            
            var option = new RewriteOptions();
            option.AddRedirect("^$", "swagger");
            app.UseRewriter(option);

            app.Run();
        }
    }
}
