using Application;
using Application.Abstractions.Data;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OrderTrackingSystem.AspNet.Extensions;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;

builder.Host.AddSerilog();

builder.Configuration.AddConfiguration(environment);

builder.Services.AddWeb(builder.Configuration);

builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseWeb();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var serviceScope = app.Services.CreateScope();
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
