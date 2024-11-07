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

        public UpdatePdpVideosTask(
            CoreSettings settings,
            ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            await _logger.InformationAsync("Updating PDP videos.");

            using (SqlConnection connection = new SqlConnection(_settings.StagingDbConnectionString))
            {
                var queryString = @"
                    SELECT
                        rp.item_key,
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
                        Console.WriteLine(String.Format("{0}, {1}, {2}",
                            reader["item_key"],
                            reader["thumbnail_image_file_name"],
                            reader["video_file_name"]));
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
