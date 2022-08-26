using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoTA.Migrations
{
    public partial class InitialCreate : Migration
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
                    BaseDecimals = table.Column<long>(type: "bigint", nullable: false),
                    CounterDecimals = table.Column<long>(type: "bigint", nullable: false),
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
                name: "Settings",
                columns: table => new
                {
                    SettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeIntervalIdChart = table.Column<int>(type: "int", nullable: false),
                    TimeIntervalIdIndicators = table.Column<int>(type: "int", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_MarketId",
                table: "Credentials",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_TradingPairId",
                table: "Settings",
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
                name: "Credentials");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Ticks");

            migrationBuilder.DropTable(
                name: "TimeIntervals");

            migrationBuilder.DropTable(
                name: "TradingPairs");

            migrationBuilder.DropTable(
                name: "Markets");
        }
    }
}
