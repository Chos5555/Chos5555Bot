﻿// <auto-generated />
using System;
using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DAL.Migrations
{
    [DbContext(typeof(BotDbContext))]
    [Migration("20221008175848_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("DAL.Guild", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("ArchiveCategoryId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("DiscordId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<int?>("MemberRoleId")
                        .HasColumnType("int");

                    b.Property<decimal>("RuleMessageId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("RuleMessageText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RuleRoomId")
                        .HasColumnType("int");

                    b.Property<int?>("SelectionRoomId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("MemberRoleId");

                    b.HasIndex("RuleRoomId");

                    b.HasIndex("SelectionRoomId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("DAL.Model.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int?>("ActiveCheckRoomId")
                        .HasColumnType("int");

                    b.Property<string>("ActiveEmote")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("GameRoleId")
                        .HasColumnType("int");

                    b.Property<int?>("GuildId")
                        .HasColumnType("int");

                    b.Property<bool>("HasActiveRole")
                        .HasColumnType("bit");

                    b.Property<int?>("ModAcceptRoomId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("SelectionMessageId")
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("ActiveCheckRoomId");

                    b.HasIndex("GameRoleId");

                    b.HasIndex("GuildId");

                    b.HasIndex("ModAcceptRoomId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("DAL.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("DisordId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("Emote")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("GameId")
                        .HasColumnType("int");

                    b.Property<int?>("GameId1")
                        .HasColumnType("int");

                    b.Property<bool>("NeedsModApproval")
                        .HasColumnType("bit");

                    b.Property<bool>("Resetable")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("GameId1");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("DAL.Room", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("DiscordId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<int?>("GameId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("DAL.Song", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int?>("GuildId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("Songs");
                });

            modelBuilder.Entity("DAL.Guild", b =>
                {
                    b.HasOne("DAL.Role", "MemberRole")
                        .WithMany()
                        .HasForeignKey("MemberRoleId");

                    b.HasOne("DAL.Room", "RuleRoom")
                        .WithMany()
                        .HasForeignKey("RuleRoomId");

                    b.HasOne("DAL.Room", "SelectionRoom")
                        .WithMany()
                        .HasForeignKey("SelectionRoomId");

                    b.Navigation("MemberRole");

                    b.Navigation("RuleRoom");

                    b.Navigation("SelectionRoom");
                });

            modelBuilder.Entity("DAL.Model.Game", b =>
                {
                    b.HasOne("DAL.Room", "ActiveCheckRoom")
                        .WithMany()
                        .HasForeignKey("ActiveCheckRoomId");

                    b.HasOne("DAL.Role", "GameRole")
                        .WithMany()
                        .HasForeignKey("GameRoleId");

                    b.HasOne("DAL.Guild", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId");

                    b.HasOne("DAL.Room", "ModAcceptRoom")
                        .WithMany()
                        .HasForeignKey("ModAcceptRoomId");

                    b.Navigation("ActiveCheckRoom");

                    b.Navigation("GameRole");

                    b.Navigation("Guild");

                    b.Navigation("ModAcceptRoom");
                });

            modelBuilder.Entity("DAL.Role", b =>
                {
                    b.HasOne("DAL.Model.Game", null)
                        .WithMany("ActiveRoles")
                        .HasForeignKey("GameId");

                    b.HasOne("DAL.Model.Game", null)
                        .WithMany("ModAcceptRoles")
                        .HasForeignKey("GameId1");
                });

            modelBuilder.Entity("DAL.Room", b =>
                {
                    b.HasOne("DAL.Model.Game", null)
                        .WithMany("Rooms")
                        .HasForeignKey("GameId");
                });

            modelBuilder.Entity("DAL.Song", b =>
                {
                    b.HasOne("DAL.Guild", null)
                        .WithMany("Songs")
                        .HasForeignKey("GuildId");
                });

            modelBuilder.Entity("DAL.Guild", b =>
                {
                    b.Navigation("Songs");
                });

            modelBuilder.Entity("DAL.Model.Game", b =>
                {
                    b.Navigation("ActiveRoles");

                    b.Navigation("ModAcceptRoles");

                    b.Navigation("Rooms");
                });
#pragma warning restore 612, 618
        }
    }
}
