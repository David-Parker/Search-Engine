select * from sys.dm_tran_locks;

select * from keywords;
select * from page_rank;

select count(*) from keywords;
select count(*) from page_rank;

truncate table keywords;
truncate table page_rank;



SELECT SUM(p.rows) FROM sys.partitions AS p
  INNER JOIN sys.tables AS t
  ON p.[object_id] = t.[object_id]
  INNER JOIN sys.schemas AS s
  ON s.[schema_id] = t.[schema_id]
  WHERE t.name = N'Keywords'
  AND s.name = N'dbo'
  AND p.index_id IN (0,1);

  select * from(select top 20 * from Keywords
  where keyword = 'apple'
  order by k_count) k
  join Page_Rank p on k.url = p.url
  order by p.p_rank desc;


  select top 50 * from keywords
  order by k_count desc;

insert into keywords values ('5', '5',3,null, '5');


update keywords set k_count = k_count + 5
where keyword = '5' AND url = '5';

select * from keywords where keyword = '5' AND url = '5';
