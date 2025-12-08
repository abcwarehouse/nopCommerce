MERGE GenericAttribute AS target
USING (
    select 
        hp.Id as EntityId,
        'Product' as KeyGroup,
        'PLPDescription' as [Key],
        aga.[Value] as [Value],
        0 as StoreId
    from NOPCommerce.dbo.GenericAttribute aga
    join NOPCommerce.dbo.Product ap on ap.Id = aga.EntityId
    join Product hp on ap.Sku = hp.Sku
    where aga.KeyGroup = 'Product'
      and aga.[Key] = 'PLPDescription'
      and aga.EntityId in (select ProductId from NOPCommerce.dbo.AbcMattressModel)
) AS source
ON target.EntityId = source.EntityId
   AND target.KeyGroup = source.KeyGroup
   AND target.[Key] = source.[Key]
   AND target.StoreId = source.StoreId
WHEN MATCHED THEN 
    UPDATE SET 
        target.[Value] = source.[Value],
        target.CreatedOrUpdatedDateUTC = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (EntityId, KeyGroup, [Key], [Value], StoreId, CreatedOrUpdatedDateUTC)
    VALUES (source.EntityId, source.KeyGroup, source.[Key], source.[Value], source.StoreId, GETDATE());
