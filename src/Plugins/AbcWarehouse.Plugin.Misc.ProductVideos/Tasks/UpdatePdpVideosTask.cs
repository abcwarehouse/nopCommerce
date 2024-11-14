using System;
using Nop.Services.Catalog;
using Nop.Services.Tasks;
using Nop.Data;
using Nop.Services.Logging;
using Microsoft.Data.SqlClient;
using Nop.Plugin.Misc.AbcCore;

namespace AbcWarehouse.Plugin.Misc.ProductVideos.Tasks
{
    class UpdatePdpVideosTask : IScheduleTask
    {
        private readonly CoreSettings _settings;
        private readonly ILogger _logger;
        private readonly IProductService _productService;

        public UpdatePdpVideosTask(
            CoreSettings settings,
            ILogger logger,
            IProductService productService)
        {
            _settings = settings;
            _logger = logger;
            _productService = productService;
        }

        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            await _logger.InformationAsync("Updating PDP videos.");

            using (SqlConnection connection = new SqlConnection(_settings.StagingDbConnectionString))
            {
                var queryString = @"
                    SELECT
                        SUBSTRING(rp.item_key, CHARINDEX(':', rp.item_key) + 1, LEN(rp.item_key)) as item_key,
                        CONCAT('rws_', rpv.thumbnail_image_file_name) as thumbnail_image_file_name,
                        REPLACE(rpv.video_file_name, 'embed/', 'watch?v=') AS video_file_name
                    FROM
                        rws_product_video rpv
                    JOIN
                        rws_product rp on rp.id = rpv.rws_product_id
                    WHERE
                        video_type = 'youtube'
                        -- exclude non-thumbnail videos
                        AND thumbnail_image_file_name is not null
                    ";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        var sku = reader["item_key"].ToString();
                        var product = _productService.GetProductBySkuAsync(sku);
                        if (product == null) { continue; }

                        var video_file_name = reader["video_file_name"].ToString();
                        // If more than one question mark found, need to clean the URL for MagnificiPopup
                        var cleanedYouTubeUrl = video_file_name.IndexOf('?') != video_file_name.LastIndexOf('?') ?
                            video_file_name.Substring(0, video_file_name.LastIndexOf('?')) :
                            video_file_name;

                        Console.WriteLine(String.Format("{0}, {1}, {2}",
                            reader["item_key"],
                            reader["thumbnail_image_file_name"],
                            cleanedYouTubeUrl));
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }
            }

            await _logger.InformationAsync("PDP videos updated.");
        }
    }
}
