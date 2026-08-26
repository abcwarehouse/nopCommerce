using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AbcEventSurveys.Domain;

namespace Nop.Plugin.Misc.AbcEventSurveys.Data
{
    [NopMigration("2026/07/27 20:00:00:0000000", "Misc.AbcEventSurveys - added SurveyEvent.PictureId")]
    public class SchemaMigrationV2 : Migration
    {
        public override void Up()
        {
            Alter.Table(nameof(SurveyEvent))
                .AddColumn(nameof(SurveyEvent.PictureId)).AsInt32().NotNullable().WithDefaultValue(0);
        }

        public override void Down()
        {
            Delete.Column(nameof(SurveyEvent.PictureId)).FromTable(nameof(SurveyEvent));
        }
    }
}
