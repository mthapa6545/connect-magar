using Microsoft.EntityFrameworkCore.Migrations;

namespace ConnectMagar.Migrations
{
    public partial class PersonTableUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Person_Address_NepalAddressID",
                table: "Person");

            migrationBuilder.DropForeignKey(
                name: "FK_Person_Address_USAAddressID",
                table: "Person");

            migrationBuilder.RenameColumn(
                name: "USAAddressID",
                table: "Person",
                newName: "USAAddressAddressID");

            migrationBuilder.RenameColumn(
                name: "NepalAddressID",
                table: "Person",
                newName: "NepalAddressAddressID");

            migrationBuilder.RenameIndex(
                name: "IX_Person_USAAddressID",
                table: "Person",
                newName: "IX_Person_USAAddressAddressID");

            migrationBuilder.RenameIndex(
                name: "IX_Person_NepalAddressID",
                table: "Person",
                newName: "IX_Person_NepalAddressAddressID");

            migrationBuilder.RenameColumn(
                name: "StreetAddress",
                table: "Address",
                newName: "StreetName");

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Address_NepalAddressAddressID",
                table: "Person",
                column: "NepalAddressAddressID",
                principalTable: "Address",
                principalColumn: "AddressID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Address_USAAddressAddressID",
                table: "Person",
                column: "USAAddressAddressID",
                principalTable: "Address",
                principalColumn: "AddressID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Person_Address_NepalAddressAddressID",
                table: "Person");

            migrationBuilder.DropForeignKey(
                name: "FK_Person_Address_USAAddressAddressID",
                table: "Person");

            migrationBuilder.RenameColumn(
                name: "USAAddressAddressID",
                table: "Person",
                newName: "USAAddressID");

            migrationBuilder.RenameColumn(
                name: "NepalAddressAddressID",
                table: "Person",
                newName: "NepalAddressID");

            migrationBuilder.RenameIndex(
                name: "IX_Person_USAAddressAddressID",
                table: "Person",
                newName: "IX_Person_USAAddressID");

            migrationBuilder.RenameIndex(
                name: "IX_Person_NepalAddressAddressID",
                table: "Person",
                newName: "IX_Person_NepalAddressID");

            migrationBuilder.RenameColumn(
                name: "StreetName",
                table: "Address",
                newName: "StreetAddress");

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Address_NepalAddressID",
                table: "Person",
                column: "NepalAddressID",
                principalTable: "Address",
                principalColumn: "AddressID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Address_USAAddressID",
                table: "Person",
                column: "USAAddressID",
                principalTable: "Address",
                principalColumn: "AddressID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
