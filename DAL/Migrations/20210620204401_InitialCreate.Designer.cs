﻿// <auto-generated />
using System;
using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DAL.Migrations
{
    [DbContext(typeof(BotDbContext))]
    [Migration("20210620204401_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.7")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DAL.Guild", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("DiscordId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<int?>("SelectionRoomId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SelectionRoomId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("DAL.Model.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Emote")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("DAL.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("DisordId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<int?>("GameId")
                        .HasColumnType("int");

                    b.Property<int?>("GuildId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("GuildId");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("DAL.Room", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("DiscordId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<int?>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("DAL.Song", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("GuildId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("Songs");
                });

            modelBuilder.Entity("DAL.Guild", b =>
                {
                    b.HasOne("DAL.Room", "SelectionRoom")
                        .WithMany()
                        .HasForeignKey("SelectionRoomId");

                    b.Navigation("SelectionRoom");
                });

            modelBuilder.Entity("DAL.Role", b =>
                {
                    b.HasOne("DAL.Model.Game", "Game")
                        .WithMany()
                        .HasForeignKey("GameId");

                    b.HasOne("DAL.Guild", null)
                        .WithMany("Roles")
                        .HasForeignKey("GuildId");

                    b.Navigation("Game");
                });

            modelBuilder.Entity("DAL.Room", b =>
                {
                    b.HasOne("DAL.Role", null)
                        .WithMany("Rooms")
                        .HasForeignKey("RoleId");
                });

            modelBuilder.Entity("DAL.Song", b =>
                {
                    b.HasOne("DAL.Guild", null)
                        .WithMany("Songs")
                        .HasForeignKey("GuildId");
                });

            modelBuilder.Entity("DAL.Guild", b =>
                {
                    b.Navigation("Roles");

                    b.Navigation("Songs");
                });

            modelBuilder.Entity("DAL.Role", b =>
                {
                    b.Navigation("Rooms");
                });
#pragma warning restore 612, 618
        }
    }
}