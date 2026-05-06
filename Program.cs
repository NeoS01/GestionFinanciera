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

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
