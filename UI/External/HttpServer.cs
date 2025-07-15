﻿// HttpServer.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Application = System.Windows.Application;

namespace UI
{
    public static class HttpServer
    {
        private static Thread? _apiThread;

        public static void Start()
        {
            if (_apiThread != null) return;

            _apiThread = new Thread(() =>
            {
                var builder = WebApplication.CreateBuilder();
                var app = builder.Build();
                app.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Beep");
                });
                app.MapGet("/health-check", async context =>
                {
                    await context.Response.WriteAsync("Check OK");
                });
                // Define endpoints here
                app.MapGet("/play-beep", async context =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (Application.Current.MainWindow is MainWindow mainWindow)
                        {
                            mainWindow.PlayBeep();
                        }
                    });

                    await context.Response.WriteAsync("Beep played.");
                });

                app.Run("http://localhost:8080");
            });

            _apiThread.IsBackground = true;
            _apiThread.Start();
        }
    }
}
