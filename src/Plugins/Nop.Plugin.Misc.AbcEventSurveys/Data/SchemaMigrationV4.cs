using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AbcEventSurveys.Domain;

namespace Nop.Plugin.Misc.AbcEventSurveys.Data
{
    [NopMigration("2026/07/31 00:00:00:0000000", "Misc.AbcEventSurveys - removed post-submission redirect feature")]
    public class SchemaMigrationV4 : Migration
    {
        public override void Up()
        {
            // RedirectUrl no longer exists on SurveyEvent, so it's referenced by string literal here.
            Delete.Column("RedirectUrl").FromTable(nameof(SurveyEvent));
        }

        public override void Down()
        {
            Alter.Table(nameof(SurveyEvent))
                .AddColumn("RedirectUrl").AsString(400).Nullable();
        }
    }
}
