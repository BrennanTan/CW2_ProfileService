using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CW2_ProfileService.Migrations
{
    /// <inheritdoc />
    public partial class initialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "CW2");

            migrationBuilder.CreateTable(
                name: "Friends",
                schema: "CW2",
                columns: table => new
                {
                    friendID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    receiverID = table.Column<int>(type: "int", nullable: false),
                    senderID = table.Column<int>(type: "int", nullable: false),
                    friendStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends", x => x.friendID);
                });

            migrationBuilder.CreateTable(
                name: "FriendsKey",
                schema: "CW2",
                columns: table => new
                {
                    friendsKeyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    friendID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendsKey", x => x.friendsKeyID);
                });

            migrationBuilder.CreateTable(
                name: "HikingGroups",
                schema: "CW2",
                columns: table => new
                {
                    groupID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    creatorUserId = table.Column<int>(type: "int", nullable: false),
                    groupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HikingGroups", x => x.groupID);
                });

            migrationBuilder.CreateTable(
                name: "HikingHistory",
                schema: "CW2",
                columns: table => new
                {
                    historyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    trailID = table.Column<int>(type: "int", nullable: false),
                    userID = table.Column<int>(type: "int", nullable: false),
                    hikeDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    distance = table.Column<float>(type: "real", nullable: false),
                    duration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    elevation = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HikingHistory", x => x.historyID);
                });

            migrationBuilder.CreateTable(
                name: "JoinedHikingGroups",
                schema: "CW2",
                columns: table => new
                {
                    joinedGroupID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    groupID = table.Column<int>(type: "int", nullable: false),
                    userID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinedHikingGroups", x => x.joinedGroupID);
                });

            migrationBuilder.CreateTable(
                name: "Trails",
                schema: "CW2",
                columns: table => new
                {
                    TrailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrailName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trails", x => x.TrailID);
                });

            migrationBuilder.CreateTable(
                name: "UserProfile",
                schema: "CW2",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfile", x => x.UserID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Friends",
                schema: "CW2");

            migrationBuilder.DropTable(
                name: "FriendsKey",
                schema: "CW2");

            migrationBuilder.DropTable(
                name: "HikingGroups",
                schema: "CW2");

            migrationBuilder.DropTable(
                name: "HikingHistory",
                schema: "CW2");

            migrationBuilder.DropTable(
                name: "JoinedHikingGroups",
                schema: "CW2");

            migrationBuilder.DropTable(
                name: "Trails",
                schema: "CW2");

            migrationBuilder.DropTable(
                name: "UserProfile",
                schema: "CW2");
        }
    }
}
