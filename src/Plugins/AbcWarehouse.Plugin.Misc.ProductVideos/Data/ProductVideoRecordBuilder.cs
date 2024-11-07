using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using Nop.Plugin.Misc.AbcCore.Domain;
using Nop.Core.Domain.Media;

namespace Nop.Plugin.Misc.AbcCore.ProductVideo
{
    public class ProductVideoRecordBuilder : NopEntityBuilder<ProductVideo>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
            .WithColumn(nameof(ProductVideo.ProductId)).AsInt32()
                                              .ForeignKey<Product>()
            .WithColumn(nameof(ProductVideo.PictureId)).AsInt32()
                                              .ForeignKey<Picture>()
            .WithColumn(nameof(ProductVideo.VideoUrl)).AsString();
        }
    }
}
