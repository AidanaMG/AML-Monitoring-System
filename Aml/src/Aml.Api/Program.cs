using Aml.Api.Services;
using Aml.Application;
using Aml.Application.Audit;
using Aml.Application.Rules;
using Aml.Application.RulesAbstractions;
using Aml.Application.Screening;
using Aml.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AmlDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AmlDb")));

builder.Services.AddSingleton<IReferenceDataCache, EfReferenceDataCache>();


builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddScoped<IRuleContextBuilder, EfRuleContextBuilder>();
builder.Services.AddScoped<IAlertRepository, EfAlertRepository>();

builder.Services.AddScoped<ITransactionRule, CriticalRiskCountryRule>();
builder.Services.AddScoped<ITransactionRule, CriticalRiskMerchantRule>();
builder.Services.AddScoped<ITransactionRule, CryptoArbitrageRule>();
builder.Services.AddScoped<ITransactionRule, DropperRule>();
builder.Services.AddScoped<ITransactionRule, DrugRule>();
builder.Services.AddScoped<ITransactionRule, GamblingRule>();
builder.Services.AddScoped<ITransactionRule, HighFrequencyRule>();
builder.Services.AddScoped<ITransactionRule, HighRiskCountryRule>();
builder.Services.AddScoped<ITransactionRule, HighRiskMerchantRule>();
builder.Services.AddScoped<ITransactionRule, WatchlistRule>();

builder.Services.AddScoped<TransactionScreeningService>();

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddScoped<IAuditService, EfAuditService>();

var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwt["Audience"],

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AdminPanel", policy => 
        policy.WithOrigins("http://localhost:5063")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AmlDbContext>();
    await ReferenceDataSeeder.SeedAsync(db, CancellationToken.None);

    await UserSeeder.SeedAsync(db, CancellationToken.None);

    var cache = scope.ServiceProvider.GetRequiredService<IReferenceDataCache>();
    await cache.RefreshAsync(CancellationToken.None);

}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if(!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AdminPanel");

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


app.Run();
