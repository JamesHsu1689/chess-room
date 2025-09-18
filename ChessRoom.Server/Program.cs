
using ChessRoom.Server.Hubs;
using ChessRoom.Server.Services;

namespace ChessRoom.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Add my own services
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IRoomStore, InMemoryRoomStore>();

            // Allow Vite dev origin in Development
            //builder.Services.AddCors(o => o.AddPolicy("DevCors", p =>
            //{
            //    p.WithOrigins("http://localhost:60201")
            //     .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            //}));

            // Allow Vite dev origin in Development
            builder.Services.AddCors(o => o.AddPolicy("DevCors", p =>
            {
                p.WithOrigins(
                    "https://localhost:60201", // Vite over HTTPS 
                    "http://localhost:60201"   // (optional) if you ever run Vite over HTTP
                )
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials();
            }));


            var app = builder.Build();

            app.UseDefaultFiles();
            app.MapStaticAssets();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseCors("DevCors");
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            // hookup GameHub
            app.MapHub<GameHub>("/hubs/game");

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
