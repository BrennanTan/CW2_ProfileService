using CW2_ProfileService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AppDb");
builder.Services.AddDbContext<ProfileServiceDbContext>(x => x.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwaggerUI();
app.UseSwagger(x  => x.SerializeAsV2 = true);

/*
app.MapGet("/Users", ([FromServices] ProfileServiceDbContext db) =>
{
    return db
});

app.MapGet("/User/{id}", ([FromServices] ProfileServiceDbContext db, string id) =>
{
    return db.Users.Where(x => x.UserID == id).FirstOrDefault();
});

/*
app.MapPut("/User/{id}", ([FromServices] UserProfileDbContext db, UserProfile user) =>
{
    db.Users.Update(user);
    db.SaveChanges();
    return db.Users.Where(x => x.userid == user.userid).FirstOrDefault();
});

app.MapPost("/User", ([FromServices] UserProfileDbContext db, UserProfile user) =>
{
    db.Users.Add(user);
    db.SaveChanges();
    return db.Users.ToList();
});
*/

app.Run();
