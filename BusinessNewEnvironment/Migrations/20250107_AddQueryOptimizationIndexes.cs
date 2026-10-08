using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessNewEnvironment.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryOptimizationIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add indexes for commonly searched/filtered columns to improve query performance
            // This is part of Tier 1 optimization: query-level improvements

            // Businesses table indexes
            migrationBuilder.CreateIndex(
                name: "IX_Businesses_EmailId",
                table: "Businesses",
                column: "EmailId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_SubCategoryID",
                table: "Businesses",
                column: "SubCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_CategoryID",
                table: "Businesses",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_RoleID",
                table: "Businesses",
                column: "RoleID");

            // Composite index for category + subcategory searches
            migrationBuilder.CreateIndex(
                name: "IX_Businesses_SubCategoryID_CategoryID",
                table: "Businesses",
                columns: new[] { "SubCategoryID", "CategoryID" });

            // SubCategories table indexes
            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CategoryID",
                table: "SubCategories",
                column: "CategoryID");

            // Customers table indexes
            migrationBuilder.CreateIndex(
                name: "IX_Customers_Cus_EmailId",
                table: "Customers",
                column: "Cus_EmailId",
                unique: true);

            // BusinessRatings table indexes
            migrationBuilder.CreateIndex(
                name: "IX_BusinessRatings_BusinessID",
                table: "BusinessRatings",
                column: "BusinessID");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessRatings_RatedBy",
                table: "BusinessRatings",
                column: "RatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessRatings_BusinessID_RatedBy",
                table: "BusinessRatings",
                columns: new[] { "BusinessID", "RatedBy" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all indexes created in Up()
            migrationBuilder.DropIndex(
                name: "IX_Businesses_EmailId",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_SubCategoryID",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_CategoryID",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_RoleID",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_SubCategoryID_CategoryID",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_SubCategories_CategoryID",
                table: "SubCategories");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Cus_EmailId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_BusinessRatings_BusinessID",
                table: "BusinessRatings");

            migrationBuilder.DropIndex(
                name: "IX_BusinessRatings_RatedBy",
                table: "BusinessRatings");

            migrationBuilder.DropIndex(
                name: "IX_BusinessRatings_BusinessID_RatedBy",
                table: "BusinessRatings");
        }
    }
}
