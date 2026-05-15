using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAACO.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class M23_ModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "MemoryRecords",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingHash",
                table: "MemoryRecords",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingModel",
                table: "MemoryRecords",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingProvider",
                table: "MemoryRecords",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VectorRef",
                table: "MemoryRecords",
                type: "TEXT",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "MemoryRecords");

            migrationBuilder.DropColumn(
                name: "EmbeddingHash",
                table: "MemoryRecords");

            migrationBuilder.DropColumn(
                name: "EmbeddingModel",
                table: "MemoryRecords");

            migrationBuilder.DropColumn(
                name: "EmbeddingProvider",
                table: "MemoryRecords");

            migrationBuilder.DropColumn(
                name: "VectorRef",
                table: "MemoryRecords");
        }
    }
}
