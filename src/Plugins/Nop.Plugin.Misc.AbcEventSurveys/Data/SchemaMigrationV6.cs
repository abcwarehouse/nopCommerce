using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AbcEventSurveys.Domain;

namespace Nop.Plugin.Misc.AbcEventSurveys.Data
{
    [NopMigration("2026/10/28 00:00:00:0000000", "Misc.AbcEventSurveys - added SurveyResponse traffic source columns")]
    public class SchemaMigrationV6 : Migration
    {
        public override void Up()
        {
            Alter.Table(nameof(SurveyResponse))
                .AddColumn(nameof(SurveyResponse.TrafficSource)).AsString(200).Nullable()
                .AddColumn(nameof(SurveyResponse.TrafficMedium)).AsString(200).Nullable();
        }

        public override void Down()
        {
            Delete.Column(nameof(SurveyResponse.TrafficSource))
                .Column(nameof(SurveyResponse.TrafficMedium))
                .FromTable(nameof(SurveyResponse));
        }
    }
}
