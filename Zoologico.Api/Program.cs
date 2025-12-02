using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

var SqlServerConeccion = builder.Configuration.GetConnectionString("PostgresDbContext");
var PostgresConeccion = builder.Configuration.GetConnectionString("SqlServerDbContext");
// Coneccion a PostgreSQL ----------------------------------------------------------------------------
builder.Services.AddDbContext<PostgresDbContext>(options =>
    options.UseNpgsql( PostgresConeccion, sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(typeof(PostgresDbContext).Assembly.FullName);
        sqlOptions.MigrationsHistoryTable("_EFMigrationsHistory_PostgresSQL");
    
    }));
// Coneccion a SQL Server ----------------------------------------------------------------------------
builder.Services.AddDbContext<SqlServerDbContext>(options =>
    options.UseSqlServer(SqlServerConeccion, sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(typeof(SqlServerDbContext).Assembly.FullName);
        sqlOptions.MigrationsHistoryTable("_EFMigrationsHistory_SqlServer");

    }));

//builder.Services.AddDbContext<SqlServerDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerDbContext") ?? throw new InvalidOperationException("Connection string 'SqlServerDbContext' not found.")));
//builder.Services.AddDbContext<ZoologicoApiContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("ZoologicoApiContext") ?? throw new InvalidOperationException("Connection string 'ZoologicoApiContext' not found.")));

// Add services to the container.
builder.Services
           .AddControllers()
           .AddNewtonsoftJson(
               options => options.SerializerSettings.ReferenceLoopHandling
               = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
