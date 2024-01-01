using CW2_ProfileService;
using CW2_ProfileService.Model;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Configure Serilog for file logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/myapp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
var connectionString = builder.Configuration.GetConnectionString("AppDb");
builder.Services.AddDbContext<ProfileServiceDbContext>(x => x.UseSqlServer(connectionString));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme,
        securityScheme: new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter the Bearer Authorization : `Bearer generated-JWT Token`",
            In=ParameterLocation.Header,
            Type=SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id=JwtBearerDefaults.AuthenticationScheme
                }
            }, new string[]
            {

            }

        }

    });
});
var app = builder.Build();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger(x  => x.SerializeAsV2 = true);

// Track failed login attempts for a user
Dictionary<string, int> failedLoginAttempts = new Dictionary<string, int>();

//Register user
app.MapPost("/accounts/register", ([FromServices] ProfileServiceDbContext db, FullUserProfile user) =>
{
    user.Username.Trim();
    user.Email.Trim();
    user.Password.Trim();
    // Check for null or empty username and email
    if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email))
    {
        var result = Results.BadRequest("Username and Email are required fields.");
        return result;
    }

    if (user.Password.Length < 8)
    {
        var result = Results.BadRequest("Password must be at least 8 characters long.");
        return result;
    }

    PasswordHasher hasher = new PasswordHasher();
    var hashedPassword = hasher.Hash(user.Password);
    user.Password = hashedPassword;
    db.UserProfile.Add(user);
    db.SaveChanges();
    Log.Information($"{user.Username} successful register");
    var result2 = Results.Ok("User registered successfully.");
    return result2;
});
//Login
app.MapPost("/accounts/login", async ([FromServices] ProfileServiceDbContext db, LoginForm login) => 
{
    login.Username.Trim();
    login.Email.Trim();
    login.Password.Trim();
    // Check if the user has exceeded the maximum allowed failed attempts
    if (failedLoginAttempts.TryGetValue(login.Username, out int attempts) && attempts >= 5)
    {
        Log.Warning($"{login.Username} Account temporarily blocked due to multiple failed login attempts.");
        return "Account temporarily blocked due to multiple failed login attempts.";
    }
    // Check for null or empty username and email
    if (string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Email))
    {
        return "Username and Email are required fields.";
    }
    //Post request UOP to auth api 
    var client = new HttpClient();
    client.BaseAddress = new Uri("https://web.socem.plymouth.ac.uk/COMP2001/auth/api/");
    var json = JsonSerializer.Serialize(login);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = client.PostAsync("users", content).Result;
    var responseContent = response.Content.ReadAsStringAsync().Result;

    if (responseContent.Contains("True"))
    {
        var foundUser = db.UserProfile.FirstOrDefault(u => u.Username == login.Username && u.Email == login.Email && u.Status == "ACTIVE");
        PasswordHasher hasher = new PasswordHasher();
        if (hasher.Verify(foundUser.Password, login.Password))
        {
            // Successful login, reset failed attempts for the user
            if (failedLoginAttempts.ContainsKey(login.Username))
            {
                failedLoginAttempts.Remove(login.Username);
            }
            int userID = foundUser.UserID;
            string claimsUserID = userID.ToString();
            var claims = new[]
            {
                    new Claim("UserID", claimsUserID),
                    new Claim("Username", foundUser.Username),
                    new Claim("Email", foundUser.Email),
                    new Claim(ClaimTypes.Role, foundUser.Role),
                };

            var token = new JwtSecurityToken(
                    issuer: builder.Configuration["Jwt:Issuer"],
                    audience: builder.Configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(6),
                    notBefore: DateTime.UtcNow,
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                        SecurityAlgorithms.HmacSha256)
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Log.Information($"{foundUser.Username} successful login");
            return tokenString;
        }
    }
    else
    {
        // Increment failed login attempts for the user
        if (failedLoginAttempts.ContainsKey(login.Username))
        {
            failedLoginAttempts[login.Username]++;
            Log.Warning($"{login.Username} failed login attempt");
        }
        else
        {
            failedLoginAttempts.Add(login.Username, 1);
            Log.Warning($"{login.Username} failed login attempt");
        }
        Log.Warning($"{login.Username} not verified login attempt");
        return "Not Verified";
    }
    Log.Error("Login Error");
    return "Error";
});
//Get all users
app.MapGet("/admin/getallusers",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
    ([FromServices] ProfileServiceDbContext db) =>
{
    var users = db.UserProfile.ToList();

    List<AdminViewUserProfile> views = users.Select(user => new AdminViewUserProfile
    {
        UserID = user.UserID,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role,
        JoinDate = user.JoinDate,
        Status = user.Status
    }).ToList();
    Log.Information($"Get all users accessed");
    return views;
});
//Get user own data
app.MapGet("/user/getuser/{id}",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "USER")]
([FromServices] ProfileServiceDbContext db, int id) =>
    {
        var user = db.UserProfile.Find(id);
        SelfUserProfileData view = new SelfUserProfileData();
        view.Username = user.Username;
        view.Email = user.Email;
        view.JoinDate = user.JoinDate;
        return view;
    });
//User limited view others
app.MapGet("/user/getotheruser/{id}",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "USER")]
([FromServices] ProfileServiceDbContext db, int id) =>
    {
        var users = db.UserProfile
        .Where(u => u.Role == "USER" && u.UserID != id) // Filter users by role
        .ToList();
        List<LimitedUserProfileView> views = users.Select(user => new LimitedUserProfileView
        {
            Username = user.Username,
            JoinDate = user.JoinDate
        }).ToList();
        return views;
    });
//Update user details
app.MapPut("/user/update/{id}",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "USER")]
([FromServices] ProfileServiceDbContext db, int id, EditUser user) =>
{
    var target = db.UserProfile.FirstOrDefault(u => u.UserID == id);
    var oldName = target.Username;
    target.Username = user.Username;
    target.Email = user.Email;
    db.SaveChanges();
    Log.Information($"User {oldName} updated profile. New name {user.Username} New Email {user.Email}");
});
//Delete user
app.MapPost("/user/delete",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "USER")]
    ([FromServices] ProfileServiceDbContext db, UserID deleteId) =>
    {
    var id = int.Parse(deleteId.Id);
    var target = db.UserProfile.Find(id);
    var targetName = target.Username;
    db.UserProfile.Remove(target);
    db.SaveChanges();
    Log.Information($"User {targetName} deleted profile.");
    });
//Archive users
app.MapPut("/admin/archive",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
([FromServices] ProfileServiceDbContext db, UserID archiveId) =>
    {
        var id = int.Parse(archiveId.Id);
        var checkRole = db.UserProfile.FirstOrDefault(u => u.UserID == id && u.Role == "ADMIN");
        //If the profile to archive is admin, stop
        if (checkRole != null)
        {
            Log.Information("Attempt to archive admin user");
            return "Invalid action!";
        }
        var target = db.UserProfile.Find(id);
        target.Status = "INACTIVE";
        db.SaveChanges();
        Log.Information($"Successfully archived user id {id}");
        return $"Successfully archived user id {id}";
    });

app.Run();