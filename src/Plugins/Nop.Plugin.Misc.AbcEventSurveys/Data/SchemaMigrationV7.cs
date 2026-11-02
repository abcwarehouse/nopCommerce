using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AbcEventSurveys.Domain;

namespace Nop.Plugin.Misc.AbcEventSurveys.Data
{
    [NopMigration("2026/11/02 00:00:00:0000000", "Misc.AbcEventSurveys - convert existing SurveyEvent.EndDateUtc values from Detroit time to UTC")]
    public class SchemaMigrationV7 : Migration
    {
        // Before this version the admin End Date was saved exactly as typed (Detroit time) but
        // compared against UTC, so events closed 4-5 hours early. The admin now converts on save;
        // this shifts the values already saved so existing events keep the end time staff intended.
        public override void Up()
        {
            IfDatabase("SqlServer").Execute.Sql($@"
                UPDATE [{nameof(SurveyEvent)}]
                SET [{nameof(SurveyEvent.EndDateUtc)}] =
                    CAST(([{nameof(SurveyEvent.EndDateUtc)}] AT TIME ZONE 'Eastern Standard Time') AT TIME ZONE 'UTC' AS datetime2)
                WHERE [{nameof(SurveyEvent.EndDateUtc)}] IS NOT NULL");
        }

        public override void Down()
        {
            IfDatabase("SqlServer").Execute.Sql($@"
                UPDATE [{nameof(SurveyEvent)}]
                SET [{nameof(SurveyEvent.EndDateUtc)}] =
                    CAST(([{nameof(SurveyEvent.EndDateUtc)}] AT TIME ZONE 'UTC') AT TIME ZONE 'Eastern Standard Time' AS datetime2)
                WHERE [{nameof(SurveyEvent.EndDateUtc)}] IS NOT NULL");
        }
    }
}
