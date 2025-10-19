using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptTemplateManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create FTS5 virtual table for template search
            migrationBuilder.Sql(@"
                CREATE VIRTUAL TABLE TemplatesFts USING fts5(
                    Id UNINDEXED,
                    Name,
                    Content,
                    tokenize='porter unicode61'
                );
            ");

            // Populate FTS table with existing data
            migrationBuilder.Sql(@"
                INSERT INTO TemplatesFts(Id, Name, Content)
                SELECT Id, Name, Content FROM Templates;
            ");

            // Trigger: Insert into FTS when new template is created
            migrationBuilder.Sql(@"
                CREATE TRIGGER Templates_Insert_Fts
                AFTER INSERT ON Templates
                BEGIN
                    INSERT INTO TemplatesFts(Id, Name, Content)
                    VALUES (NEW.Id, NEW.Name, NEW.Content);
                END;
            ");

            // Trigger: Update FTS when template is updated
            migrationBuilder.Sql(@"
                CREATE TRIGGER Templates_Update_Fts
                AFTER UPDATE ON Templates
                BEGIN
                    UPDATE TemplatesFts
                    SET Name = NEW.Name, Content = NEW.Content
                    WHERE Id = OLD.Id;
                END;
            ");

            // Trigger: Delete from FTS when template is deleted
            migrationBuilder.Sql(@"
                CREATE TRIGGER Templates_Delete_Fts
                AFTER DELETE ON Templates
                BEGIN
                    DELETE FROM TemplatesFts WHERE Id = OLD.Id;
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop triggers first
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS Templates_Insert_Fts;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS Templates_Update_Fts;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS Templates_Delete_Fts;");

            // Drop FTS5 virtual table
            migrationBuilder.Sql("DROP TABLE IF EXISTS TemplatesFts;");
        }
    }
}
