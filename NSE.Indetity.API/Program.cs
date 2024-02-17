using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NS.Identidade.API.Extensions;
using NSE.Indetity.API.Configuration;
using NSE.Indetity.API.Data;
using NSE.Indetity.API.Extensions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



IHostEnvironment env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettingsDevelopment.{env.EnvironmentName}.json", optional: true, true);


// Add services to the container.
builder.Services.AddIdentityConfiguration(builder.Configuration) ;

builder.Services.AddApiConfiguration(); //conf api

builder.Services.AddSwaggerConfiguration(); // conf swagger

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwaggerConfiguration(builder.Environment);

app.UseApiConfiguration();

app.Run();