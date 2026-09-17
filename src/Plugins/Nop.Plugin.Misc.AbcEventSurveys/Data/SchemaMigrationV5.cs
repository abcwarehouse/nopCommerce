using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AbcEventSurveys.Domain;

namespace Nop.Plugin.Misc.AbcEventSurveys.Data
{
    [NopMigration("2026/09/14 00:00:00:0000000", "Misc.AbcEventSurveys - added SurveyResponse.ConsentSms, made Phone optional")]
    public class SchemaMigrationV5 : Migration
    {
        public override void Up()
        {
            Alter.Table(nameof(SurveyResponse))
                .AddColumn(nameof(SurveyResponse.ConsentSms)).AsBoolean().NotNullable().WithDefaultValue(false);

            // Phone is no longer required on the entry form.
            Alter.Table(nameof(SurveyResponse))
                .AlterColumn(nameof(SurveyResponse.Phone)).AsString(50).Nullable();
        }

        public override void Down()
        {
            Delete.Column(nameof(SurveyResponse.ConsentSms)).FromTable(nameof(SurveyResponse));

            Alter.Table(nameof(SurveyResponse))
                .AlterColumn(nameof(SurveyResponse.Phone)).AsString(50).NotNullable();
        }
    }
}
