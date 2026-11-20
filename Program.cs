using System.Text.Json.Serialization;
using System.Reflection;
using LINDE.Registro.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var cadena = builder.Configuration.GetConnectionString("LindeDb")
    ?? throw new InvalidOperationException("Falta la cadena de conexión LindeDb.");

builder.Services.AddDbContext<LindeDbContext>(options =>
    options.UseSqlServer(cadena));

// Los enum (EstadoPredio) se envían y reciben como texto en lugar de números.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Errores en formato estándar ProblemDetails (RFC 7807).
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{var xml = Path.Combine(
    AppContext.BaseDirectory,
    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"
);

if (File.Exists(xml))
{
    c.IncludeXmlComments(xml);
}
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LINDE Registro API",
        Version = "v1",
        Description = "Gestión de productores y predios del sistema LINDE."
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
