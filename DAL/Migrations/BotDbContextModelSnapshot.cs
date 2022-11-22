﻿// <auto-generated />
using System;
using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAL.Migrations
{
    [DbContext(typeof(BotDbContext))]
    partial class BotDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DAL.Guild", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("ArchiveCategoryId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("DiscordId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("MemberRoleId")
                        .HasColumnType("integer");

                    b.Property<string>("Prefix")
                        .HasColumnType("text");

                    b.Property<decimal>("RuleMessageId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("RuleMessageText")
                        .HasColumnType("text");

                    b.Property<int?>("RuleRoomId")
                        .HasColumnType("integer");

                    b.Property<int?>("SelectionRoomId")
                        .HasColumnType("integer");

                    b.Property<decimal>("UserLeaveMessageRoomId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("MemberRoleId");

                    b.HasIndex("RuleRoomId");

                    b.HasIndex("SelectionRoomId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("DAL.Model.CompletedQuests", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("CompletedQuestsUserId")
                        .HasColumnType("integer");

                    b.Property<string>("GameName")
                        .HasColumnType("text");

                    b.Property<int>("QuestCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CompletedQuestsUserId");

                    b.ToTable("CompletedQuests");
                });

            modelBuilder.Entity("DAL.Model.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("ActiveCheckRoomId")
                        .HasColumnType("integer");

                    b.Property<string>("ActiveEmote")
                        .HasColumnType("text");

                    b.Property<decimal>("CategoryId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("GameRoleGameId")
                        .HasColumnType("integer");

                    b.Property<int?>("GuildId")
                        .HasColumnType("integer");

                    b.Property<bool>("HasActiveRole")
                        .HasColumnType("boolean");

                    b.Property<int?>("MainActiveRoleGameId")
                        .HasColumnType("integer");

                    b.Property<int?>("ModAcceptRoomId")
                        .HasColumnType("integer");

                    b.Property<int?>("ModQuestRoomId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<decimal>("SelectionMessageId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("ActiveCheckRoomId");

                    b.HasIndex("GameRoleGameId");

                    b.HasIndex("GuildId");

                    b.HasIndex("MainActiveRoleGameId");

                    b.HasIndex("ModAcceptRoomId");

                    b.HasIndex("ModQuestRoomId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("DAL.Model.Quest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("AuthorId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("GameName")
                        .HasColumnType("text");

                    b.Property<decimal>("ModMessage")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("QuestMessage")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("QuestMessageChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Score")
                        .HasColumnType("integer");

                    b.Property<decimal>("TakerId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Text")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Quests");
                });

            modelBuilder.Entity("DAL.Model.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("DiscordId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DAL.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("ActiveRoleGameId")
                        .HasColumnType("integer");

                    b.Property<string>("ChoiceEmote")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<decimal>("DisordId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("ModAcceptRoleGameId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<bool>("NeedsModApproval")
                        .HasColumnType("boolean");

                    b.Property<bool>("Resettable")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("ActiveRoleGameId");

                    b.HasIndex("ModAcceptRoleGameId");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("DAL.Room", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("DiscordId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("GameId")
                        .HasColumnType("integer");

                    b.Property<int?>("GuildId")
                        .HasColumnType("integer");

                    b.Property<decimal>("SpeakerRoleId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("TextForStageId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("GuildId");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("DAL.Song", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("GuildId")
                        .HasColumnType("integer");

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

            modelBuilder.Entity("DAL.Model.CompletedQuests", b =>
                {
                    b.HasOne("DAL.Model.User", null)
                        .WithMany("CompletedQuests")
                        .HasForeignKey("CompletedQuestsUserId");
                });

            modelBuilder.Entity("DAL.Model.Game", b =>
                {
                    b.HasOne("DAL.Room", "ActiveCheckRoom")
                        .WithMany()
                        .HasForeignKey("ActiveCheckRoomId");

                    b.HasOne("DAL.Role", "GameRole")
                        .WithMany()
                        .HasForeignKey("GameRoleGameId");

                    b.HasOne("DAL.Guild", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId");

                    b.HasOne("DAL.Role", "MainActiveRole")
                        .WithMany()
                        .HasForeignKey("MainActiveRoleGameId");

                    b.HasOne("DAL.Room", "ModAcceptRoom")
                        .WithMany()
                        .HasForeignKey("ModAcceptRoomId");

                    b.HasOne("DAL.Room", "ModQuestRoom")
                        .WithMany()
                        .HasForeignKey("ModQuestRoomId");

                    b.Navigation("ActiveCheckRoom");

                    b.Navigation("GameRole");

                    b.Navigation("Guild");

                    b.Navigation("MainActiveRole");

                    b.Navigation("ModAcceptRoom");

                    b.Navigation("ModQuestRoom");
                });

            modelBuilder.Entity("DAL.Role", b =>
                {
                    b.HasOne("DAL.Model.Game", null)
                        .WithMany("ActiveRoles")
                        .HasForeignKey("ActiveRoleGameId");

                    b.HasOne("DAL.Model.Game", null)
                        .WithMany("ModAcceptRoles")
                        .HasForeignKey("ModAcceptRoleGameId");
                });

            modelBuilder.Entity("DAL.Room", b =>
                {
                    b.HasOne("DAL.Model.Game", null)
                        .WithMany("Rooms")
                        .HasForeignKey("GameId");

                    b.HasOne("DAL.Guild", null)
                        .WithMany("StageChannels")
                        .HasForeignKey("GuildId");
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

                    b.Navigation("StageChannels");
                });

            modelBuilder.Entity("DAL.Model.Game", b =>
                {
                    b.Navigation("ActiveRoles");

                    b.Navigation("ModAcceptRoles");

                    b.Navigation("Rooms");
                });

            modelBuilder.Entity("DAL.Model.User", b =>
                {
                    b.Navigation("CompletedQuests");
                });
#pragma warning restore 612, 618
        }
    }
}
