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
using System;

var builder = WebApplication.CreateBuilder(args);
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

// UserProfile API
//Register user
app.MapPost("/accounts/register", ([FromServices] ProfileServiceDbContext db, FullUserProfile user) =>
{
    PasswordHasher hasher = new PasswordHasher();
    var hashedPassword = hasher.Hash(user.Password);
    user.Password = hashedPassword;
    db.UserProfile.Add(user);
    db.SaveChanges();
});
//Login
app.MapPost("/accounts/login", async ([FromServices] ProfileServiceDbContext db, LoginForm login) => 
{
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
            return tokenString;
        }
    }
    else
    {
        return "Not Verified";
    }
    return "Error";
});
//Get all users
app.MapGet("/admin/getallusers",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
    ([FromServices] ProfileServiceDbContext db) =>
{
    return db.UserProfile.ToList();
});
//Get specific user
app.MapGet("/admin/getuser/{id}",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
    ([FromServices] ProfileServiceDbContext db, int id) =>
{
    return db.UserProfile.Find(id);
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
    target.Username = user.Username;
    target.Email = user.Email;
    db.SaveChanges();
});
//Delete user
app.MapPost("/user/delete",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "USER")]
    ([FromServices] ProfileServiceDbContext db, DeleteUser deleteId) =>
    {
    var id = int.Parse(deleteId.Id);
    var target = db.UserProfile.Find(id);
    db.UserProfile.Remove(target);
    db.SaveChanges();
});
//Archive users
app.MapPut("/admin/archive/{id}",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
([FromServices] ProfileServiceDbContext db, int id) =>
    {
        var target = db.UserProfile.FirstOrDefault(u => u.UserID == id);
        target.Status = "INACTIVE";
        db.SaveChanges();
    });

app.Run();