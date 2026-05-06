using Microsoft.EntityFrameworkCore;
using GestionFinanciera.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GestionFinancieraContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("GestionFinancieraContext")
        ?? throw new InvalidOperationException("Connection string 'GestionFinancieraContext' not found.")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ================================================================
// APLICAR MIGRACIONES AUTOMÁTICAMENTE AL INICIAR
// ================================================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GestionFinancieraContext>();
    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("✅ Migraciones aplicadas correctamente a la base de datos");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error aplicando migraciones: {ex.Message}");
        Console.WriteLine("La aplicación continuará ejecutándose, pero las consultas podrían fallar");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Endpoint de bienvenida para la raíz (opcional, evita error 404)
app.MapGet("/", () => Results.Ok(new { 
    mensaje = "API GestionFinanciera funcionando correctamente",
    swagger = "/swagger",
    endpoints = "/api/nombre-de-tus-controladores"
}));

app.Run();