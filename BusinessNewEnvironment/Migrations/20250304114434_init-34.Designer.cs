﻿// <auto-generated />
using System;
using BusinessNewEnvironment.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BusinessNewEnvironment.Migrations
{
    [DbContext(typeof(BusinessContext))]
    [Migration("20250304114434_init-34")]
    partial class init34
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BusinessNewEnvironment.Models.AdminLoginRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("AdminPassword")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EmailId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsPasswordChanged")
                        .HasColumnType("boolean");

                    b.Property<int>("RoleId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("AdminLoginRequests");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.Busines", b =>
                {
                    b.Property<int>("BusinessID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("BusinessID"));

                    b.Property<int?>("CategoryID")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("EmailId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<string>("Location")
                        .HasColumnType("text");

                    b.Property<double>("Longitude")
                        .HasColumnType("double precision");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .HasColumnType("text");

                    b.Property<int>("RoleID")
                        .HasColumnType("integer");

                    b.Property<int>("SubCategoryID")
                        .HasColumnType("integer");

                    b.Property<string>("VisitingCard")
                        .HasColumnType("text");

                    b.HasKey("BusinessID");

                    b.HasIndex("RoleID");

                    b.HasIndex("SubCategoryID");

                    b.ToTable("Businesses");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.BusinessRatings", b =>
                {
                    b.Property<int>("BusinessRatingID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("BusinessRatingID"));

                    b.Property<int>("BusinessID")
                        .HasColumnType("integer");

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Rating")
                        .HasColumnType("decimal(3,2)");

                    b.HasKey("BusinessRatingID");

                    b.HasIndex("BusinessID");

                    b.ToTable("BusinessRatings");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.Category", b =>
                {
                    b.Property<int>("CategoryID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CategoryID"));

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("CategoryID");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.Customer", b =>
                {
                    b.Property<int>("Cus_Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Cus_Id"));

                    b.Property<string>("Cus_EmailId")
                        .HasColumnType("text");

                    b.Property<string>("Cus_Location")
                        .HasColumnType("text");

                    b.Property<string>("Cus_Password")
                        .HasColumnType("text");

                    b.Property<double>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<double>("Longitude")
                        .HasColumnType("double precision");

                    b.Property<int>("RoleID")
                        .HasColumnType("integer");

                    b.HasKey("Cus_Id");

                    b.HasIndex("RoleID");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.LoginRequest", b =>
                {
                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("RememberMe")
                        .HasColumnType("boolean");

                    b.HasKey("Username");

                    b.ToTable("loginRequests");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.Role", b =>
                {
                    b.Property<int>("RoleID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("RoleID"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("RoleID");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.SubCategory", b =>
                {
                    b.Property<int>("SubCategoryID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("SubCategoryID"));

                    b.Property<int>("CategoryID")
                        .HasColumnType("integer");

                    b.Property<string>("SubCategoryName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("SubCategoryID");

                    b.HasIndex("CategoryID");

                    b.ToTable("SubCategories");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.Busines", b =>
                {
                    b.HasOne("BusinessNewEnvironment.Models.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessNewEnvironment.Models.SubCategory", "SubCategory")
                        .WithMany("Businesses")
                        .HasForeignKey("SubCategoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("SubCategory");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.BusinessRatings", b =>
                {
                    b.HasOne("BusinessNewEnvironment.Models.Busines", "Business")
                        .WithMany("BusinessRatings")
                        .HasForeignKey("BusinessID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Business");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.Customer", b =>
                {
                    b.HasOne("BusinessNewEnvironment.Models.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.SubCategory", b =>
                {
                    b.HasOne("BusinessNewEnvironment.Models.Category", "Category")
                        .WithMany("SubCategories")
                        .HasForeignKey("CategoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.Busines", b =>
                {
                    b.Navigation("BusinessRatings");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.Category", b =>
                {
                    b.Navigation("SubCategories");
                });

            modelBuilder.Entity("BusinessNewEnvironment.Models.SubCategory", b =>
                {
                    b.Navigation("Businesses");
                });
#pragma warning restore 612, 618
        }
    }
}
