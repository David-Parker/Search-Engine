select * from sys.dm_tran_locks;

-- Stored procedure same as SUM below
EXECUTE Keyword_count;
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

-- Two keyword search
select * from(select top 50 k1.url as k1url,k1.keyword as key1,k2.keyword as key2 from Keywords k1
  join Keywords k2 on k1.url = k2.url AND k1.keyword = 'donald'
  where k2.keyword = 'trump'
  order by k1.k_count) k
  join Page_Rank p on k.k1url = p.url
  order by p.p_rank desc;

-- One keyword search
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
