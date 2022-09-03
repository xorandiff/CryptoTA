using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoTA.Migrations
{
    public partial class AssetsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StrategyCategories",
                keyColumn: "StrategyCategoryId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "StrategyCategories",
                keyColumn: "StrategyCategoryId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "StrategyCategories",
                keyColumn: "StrategyCategoryId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "StrategyCategories",
                keyColumn: "StrategyCategoryId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "TimeIntervals",
                keyColumn: "TimeIntervalId",
                keyValue: 19);

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    AssetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarketName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlternativeSymbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Decimals = table.Column<long>(type: "bigint", nullable: false),
                    MarketId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_Assets_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "MarketId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_MarketId",
                table: "Assets",
                column: "MarketId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.InsertData(
                table: "StrategyCategories",
                columns: new[] { "StrategyCategoryId", "Name" },
                values: new object[,]
                {
                    { 1, "Rapid (less than an hour)" },
                    { 2, "Short (less than a day)" },
                    { 3, "Medium (less than 3 days)" },
                    { 4, "Long (less than a week)" }
                });

            migrationBuilder.InsertData(
                table: "TimeIntervals",
                columns: new[] { "TimeIntervalId", "IsIndicatorInterval", "Name", "Seconds" },
                values: new object[,]
                {
                    { 1, false, "1 day", 86400L },
                    { 2, false, "3 days", 259200L },
                    { 3, false, "1 week", 604800L },
                    { 4, false, "2 weeks", 1209600L },
                    { 5, false, "1 month", 2678400L },
                    { 6, false, "3 months", 8035200L },
                    { 7, false, "6 months", 16070400L },
                    { 8, false, "1 year", 32140800L },
                    { 9, false, "5 years", 160704000L },
                    { 10, true, "1 minute", 60L },
                    { 11, true, "5 minutes", 300L },
                    { 12, true, "15 minutes", 900L },
                    { 13, true, "30 minutes", 1800L },
                    { 14, true, "1 hour", 3600L },
                    { 15, true, "2 hours", 7200L },
                    { 16, true, "4 hours", 14400L },
                    { 17, true, "1 day", 86400L },
                    { 18, true, "1 week", 604800L },
                    { 19, true, "1 month", 2678400L }
                });
        }
    }
}
