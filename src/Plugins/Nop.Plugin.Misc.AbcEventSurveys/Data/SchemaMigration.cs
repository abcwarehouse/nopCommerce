using System.Data;
using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AbcEventSurveys.Domain;

namespace Nop.Plugin.Misc.AbcEventSurveys.Data
{
    [NopMigration("2026/07/27 00:00:00:0000000", "Misc.AbcEventSurveys base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            BuildSurveyEvent();
            BuildSurveyCustomField();
            BuildSurveyResponse();
            BuildSurveyResponseCustomValue();
        }

        private void BuildSurveyEvent()
        {
            Create.Table(nameof(SurveyEvent))
                .WithColumn(nameof(SurveyEvent.Id)).AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn(nameof(SurveyEvent.Name)).AsString(400).NotNullable()
                .WithColumn(nameof(SurveyEvent.Code)).AsString(200).NotNullable().Unique()
                .WithColumn(nameof(SurveyEvent.Header1)).AsString(400).Nullable()
                .WithColumn(nameof(SurveyEvent.Header2)).AsString(400).Nullable()
                .WithColumn(nameof(SurveyEvent.Description)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(SurveyEvent.IsActive)).AsBoolean().NotNullable()
                .WithColumn(nameof(SurveyEvent.StartDateUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof(SurveyEvent.EndDateUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof(SurveyEvent.CreatedOnUtc)).AsDateTime2().NotNullable();
        }

        private void BuildSurveyCustomField()
        {
            Create.Table(nameof(SurveyCustomField))
                .WithColumn(nameof(SurveyCustomField.Id)).AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn(nameof(SurveyCustomField.SurveyEventId))
                    .AsInt32()
                    .ForeignKey(nameof(SurveyEvent), nameof(SurveyEvent.Id))
                    .OnDelete(Rule.Cascade)
                .WithColumn(nameof(SurveyCustomField.Name)).AsString(400).NotNullable()
                .WithColumn(nameof(SurveyCustomField.IsRequired)).AsBoolean().NotNullable()
                .WithColumn(nameof(SurveyCustomField.DisplayOrder)).AsInt32().NotNullable();
        }

        private void BuildSurveyResponse()
        {
            Create.Table(nameof(SurveyResponse))
                .WithColumn(nameof(SurveyResponse.Id)).AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn(nameof(SurveyResponse.SurveyEventId))
                    .AsInt32()
                    .ForeignKey(nameof(SurveyEvent), nameof(SurveyEvent.Id))
                    .OnDelete(Rule.Cascade)
                .WithColumn(nameof(SurveyResponse.FirstName)).AsString(400).NotNullable()
                .WithColumn(nameof(SurveyResponse.LastName)).AsString(400).NotNullable()
                .WithColumn(nameof(SurveyResponse.Email)).AsString(400).NotNullable()
                .WithColumn(nameof(SurveyResponse.Phone)).AsString(50).NotNullable()
                .WithColumn(nameof(SurveyResponse.ConsentMarketing)).AsBoolean().NotNullable()
                .WithColumn(nameof(SurveyResponse.CreatedOnUtc)).AsDateTime2().NotNullable()
                .WithColumn(nameof(SurveyResponse.IpAddress)).AsString(100).Nullable();
        }

        private void BuildSurveyResponseCustomValue()
        {
            Create.Table(nameof(SurveyResponseCustomValue))
                .WithColumn(nameof(SurveyResponseCustomValue.Id)).AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn(nameof(SurveyResponseCustomValue.SurveyResponseId))
                    .AsInt32()
                    .ForeignKey(nameof(SurveyResponse), nameof(SurveyResponse.Id))
                    .OnDelete(Rule.Cascade)
                .WithColumn(nameof(SurveyResponseCustomValue.SurveyCustomFieldId))
                    .AsInt32()
                    // NoAction (not Cascade) here: SurveyEvent already cascades to both SurveyResponse and
                    // SurveyCustomField, and both of those cascade toward this table - a second cascade path
                    // to the same descendant table is rejected by SQL Server ("multiple cascade paths").
                    // The SurveyResponseId cascade above is sufficient to clean these rows up when an event
                    // (and its responses) is deleted; DeleteCustomFieldAsync removes orphaned values explicitly
                    // when a single custom field is removed from a still-active event.
                    .ForeignKey(nameof(SurveyCustomField), nameof(SurveyCustomField.Id))
                    .OnDelete(Rule.None)
                .WithColumn(nameof(SurveyResponseCustomValue.Value)).AsString(int.MaxValue).Nullable();
        }
    }
}
