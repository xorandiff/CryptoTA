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

            modelBuilder.Entity("CryptoTA.Database.Models.Settings", b =>
                {
                    b.Property<int>("SettingsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SettingsId"), 1L, 1);

                    b.Property<int>("TimeIntervalId")
                        .HasColumnType("int");

                    b.Property<int>("TradingPairId")
                        .HasColumnType("int");

                    b.HasKey("SettingsId");

                    b.HasIndex("TimeIntervalId");

                    b.HasIndex("TradingPairId");

                    b.ToTable("Settings");
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

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Seconds")
                        .HasColumnType("bigint");

                    b.HasKey("TimeIntervalId");

                    b.ToTable("TimeIntervals");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.TradingPair", b =>
                {
                    b.Property<int>("TradingPairId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TradingPairId"), 1L, 1);

                    b.Property<string>("AlternativeName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("BaseDecimals")
                        .HasColumnType("bigint");

                    b.Property<string>("BaseName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BaseSymbol")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("CounterDecimals")
                        .HasColumnType("bigint");

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

            modelBuilder.Entity("CryptoTA.Database.Models.Credentials", b =>
                {
                    b.HasOne("CryptoTA.Database.Models.Market", "Market")
                        .WithMany("Credentials")
                        .HasForeignKey("MarketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Market");
                });

            modelBuilder.Entity("CryptoTA.Database.Models.Settings", b =>
                {
                    b.HasOne("CryptoTA.Database.Models.TimeInterval", "TimeInterval")
                        .WithMany()
                        .HasForeignKey("TimeIntervalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CryptoTA.Database.Models.TradingPair", "TradingPair")
                        .WithMany()
                        .HasForeignKey("TradingPairId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TimeInterval");

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