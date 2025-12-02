using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

// Leer correctamente las connection strings
var sqlServerConnection = builder.Configuration.GetConnectionString("SqlServerDbContext")
    ?? throw new InvalidOperationException("Connection string 'SqlServerDbContext' not found.");
var postgresConnection = builder.Configuration.GetConnectionString("PostgresDbContext")
    ?? throw new InvalidOperationException("Connection string 'PostgresDbContext' not found.");

// (Opicional) Debug: ver qué cadenas se están leyendo
Console.WriteLine($"SQL Server connection: {sqlServerConnection}");
Console.WriteLine($"Postgres connection: {postgresConnection}");

// Conexión a PostgreSQL ----------------------------------------------------------------------------
builder.Services.AddDbContext<PostgresDbContext>(options =>
    options.UseNpgsql(postgresConnection, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(typeof(PostgresDbContext).Assembly.FullName);
        npgsqlOptions.MigrationsHistoryTable("_EFMigrationsHistory_PostgresSQL");
    }));

// Conexión a SQL Server ----------------------------------------------------------------------------
builder.Services.AddDbContext<SqlServerDbContext>(options =>
    options.UseSqlServer(sqlServerConnection, sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(typeof(SqlServerDbContext).Assembly.FullName);
        sqlOptions.MigrationsHistoryTable("_EFMigrationsHistory_SqlServer");
    }));

// Add services to the container.
builder.Services
           .AddControllers()
           .AddNewtonsoftJson(options =>
               options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();