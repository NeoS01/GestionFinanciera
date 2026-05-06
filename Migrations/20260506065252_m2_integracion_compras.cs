using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GestionFinanciera.Migrations
{
    /// <inheritdoc />
    public partial class m2_integracion_compras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PagoProveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodigoPago = table.Column<string>(type: "text", nullable: false),
                    CodigoOrden = table.Column<string>(type: "text", nullable: false),
                    FacturaNumero = table.Column<string>(type: "text", nullable: false),
                    FacturaArchivoUrl = table.Column<string>(type: "text", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Proveedor = table.Column<string>(type: "text", nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecibidoConforme = table.Column<bool>(type: "boolean", nullable: false),
                    EstadoPago = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagoProveedor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PresupuestoMensual",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Periodo = table.Column<string>(type: "text", nullable: false),
                    MontoAsignado = table.Column<decimal>(type: "numeric", nullable: false),
                    MontoDisponible = table.Column<decimal>(type: "numeric", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestoMensual", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagoProveedor");

            migrationBuilder.DropTable(
                name: "PresupuestoMensual");
        }
    }
}
