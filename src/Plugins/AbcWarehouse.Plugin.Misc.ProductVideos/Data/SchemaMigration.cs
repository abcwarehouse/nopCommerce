using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.AbcCore.ProductVideo
{
    [SkipMigrationOnUpdate]
    [NopMigration("2024/11/07 11:34:55:1687541", "Create ProductVideo table")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            _migrationManager.BuildTable<ProductVideo>(Create);
        }
    }
}
