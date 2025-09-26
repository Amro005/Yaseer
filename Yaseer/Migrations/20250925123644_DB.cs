using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Yaseer.Migrations
{
    /// <inheritdoc />
    public partial class DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DisabilityTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DisabilityTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DisabilityTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "DisabilityTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "DisabilityTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "DisabilityTypes",
                keyColumn: "Id",
                keyValue: 6);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DisabilityTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "صعوبات في الحركة والتنقل", "إعاقة حركية" },
                    { 2, "صعوبات في الرؤية", "إعاقة بصرية" },
                    { 3, "صعوبات في السمع", "إعاقة سمعية" },
                    { 4, "صعوبات في التعلم والفهم", "إعاقة ذهنية" },
                    { 5, "صعوبات في الكلام والتواصل", "إعاقة نطقية" },
                    { 6, "اضطرابات طيف التوحد", "طيف التوحد" }
                });
        }
    }
}
