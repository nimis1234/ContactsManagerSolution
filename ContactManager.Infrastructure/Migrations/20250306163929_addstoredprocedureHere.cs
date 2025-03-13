using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addstoredprocedureHere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // add stored procedure to get the list of person
            var sp_GetPersonList = @"CREATE PROCEDURE [dbo].[sp_GetAllPersons] 
                                    AS
                                    BEGIN
                                        SET NOCOUNT ON;
                                        SELECT * FROM Persons
                                    END";

            migrationBuilder.Sql(sp_GetPersonList);

            // add stored procedure to insert country
            var sp_InsertCountryName = @"CREATE PROCEDURE [dbo].[sp_InsertCountryName](@CountryName nvarchar(50),@CountryId uniqueidentifier)

                                    AS
                                    BEGIN
                                        SET NOCOUNT ON;
                                        INSERT INTO Countries (CountryName,CountryId) VALUES (@CountryName,@CountryId)
                                    END";


            migrationBuilder.Sql(sp_InsertCountryName);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // drop stored procedure
            var sp_GetPersonList = @"DROP PROCEDURE [dbo].[sp_GetAllPersons]";
            migrationBuilder.Sql(sp_GetPersonList);

            var sp_InsertCountryName = @"DROP PROCEDURE [dbo].[sp_InsertCountryName]";
            migrationBuilder.Sql(sp_InsertCountryName);
        }
    }
}
