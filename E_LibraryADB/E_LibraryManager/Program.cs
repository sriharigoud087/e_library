using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using E_LibraryManager.BackgroundServices;
using E_LibraryManager.Common.Models;
using E_LibraryManager.Migration;
using E_LibraryManager.Models;
using E_LibraryManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));

builder.Services.Configure<JwtConfiguration>(
    builder.Configuration.GetSection("JwtConfiguration"));

var configuration = builder.Configuration;
AppSettings appSettings = new AppSettings();
configuration.Bind(appSettings);

builder.Services.AddSingleton(appSettings);

MigrateDB migrateDB = new MigrateDB(appSettings);
migrateDB.MigrateDatabase();

builder.Services.AddScoped<AuthorService>();
builder.Services.AddScoped<BooksService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<BookCategoryService>();
builder.Services.AddScoped<BookTransactionsService>();
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<BookReserveService>();

builder.Services.AddHostedService<TimedHostedService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region AddAuthentication

string issuer = appSettings.JwtConfiguration.Issuer;
string audience = appSettings.JwtConfiguration.Audience;
string signingKey = appSettings.JwtConfiguration.Key;
byte[] signingKeyBytes = System.Text.Encoding.UTF8.GetBytes(signingKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(options =>
   {
       options.RequireHttpsMetadata = false;
       options.SaveToken = true;
       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuer = true,
           ValidateAudience = true,
           ValidateLifetime = true,
           ValidateIssuerSigningKey = true,
           ValidIssuer = issuer,
           ValidAudience = audience,
           IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes)
       };
   });
#endregion

AddSwaggerGen(builder.Services);

builder.Services.AddCors(policyBuilder =>
    policyBuilder.AddDefaultPolicy(policy =>
        policy.WithOrigins("*").AllowAnyHeader().AllowAnyHeader())
);

var app = builder.Build();
app.UseCors();

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


void AddSwaggerGen(IServiceCollection services)
{

    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "E_Library API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please insert JWT with Bearer into field",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                       {
                         Type = ReferenceType.SecurityScheme,
                         Id = "Bearer"
                       }
                      },
                      new string[] { }
                    }
                  });
    });
}
