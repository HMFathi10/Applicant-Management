using ApplicantManagement.Application.Features.Applicants.Commands.CreateApplicant;
using ApplicantManagement.Domain.Repositories;
using ApplicantManagement.Infrastructure.Data;
using ApplicantManagement.Infrastructure.Data.DataMigration;
using ApplicantManagement.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IApplicantRepository, ApplicantRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add MediatR
builder.Services.AddMediatR(typeof(CreateApplicantCommand).Assembly);

// Add Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateApplicantValidator>();

// Add HttpClient
builder.Services.AddHttpClient();

// Add Country Validation Service
builder.Services.AddHttpClient<ApplicantManagement.Infrastructure.Services.ICountryValidationService, ApplicantManagement.Infrastructure.Services.CountryValidationService>();
builder.Services.AddScoped<ApplicantManagement.Infrastructure.Services.ICountryValidationService, ApplicantManagement.Infrastructure.Services.CountryValidationService>();

// Add Applicant Services
builder.Services.AddScoped<ApplicantManagement.Applicants.Services.IApplicantLoggingService, ApplicantManagement.Applicants.Services.ApplicantLoggingService>();
builder.Services.AddScoped<ApplicantManagement.Applicants.Services.IApplicantSecurityService, ApplicantManagement.Applicants.Services.ApplicantSecurityService>();

// Add Data Migration
builder.Services.AddDataMigration();

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add CORS - Updated configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "https://localhost:5173", "https://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true)); // For development only
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Applicant Management API", Version = "v1" });
});

var app = builder.Build();

// Run data migration
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    await app.MigrateDataAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANT: CORS must come BEFORE UseHttpsRedirection
app.UseCors("AllowAll");

// Only use HTTPS redirection in production, or configure it properly for development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();