using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoTA.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Markets",
                columns: table => new
                {
                    MarketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CredentialsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markets", x => x.MarketId);
                });

            migrationBuilder.CreateTable(
                name: "StrategyCategories",
                columns: table => new
                {
                    StrategyCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyCategories", x => x.StrategyCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "TimeIntervals",
                columns: table => new
                {
                    TimeIntervalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsIndicatorInterval = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Seconds = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeIntervals", x => x.TimeIntervalId);
                });

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

            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    CredentialsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivateKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MarketId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => x.CredentialsId);
                    table.ForeignKey(
                        name: "FK_Credentials_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "MarketId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradingPairs",
                columns: table => new
                {
                    TradingPairId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlternativeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebsocketName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CounterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaseSymbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CounterSymbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaseDecimals = table.Column<int>(type: "int", nullable: false),
                    CounterDecimals = table.Column<int>(type: "int", nullable: false),
                    MinimalOrderAmount = table.Column<double>(type: "float", nullable: false),
                    MarketId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingPairs", x => x.TradingPairId);
                    table.ForeignKey(
                        name: "FK_TradingPairs_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "MarketId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarketOrderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MarketReferralOrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalCost = table.Column<double>(type: "float", nullable: false),
                    AveragePrice = table.Column<double>(type: "float", nullable: true),
                    StopPrice = table.Column<double>(type: "float", nullable: true),
                    LimitPrice = table.Column<double>(type: "float", nullable: true),
                    SecondaryPrice = table.Column<double>(type: "float", nullable: true),
                    Leverage = table.Column<double>(type: "float", nullable: true),
                    Fee = table.Column<double>(type: "float", nullable: false),
                    Volume = table.Column<double>(type: "float", nullable: false),
                    VolumeExecuted = table.Column<double>(type: "float", nullable: true),
                    OpenDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Miscellaneous = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Flags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TradingPairId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_TradingPairs_TradingPairId",
                        column: x => x.TradingPairId,
                        principalTable: "TradingPairs",
                        principalColumn: "TradingPairId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    SettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeIntervalIdChart = table.Column<int>(type: "int", nullable: false),
                    TimeIntervalIdIndicators = table.Column<int>(type: "int", nullable: false),
                    StrategyId = table.Column<int>(type: "int", nullable: false),
                    TradingPairId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.SettingsId);
                    table.ForeignKey(
                        name: "FK_Settings_TradingPairs_TradingPairId",
                        column: x => x.TradingPairId,
                        principalTable: "TradingPairs",
                        principalColumn: "TradingPairId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ticks",
                columns: table => new
                {
                    TickId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    High = table.Column<double>(type: "float", nullable: false),
                    Low = table.Column<double>(type: "float", nullable: false),
                    Open = table.Column<double>(type: "float", nullable: false),
                    Close = table.Column<double>(type: "float", nullable: false),
                    Volume = table.Column<double>(type: "float", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TradingPairId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticks", x => x.TickId);
                    table.ForeignKey(
                        name: "FK_Ticks_TradingPairs_TradingPairId",
                        column: x => x.TradingPairId,
                        principalTable: "TradingPairs",
                        principalColumn: "TradingPairId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Strategies",
                columns: table => new
                {
                    StrategyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinimalGain = table.Column<double>(type: "float", nullable: false),
                    MaximalLoss = table.Column<double>(type: "float", nullable: false),
                    BuyAmount = table.Column<double>(type: "float", nullable: false),
                    BuyPercentages = table.Column<double>(type: "float", nullable: false),
                    AskBeforeTrade = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    StrategyCategoryId = table.Column<int>(type: "int", nullable: false),
                    TradingPairId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strategies", x => x.StrategyId);
                    table.ForeignKey(
                        name: "FK_Strategies_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_Strategies_StrategyCategories_StrategyCategoryId",
                        column: x => x.StrategyCategoryId,
                        principalTable: "StrategyCategories",
                        principalColumn: "StrategyCategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Strategies_TradingPairs_TradingPairId",
                        column: x => x.TradingPairId,
                        principalTable: "TradingPairs",
                        principalColumn: "TradingPairId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Assets_MarketId",
                table: "Assets",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_MarketId",
                table: "Credentials",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TradingPairId",
                table: "Orders",
                column: "TradingPairId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_TradingPairId",
                table: "Settings",
                column: "TradingPairId");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_OrderId",
                table: "Strategies",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_StrategyCategoryId",
                table: "Strategies",
                column: "StrategyCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_TradingPairId",
                table: "Strategies",
                column: "TradingPairId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticks_TradingPairId",
                table: "Ticks",
                column: "TradingPairId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingPairs_MarketId",
                table: "TradingPairs",
                column: "MarketId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Credentials");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Strategies");

            migrationBuilder.DropTable(
                name: "Ticks");

            migrationBuilder.DropTable(
                name: "TimeIntervals");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "StrategyCategories");

            migrationBuilder.DropTable(
                name: "TradingPairs");

            migrationBuilder.DropTable(
                name: "Markets");
        }
    }
}
