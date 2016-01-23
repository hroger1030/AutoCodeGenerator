DECLARE		@TableName VARCHAR(50)
SET			@TableName = 'tb_foo' 

DECLARE @VariableTypes TABLE 
(
	[ColumnName] VARCHAR(255),
	[DataType] VARCHAR(255),
	[Length] INT,
	[Precision] INT,
	[Scale]INT,
	[isNullable] BIT,
	[isPK] BIT DEFAULT '0',
	[isIdentity] BIT DEFAULT '0'
)


INSERT @VariableTypes
(
	[ColumnName],
	[DataType],
	[Length],
	[Precision],
	[Scale],
	[isNullable]
)

SELECT	sc.[Name]			AS [ColumnName],
		st.[name]			AS [DataType],
		sc.[length]			AS [Length],
		sc.[xprec]			AS [Precision],
		sc.[xScale]			AS [Scale],
		sc.[isnullable]		AS [IsNullable]
FROM	sysobjects so
		INNER JOIN syscolumns sc ON so.[ID] = sc.[ID]
		INNER JOIN systypes st ON sc.[xtype] = st.[xtype]
WHERE	so.[name] = @TableName
ORDER	BY sc.colorder

-- Flag any Identities
UPDATE	@VariableTypes
SET		[isIdentity] = 1
WHERE	[ColumnName] IN
(
	SELECT	c.[name]
	FROM	syscolumns c 
			INNER JOIN sysobjects o ON o.[id] = c.[id]
	AND c.[autoval] IS NOT NULL
	AND o.[name] = @TableName
)


-- Flag any PKs in table
UPDATE @VariableTypes
SET [isPK] = 1
WHERE [ColumnName] IN
(
	SELECT	ColumnName = convert(SYSNAME,c.[name])
	FROM	sysindexes i, 
			syscolumns c,
			sysobjects o
	WHERE	o.[id] = object_id(quotename(@TableName))
	AND		o.[id] = c.[id]
	AND		o.[id] = i.[id]
	AND		(i.[status] & 0x800) = 0x800
	AND
	(
		c.[name] = index_col (quotename(@TableName), i.indid,  1) OR
		c.[name] = index_col (quotename(@TableName), i.indid,  2) OR
		c.[name] = index_col (quotename(@TableName), i.indid,  3) OR
		c.[name] = index_col (quotename(@TableName), i.indid,  4) OR
		c.[name] = index_col (quotename(@TableName), i.indid,  5) OR
		c.[name] = index_col (quotename(@TableName), i.indid,  6) OR
		c.[name] = index_col (quotename(@TableName), i.indid,  7) OR
		c.[name] = index_col (quotename(@TableName), i.indid,  8) OR
		c.[name] = index_col (quotename(@TableName), i.indid,  9) OR
		c.[name] = index_col (quotename(@TableName), i.indid, 10) OR
		c.[name] = index_col (quotename(@TableName), i.indid, 11) OR
		c.[name] = index_col (quotename(@TableName), i.indid, 12) OR
		c.[name] = index_col (quotename(@TableName), i.indid, 13) OR
		c.[name] = index_col (quotename(@TableName), i.indid, 14) OR
		c.[name] = index_col (quotename(@TableName), i.indid, 15) OR
		c.[name] = index_col (quotename(@TableName), i.indid, 16)
	)
)


-- Return Data
SELECT	*
FROM	@VariableTypes	 