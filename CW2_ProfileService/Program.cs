using CW2_ProfileService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AppDb");
builder.Services.AddDbContext<ProfileServiceDbContext>(x => x.UseSqlServer(connectionString));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});
var app = builder.Build();
app.UseSwaggerUI();
app.UseSwagger(x  => x.SerializeAsV2 = true);

// UserProfile API
//Get all users
app.MapGet("/users", ([FromServices] ProfileServiceDbContext db) =>
{
    return db.UserProfile.ToList();
});
//Get specific user
app.MapGet("/user/{id}", ([FromServices] ProfileServiceDbContext db, string userID) =>
{
    return db.UserProfile.Find(userID);
});
//Register user
app.MapPost("/user", ([FromServices] ProfileServiceDbContext db, UserProfile user) =>
{
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
app.MapPost("/user/delete/{id}", ([FromServices] ProfileServiceDbContext db, int userID) =>
{
    var target = db.UserProfile.Find(userID);
    db.UserProfile.Remove(target);
    db.SaveChanges();
});

// Hiking Group API
//Get all groups
app.MapGet("/group", ([FromServices] ProfileServiceDbContext db) =>
{
    return db.HikingGroups.ToList();
});
//Get specific group
app.MapGet("/group/{id}", ([FromServices] ProfileServiceDbContext db, string groupID) =>
{
    return db.HikingGroups.Find(groupID);
});
//New group
app.MapPost("/group", ([FromServices] ProfileServiceDbContext db, HikingGroups group) =>
{
    db.HikingGroups.Add(group);
    db.SaveChanges();
});
//Update group
app.MapPut("/group/update/{id}", ([FromServices] ProfileServiceDbContext db, int groupID, HikingGroups group) =>
{
    var target = db.HikingGroups.FirstOrDefault(u => u.groupID == groupID);
    target.creatorUserId = group.creatorUserId;
    target.groupName = group.groupName;
    target.description = group.description;
    db.SaveChanges();
});
//Delete group
app.MapPost("/group/delete/{id}", ([FromServices] ProfileServiceDbContext db, int groupID) =>
{
    var target = db.HikingGroups.Find(groupID);
    db.HikingGroups.Remove(target);
    db.SaveChanges();
});

//Hiking History API
//Get user HH
app.MapGet("/hikinghistory/{id}", ([FromServices] ProfileServiceDbContext db, int userID) =>
{
    return db.HikingHistory.Where(hh => hh.userID == userID);
});
//New HH
app.MapPost("/hikinghistory", ([FromServices] ProfileServiceDbContext db, HikingHistory hikingHistory) =>
{
    db.HikingHistory.Add(hikingHistory);
    db.SaveChanges();
});

//Joined Group API
//Get all joined group
app.MapGet("/joinedgroup", ([FromServices] ProfileServiceDbContext db) =>
{
    return db.JoinedHikingGroups.ToList();
});
//Get specific user joined groups
app.MapGet("/joinedgroup/{id}", ([FromServices] ProfileServiceDbContext db, int userID) =>
{
    return db.JoinedHikingGroups.Where(jg => jg.userID == userID);
});
//User join group
app.MapPost("/joinedgroup", ([FromServices] ProfileServiceDbContext db, JoinedHikingGroups joinedHikingGroups) =>
{
    db.JoinedHikingGroups.Add(joinedHikingGroups);
    db.SaveChanges();
});
//User leave group
app.MapPost("/joinedgroup/delete/{id}", ([FromServices] ProfileServiceDbContext db, int joinedGroupID) =>
{
    var target = db.JoinedHikingGroups.Find(joinedGroupID);
    db.JoinedHikingGroups.Remove(target);
    db.SaveChanges();
});

//Friends API
//Get all friends detail
app.MapGet("/friends", ([FromServices] ProfileServiceDbContext db) =>
{
    return db.Friends.ToList();
});
//Get specific user friends
app.MapGet("/friends/{id}", ([FromServices] ProfileServiceDbContext db, int userID) =>
{
    return db.Friends.Where(f => f.senderID == userID && f.receiverID == userID);
});
//Create new friends req
app.MapPost("/friends", ([FromServices] ProfileServiceDbContext db, Friends friend) =>
{
    db.Friends.Add(friend);
    var friendsKey = new FriendsKey { userID = friend.receiverID, friendID = friend.friendID };
    db.FriendsKey.Add(friendsKey);
    db.SaveChanges();
});
//Update friend req details
app.MapPut("/friends/update/{id}", ([FromServices] ProfileServiceDbContext db, int userID, String friendStatus) =>
{
    var target = db.Friends.FirstOrDefault(u => u.receiverID == userID);
    target.friendStatus = friendStatus;
    db.SaveChanges();
});
//Delete friend req
app.MapPost("/friends/delete/{id}", ([FromServices] ProfileServiceDbContext db, int friendID) =>
{
    var target = db.Friends.Find(friendID);
    db.Friends.Remove(target);
    var target2 = db.FriendsKey.Find(friendID);
    db.FriendsKey.Remove(target2);
    db.SaveChanges();
});

app.Run();