using Application;
using Application.Abstractions.Data;
using Infrastructure;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using OrderTrackingSystem.AspNet.Extensions;
using WebApi.GrpcServices;
using WebApi.GrpcServices.Interceptors;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8083, o => o.Protocols = HttpProtocols.Http2);
    options.ListenAnyIP(8080, o => o.Protocols = HttpProtocols.Http1); 
});

var environment = builder.Environment;

builder.Host.AddSerilog();

builder.Configuration.AddConfiguration(environment);

builder.Services.AddWeb(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddGrpc(options => 
    options.Interceptors.Add<LoggingInterceptor>());

var app = builder.Build();

app.UseWeb();

app.MapControllers();
app.MapGrpcService<OrderService>();

if (app.Environment.IsDevelopment())
{
    using var serviceScope = app.Services.CreateScope();
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();

[UsedImplicitly]
public partial class Program;

