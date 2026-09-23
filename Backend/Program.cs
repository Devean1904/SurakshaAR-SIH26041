using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using SurakshaAR.Backend.Services;
using SurakshaAR.Backend.Config;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SurakshaAR API",
        Version = "v1",
        Description = "AR-based Workplace Safety Platform API"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter: Bearer {your_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.Configure<MongoConfig>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<MongoService>();
builder.Services.AddSingleton<BlockchainService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<OtpService>();
builder.Services.AddSingleton<ScenarioService>();
builder.Services.AddSingleton<EscalationService>();
builder.Services.AddHttpClient<SmsService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(EnsureKeyLength(builder.Configuration["Jwt:Key"] ?? "SurakshaAR_SuperSecret_Key_2026!SecureEnough")))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("WorkerOnly", policy => policy.RequireRole("Worker"));
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

static string EnsureKeyLength(string key)
    => key.Length < 32 ? key.PadRight(32, '!') : key;

using (var scope = app.Services.CreateScope())
{
    var mongoService = scope.ServiceProvider.GetRequiredService<MongoService>();
    await mongoService.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path == "/verify")
    {
        ctx.Response.Redirect("/verify/");
        return;
    }
    await next();
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", version = "1.0.0" }));
app.MapGet("/", () => Results.Redirect("/admin/"));

app.Run();
