using CW2_ProfileService;
using CW2_ProfileService.Model;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

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
});
var app = builder.Build();
app.UseSwaggerUI();
app.UseAuthorization();
app.UseAuthentication();
app.UseSwagger(x  => x.SerializeAsV2 = true);


// UserProfile API
//Login
app.MapPost("/user/login", ([FromServices] ProfileServiceDbContext db, LoginForm login) => 
{

        var foundUser = db.UserProfile.FirstOrDefault(u => u.Username == login.Username && u.Email == login.Email && u.Status == "ACTIVE");
        PasswordHasher hasher = new PasswordHasher();
         if (hasher.Verify(foundUser.Password,login.Password))
         {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, foundUser.Username),
                    new Claim(ClaimTypes.Email, foundUser.Email),
                    new Claim(ClaimTypes.Role, foundUser.Role)
                };

                var token = new JwtSecurityToken(
                        issuer: builder.Configuration["Jwt:Issuer"],
                        audience: builder.Configuration["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.UtcNow.AddMinutes(20),
                        notBefore: DateTime.UtcNow,
                        signingCredentials: new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                            SecurityAlgorithms.HmacSha256)
                    );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return tokenString;
         }
         else
         {
            return "Account not found";
         }
});

//Get all users
app.MapGet("/users",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
    ([FromServices] ProfileServiceDbContext db) =>
{
    return db.UserProfile.ToList();
});
//Get specific user
app.MapGet("/user/{id}",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
    ([FromServices] ProfileServiceDbContext db, string userID) =>
{
    return db.UserProfile.Find(userID);
});
//Register user
app.MapPost("/user/register", ([FromServices] ProfileServiceDbContext db, UserProfile user) =>
{
    PasswordHasher hasher = new PasswordHasher();
    var hashedPassword = hasher.Hash(user.Password);
    user.Password = hashedPassword;
    db.UserProfile.Add(user);
    db.SaveChanges();
});
//Update user details
app.MapPut("/user/update/{id}", ([FromServices] ProfileServiceDbContext db, int userID, UserProfile user) =>
{
    var target = db.UserProfile.FirstOrDefault(u => u.UserID == userID);
    target.Username = user.Username;
    target.Password = user.Password;
    target.Email = user.Email;
    target.Role = user.Role;
    target.JoinDate = user.JoinDate;
    target.Status = user.Status;
    db.SaveChanges();
});
//Delete user
app.MapPost("/user/delete/{id}",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "USER")]
    ([FromServices] ProfileServiceDbContext db, int userID) =>
{
    var target = db.UserProfile.Find(userID);
    db.UserProfile.Remove(target);
    db.SaveChanges();
});

app.Run();