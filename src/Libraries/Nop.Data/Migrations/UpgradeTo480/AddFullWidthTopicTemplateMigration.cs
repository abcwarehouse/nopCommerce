using FluentMigrator;
using Nop.Core.Domain.Topics;

namespace Nop.Data.Migrations.UpgradeTo480;

[NopUpdateMigration("2026-09-04 12:00:00", "4.80", UpdateMigrationType.Data)]
public class AddFullWidthTopicTemplateMigration : Migration
{
    private readonly INopDataProvider _dataProvider;

    public AddFullWidthTopicTemplateMigration(INopDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public override void Up()
    {
        if (!_dataProvider.GetTable<TopicTemplate>().Any(tt => tt.ViewPath == "TopicDetailsFullWidth"))
        {
            _dataProvider.InsertEntity(new TopicTemplate
            {
                Name = "Full width template",
                ViewPath = "TopicDetailsFullWidth",
                DisplayOrder = 2
            });
        }
    }

    public override void Down()
    {
        throw new NotImplementedException();
    }
}
