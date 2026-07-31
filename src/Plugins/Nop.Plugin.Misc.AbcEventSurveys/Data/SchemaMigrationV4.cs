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
            Delete.Column(nameof(SurveyEvent.RedirectUrl)).FromTable(nameof(SurveyEvent));
        }

        public override void Down()
        {
            Alter.Table(nameof(SurveyEvent))
                .AddColumn(nameof(SurveyEvent.RedirectUrl)).AsString(400).Nullable();
        }
    }
}
