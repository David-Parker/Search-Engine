-- ==========================================================
-- Create Stored Procedure Template for Windows Azure SQL Database
-- ==========================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE Keyword_count 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	  SELECT SUM(p.rows) FROM sys.partitions AS p
	  INNER JOIN sys.tables AS t
	  ON p.[object_id] = t.[object_id]
	  INNER JOIN sys.schemas AS s
	  ON s.[schema_id] = t.[schema_id]
	  WHERE t.name = N'Keywords'
	  AND s.name = N'dbo'
	  AND p.index_id IN (0,1);
END
GO
