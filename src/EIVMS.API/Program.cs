using EIVMS.Infrastructure;
using EIVMS.Infrastructure.Persistence;
using EIVMS.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (app.Environment.IsDevelopment())
    {
        try
        {
            await RolePermissionSeeder.SeedAsync(db);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database seeding skipped: {ex.Message}");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Development");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();