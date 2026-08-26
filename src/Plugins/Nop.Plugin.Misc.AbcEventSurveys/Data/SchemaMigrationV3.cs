using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AbcEventSurveys.Domain;

namespace Nop.Plugin.Misc.AbcEventSurveys.Data
{
    [NopMigration("2026/07/28 00:00:00:0000000", "Misc.AbcEventSurveys - added custom terms/thank-you/redirect fields")]
    public class SchemaMigrationV3 : AutoReversingMigration
    {
        public override void Up()
        {
            Alter.Table(nameof(SurveyEvent))
                .AddColumn(nameof(SurveyEvent.TermsAndConditions)).AsString(int.MaxValue).Nullable()
                .AddColumn(nameof(SurveyEvent.ThankYouHeader)).AsString(400).Nullable()
                .AddColumn(nameof(SurveyEvent.ThankYouDescription)).AsString(int.MaxValue).Nullable()
                // RedirectUrl was removed from SurveyEvent in SchemaMigrationV4 - kept as a string
                // literal here (rather than nameof) since the property no longer exists to reference.
                .AddColumn("RedirectUrl").AsString(400).Nullable();
        }
    }
}
