using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Owin;
using Microsoft.Extensions.Options;
using System;
using Microsoft.Extensions.Time.Testing;
using System.Text.Json;

[assembly: OwinStartup(typeof(C3D.IISOwinApp.Startup))]
namespace C3D.IISOwinApp;


public class Startup
{
    private readonly IHost hostApp;

    public Startup()
    {
        System.Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddOptions<AppOptions>()
            .BindConfiguration("AppOptions")
            .ValidateOnStart();

        builder.Services.AddSingleton<TimeProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AppOptions>>().Value;
            if (options.UseTestTimeservice)
            {
                return new FakeTimeProvider(options.TestTime!.Value);
            }
            return TimeProvider.System;
        });

        this.hostApp = builder.Build();
        this.hostApp.Start();
    }

    public void Configuration(IAppBuilder app)
    {
        app.Map("/options", opt => opt.Run(async context =>
        {
            var options = hostApp.Services.GetRequiredService<IOptions<AppOptions>>().Value;
            context.Response.ContentType = "application/json";
            using var outputStream = context.Response.Body;
            await JsonSerializer.SerializeAsync(outputStream, options);
        }));
        app.Map("/ts", ts=>ts.Run(async context =>
        {
            var ts = hostApp.Services.GetRequiredService<TimeProvider>();
            await context.Response.WriteAsync(ts.GetUtcNow().ToString("O"));
        }));
        app.Run(async context =>
        {
            var options = hostApp.Services.GetRequiredService<IOptions<AppOptions>>().Value;
            await context.Response.WriteAsync(options.TestResponse);
        });
    }
}
