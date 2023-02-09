using FinancialApi.Data;
using FinancialApi.Models.Account;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FinancialDbContext>(options => options.UseSqlServer(connection));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();
    c.CustomSchemaIds(type => type.FullName);
});
builder.Services.AddCors(p => p.AddPolicy("corspolicy", build =>
{
    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));


var app = builder.Build();
app.UseCors("corspolicy");


app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
