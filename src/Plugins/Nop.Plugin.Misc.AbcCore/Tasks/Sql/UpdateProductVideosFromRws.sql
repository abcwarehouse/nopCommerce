--1. Add RWS videos to NOPCommerce
MERGE Video AS t
USING (
	SELECT DISTINCT video_file_name, MIN(thumbnail_image_file_name) as ThumbnailImageFilename
	FROM [StagingDb].dbo.rws_product_video pv
	WHERE video_type = 'youtube' and thumbnail_image_file_name IS NOT NULL
	GROUP BY video_file_name
) AS s
ON s.video_file_name = t.VideoUrl COLLATE DATABASE_DEFAULT
	AND CONCAT('https://images.webfronts.com/cache/', s.ThumbnailImageFilename) = t.ThumbnailUrl COLLATE DATABASE_DEFAULT
WHEN NOT MATCHED BY TARGET THEN
    INSERT (VideoUrl, ThumbnailUrl)
    VALUES (s.video_file_name, CONCAT('https://images.webfronts.com/cache/', s.ThumbnailImageFilename))
WHEN NOT MATCHED BY SOURCE THEN
    DELETE;

--2. Map videos to products
MERGE ProductVideo AS t
USING (
	SELECT
		v.Id as VideoId,
		p.Id as ProductId
	FROM [StagingDb].dbo.rws_product_video pv  
	JOIN Video v on v.VideoUrl = pv.video_file_name COLLATE DATABASE_DEFAULT
	JOIN [StagingDb].dbo.rws_product rp on rp.id = pv.rws_product_id
	JOIN Product p on p.Sku = rp.manufacturer_pn and p.Deleted = 0
	WHERE video_type = 'youtube'
) AS s
ON s.VideoId = t.VideoId and s.ProductId = t.ProductId
WHEN NOT MATCHED BY TARGET THEN
    INSERT (VideoId, ProductId, DisplayOrder)
    VALUES (s.VideoId, s.ProductId, 0)
WHEN NOT MATCHED BY SOURCE THEN
    DELETE;