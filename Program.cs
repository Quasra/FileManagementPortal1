using System.Text;
using AutoMapper;
using FileManagementPortal1.DTOs.Account;
using FileManagementPortal1.DTOs.Files;
using FileManagementPortal1.Models;
using FileManagementPortal1.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add HttpContextAccessor for tracking user in auditing
builder.Services.AddHttpContextAccessor();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<AppUser, AppRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add Authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"]
    };
});

// Add JWT Token Generation Service directly in Program.cs
builder.Services.AddScoped<Func<AppUser, Task<string>>>(serviceProvider => async (AppUser user) => {
    var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var roles = await userManager.GetRolesAsync(user);

    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Name, user.UserName)
    };

    // Add roles as claims
    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(1),
        SigningCredentials = credentials,
        Issuer = configuration["JWT:Issuer"],
        Audience = configuration["JWT:Audience"]
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);

    return tokenHandler.WriteToken(token);
});

// Add AutoMapper
builder.Services.AddAutoMapper(config => {
    // User mapping
    config.CreateMap<AppUser, UserDto>();
    config.CreateMap<RegisterDto, AppUser>();

    // File mapping
    config.CreateMap<FileModel, FileDto>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
        .ForMember(dest => dest.FolderName, opt => opt.MapFrom(src => src.Folder != null ? src.Folder.Name : null));

    // Folder mapping
    config.CreateMap<Folder, FolderDto>()
        .ForMember(dest => dest.FileCount, opt => opt.MapFrom(src => src.Files != null ? src.Files.Count : 0));
    config.CreateMap<FolderCreateDto, Folder>();
});

// Register Repositories (No Interface)
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped<FileRepository>();
builder.Services.AddScoped<FolderRepository>();
builder.Services.AddScoped<UserRepository>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FileManagementPortal1 API",
        Version = "v1",
        Description = "File Management Portal API developed for university project"
    });

    // Include JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Uygulama baþlangýcýnda veritabaný ve rolleri oluþtur
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Veritabanýný oluþtur
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // Rolleri oluþtur
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var roles = new[] { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new AppRole { Name = role, Description = $"{role} role" });
            }
        }

        // Varsayýlan admin kullanýcýsýný oluþtur
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        const string adminEmail = "admin@example.com";
        const string adminPassword = "Admin123!";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var user = new AppUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, adminPassword);
            await userManager.AddToRoleAsync(user, "Admin");
        }

        // Varsayýlan klasörleri oluþtur
        if (!context.Folders.Any())
        {
            context.Folders.AddRange(
                new Folder { Name = "Documents", Description = "Word, Excel, PDF files", CreatedBy = "System" },
                new Folder { Name = "Images", Description = "Photo and image files", CreatedBy = "System" },
                new Folder { Name = "Videos", Description = "Video files", CreatedBy = "System" },
                new Folder { Name = "Others", Description = "Other file types", CreatedBy = "System" }
            );
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();