using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yaseer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppointmentCreateRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedsTransport",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TransportAddress",
                table: "Appointments",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImageUrl",
                value: "~/images/Home_1.jpg");

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImageUrl",
                value: "~/images/Home_1.jpg");

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImageUrl",
                value: "~/images/Home_1.jpg");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsTransport",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "TransportAddress",
                table: "Appointments");

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImageUrl",
                value: "/images/clinic1.jpg");

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImageUrl",
                value: "/images/clinic2.jpg");

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImageUrl",
                value: "/images/clinic3.jpg");
        }
    }
}
