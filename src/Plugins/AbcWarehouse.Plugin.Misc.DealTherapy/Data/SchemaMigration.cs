using AbcWarehouse.Plugin.Misc.DealTherapy.Domain;
using FluentMigrator;
using Nop.Data.Migrations;

namespace AbcWarehouse.Plugin.Misc.DealTherapy.Data
{
    [NopMigration("2026/05/29 00:00:00:0000001", "Misc.DealTherapy base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            Create.Table(nameof(DealTherapySubmission))
                .WithColumn(nameof(DealTherapySubmission.Id)).AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn(nameof(DealTherapySubmission.CustomerId)).AsInt32().Nullable()
                .WithColumn(nameof(DealTherapySubmission.Email)).AsString(255).Nullable()
                .WithColumn(nameof(DealTherapySubmission.CreatedOnUtc)).AsDateTime().NotNullable();

            Create.Table(nameof(DealTherapyAnswer))
                .WithColumn(nameof(DealTherapyAnswer.Id)).AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn(nameof(DealTherapyAnswer.SubmissionId)).AsInt32().NotNullable()
                    .ForeignKey(nameof(DealTherapySubmission), nameof(DealTherapySubmission.Id))
                .WithColumn(nameof(DealTherapyAnswer.QuestionKey)).AsString(100).NotNullable()
                .WithColumn(nameof(DealTherapyAnswer.AnswerValue)).AsString(500).NotNullable();
        }
    }
}
