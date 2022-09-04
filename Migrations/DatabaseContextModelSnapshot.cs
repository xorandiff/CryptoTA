﻿// <auto-generated />
using System;
using CryptoTA.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CryptoTA.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("CryptoTA.Database.Models.Asset", b =>
                {
                    b.Property<int>("AssetId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AssetId"), 1L, 1);

                    b.Property<string>("AlternativeSymbol")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Decimals")
                        .HasColumnType("bigint");

                    b.Property<int>("MarketId")
                        .HasColumnType("int");

                    b.Property<string>("MarketName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AssetId");

                    b.HasIndex("MarketId");

                    b.ToTable("Assets");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Credentials", b =>
                {
                    b.Property<int>("CredentialsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CredentialsId"), 1L, 1);

                    b.Property<int>("MarketId")
                        .HasColumnType("int");

                    b.Property<string>("PrivateKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PublicKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CredentialsId");

                    b.HasIndex("MarketId");

                    b.ToTable("Credentials");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Market", b =>
                {
                    b.Property<int>("MarketId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MarketId"), 1L, 1);

                    b.Property<bool>("CredentialsRequired")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MarketId");

                    b.ToTable("Markets");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Order", b =>
                {
                    b.Property<int>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("OrderId"), 1L, 1);

                    b.Property<double?>("AveragePrice")
                        .HasColumnType("float");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpireDate")
                        .HasColumnType("datetime2");

                    b.Property<double>("Fee")
                        .HasColumnType("float");

                    b.Property<string>("Flags")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("Leverage")
                        .HasColumnType("float");

                    b.Property<double?>("LimitPrice")
                        .HasColumnType("float");

                    b.Property<string>("MarketOrderId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MarketReferralOrderId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Miscellaneous")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("OpenDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OrderType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("SecondaryPrice")
                        .HasColumnType("float");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("StopPrice")
                        .HasColumnType("float");

                    b.Property<double>("TotalCost")
                        .HasColumnType("float");

                    b.Property<int>("TradingPairId")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserReferenceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Volume")
                        .HasColumnType("float");

                    b.Property<double?>("VolumeExecuted")
                        .HasColumnType("float");

                    b.HasKey("OrderId");

                    b.HasIndex("TradingPairId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Settings", b =>
                {
                    b.Property<int>("SettingsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SettingsId"), 1L, 1);

                    b.Property<int>("StrategyId")
                        .HasColumnType("int");

                    b.Property<int>("TimeIntervalIdChart")
                        .HasColumnType("int");

                    b.Property<int>("TimeIntervalIdIndicators")
                        .HasColumnType("int");

                    b.Property<int>("TradingPairId")
                        .HasColumnType("int");

                    b.HasKey("SettingsId");

                    b.HasIndex("TradingPairId");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Strategy", b =>
                {
                    b.Property<int>("StrategyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StrategyId"), 1L, 1);

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<bool>("AskBeforeTrade")
                        .HasColumnType("bit");

                    b.Property<double>("BuyAmount")
                        .HasColumnType("float");

                    b.Property<double>("BuyPercentages")
                        .HasColumnType("float");

                    b.Property<double>("MaximalLoss")
                        .HasColumnType("float");

                    b.Property<double>("MinimalGain")
                        .HasColumnType("float");

                    b.Property<int?>("OrderId")
                        .HasColumnType("int");

                    b.Property<int>("StrategyCategoryId")
                        .HasColumnType("int");

                    b.Property<int>("TradingPairId")
                        .HasColumnType("int");

                    b.HasKey("StrategyId");

                    b.HasIndex("OrderId");

                    b.HasIndex("StrategyCategoryId");

                    b.HasIndex("TradingPairId");

                    b.ToTable("Strategies");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.StrategyCategory", b =>
                {
                    b.Property<int>("StrategyCategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StrategyCategoryId"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("StrategyCategoryId");

                    b.ToTable("StrategyCategories");

                    b.HasData(
                        new
                        {
                            StrategyCategoryId = 1,
                            Name = "Rapid (less than an hour)"
                        },
                        new
                        {
                            StrategyCategoryId = 2,
                            Name = "Short (less than a day)"
                        },
                        new
                        {
                            StrategyCategoryId = 3,
                            Name = "Medium (less than 3 days)"
                        },
                        new
                        {
                            StrategyCategoryId = 4,
                            Name = "Long (less than a week)"
                        });
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Tick", b =>
                {
                    b.Property<int>("TickId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TickId"), 1L, 1);

                    b.Property<double>("Close")
                        .HasColumnType("float");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<double>("High")
                        .HasColumnType("float");

                    b.Property<double>("Low")
                        .HasColumnType("float");

                    b.Property<double>("Open")
                        .HasColumnType("float");

                    b.Property<int>("TradingPairId")
                        .HasColumnType("int");

                    b.Property<double>("Volume")
                        .HasColumnType("float");

                    b.HasKey("TickId");

                    b.HasIndex("TradingPairId");

                    b.ToTable("Ticks");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.TimeInterval", b =>
                {
                    b.Property<int>("TimeIntervalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TimeIntervalId"), 1L, 1);

                    b.Property<bool>("IsIndicatorInterval")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Seconds")
                        .HasColumnType("bigint");

                    b.HasKey("TimeIntervalId");

                    b.ToTable("TimeIntervals");

                    b.HasData(
                        new
                        {
                            TimeIntervalId = 1,
                            IsIndicatorInterval = false,
                            Name = "1 day",
                            Seconds = 86400L
                        },
                        new
                        {
                            TimeIntervalId = 2,
                            IsIndicatorInterval = false,
                            Name = "3 days",
                            Seconds = 259200L
                        },
                        new
                        {
                            TimeIntervalId = 3,
                            IsIndicatorInterval = false,
                            Name = "1 week",
                            Seconds = 604800L
                        },
                        new
                        {
                            TimeIntervalId = 4,
                            IsIndicatorInterval = false,
                            Name = "2 weeks",
                            Seconds = 1209600L
                        },
                        new
                        {
                            TimeIntervalId = 5,
                            IsIndicatorInterval = false,
                            Name = "1 month",
                            Seconds = 2678400L
                        },
                        new
                        {
                            TimeIntervalId = 6,
                            IsIndicatorInterval = false,
                            Name = "3 months",
                            Seconds = 8035200L
                        },
                        new
                        {
                            TimeIntervalId = 7,
                            IsIndicatorInterval = false,
                            Name = "6 months",
                            Seconds = 16070400L
                        },
                        new
                        {
                            TimeIntervalId = 8,
                            IsIndicatorInterval = false,
                            Name = "1 year",
                            Seconds = 32140800L
                        },
                        new
                        {
                            TimeIntervalId = 9,
                            IsIndicatorInterval = false,
                            Name = "5 years",
                            Seconds = 160704000L
                        },
                        new
                        {
                            TimeIntervalId = 10,
                            IsIndicatorInterval = true,
                            Name = "1 minute",
                            Seconds = 60L
                        },
                        new
                        {
                            TimeIntervalId = 11,
                            IsIndicatorInterval = true,
                            Name = "5 minutes",
                            Seconds = 300L
                        },
                        new
                        {
                            TimeIntervalId = 12,
                            IsIndicatorInterval = true,
                            Name = "15 minutes",
                            Seconds = 900L
                        },
                        new
                        {
                            TimeIntervalId = 13,
                            IsIndicatorInterval = true,
                            Name = "30 minutes",
                            Seconds = 1800L
                        },
                        new
                        {
                            TimeIntervalId = 14,
                            IsIndicatorInterval = true,
                            Name = "1 hour",
                            Seconds = 3600L
                        },
                        new
                        {
                            TimeIntervalId = 15,
                            IsIndicatorInterval = true,
                            Name = "2 hours",
                            Seconds = 7200L
                        },
                        new
                        {
                            TimeIntervalId = 16,
                            IsIndicatorInterval = true,
                            Name = "4 hours",
                            Seconds = 14400L
                        },
                        new
                        {
                            TimeIntervalId = 17,
                            IsIndicatorInterval = true,
                            Name = "1 day",
                            Seconds = 86400L
                        },
                        new
                        {
                            TimeIntervalId = 18,
                            IsIndicatorInterval = true,
                            Name = "1 week",
                            Seconds = 604800L
                        },
                        new
                        {
                            TimeIntervalId = 19,
                            IsIndicatorInterval = true,
                            Name = "1 month",
                            Seconds = 2678400L
                        });
                });

            modelBuilder.Entity("CryptoTA.Database.Models.TradingPair", b =>
                {
                    b.Property<int>("TradingPairId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TradingPairId"), 1L, 1);

                    b.Property<string>("AlternativeName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("BaseDecimals")
                        .HasColumnType("int");

                    b.Property<string>("BaseName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BaseSymbol")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CounterDecimals")
                        .HasColumnType("int");

                    b.Property<string>("CounterName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CounterSymbol")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MarketId")
                        .HasColumnType("int");

                    b.Property<double>("MinimalOrderAmount")
                        .HasColumnType("float");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WebsocketName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TradingPairId");

                    b.HasIndex("MarketId");

                    b.ToTable("TradingPairs");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Asset", b =>
                {
                    b.HasOne("CryptoTA.Database.Models.Market", "Market")
                        .WithMany()
                        .HasForeignKey("MarketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Market");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Credentials", b =>
                {
                    b.HasOne("CryptoTA.Database.Models.Market", "Market")
                        .WithMany("Credentials")
                        .HasForeignKey("MarketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Market");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Order", b =>
                {
                    b.HasOne("CryptoTA.Database.Models.TradingPair", "TradingPair")
                        .WithMany()
                        .HasForeignKey("TradingPairId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TradingPair");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Settings", b =>
                {
                    b.HasOne("CryptoTA.Database.Models.TradingPair", "TradingPair")
                        .WithMany()
                        .HasForeignKey("TradingPairId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TradingPair");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Strategy", b =>
                {
                    b.HasOne("CryptoTA.Database.Models.Order", "Order")
                        .WithMany()
                        .HasForeignKey("OrderId");

                    b.HasOne("CryptoTA.Database.Models.StrategyCategory", "StrategyCategory")
                        .WithMany()
                        .HasForeignKey("StrategyCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CryptoTA.Database.Models.TradingPair", "TradingPair")
                        .WithMany()
                        .HasForeignKey("TradingPairId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("StrategyCategory");

                    b.Navigation("TradingPair");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Tick", b =>
                {
                    b.HasOne("CryptoTA.Database.Models.TradingPair", "TradingPair")
                        .WithMany("Ticks")
                        .HasForeignKey("TradingPairId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TradingPair");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.TradingPair", b =>
                {
                    b.HasOne("CryptoTA.Database.Models.Market", "Market")
                        .WithMany("TradingPairs")
                        .HasForeignKey("MarketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Market");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Market", b =>
                {
                    b.Navigation("Credentials");

                    b.Navigation("TradingPairs");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.TradingPair", b =>
                {
                    b.Navigation("Ticks");
                });
#pragma warning restore 612, 618
        }
    }
}
