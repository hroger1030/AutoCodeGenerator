--Table & Columns
SELECT	sys.Objects.[Name]				AS [TableName],
		sys.columns.[Name]				AS [ColumnName],
		sys.types.[name]				AS [DataType],
		sys.columns.[max_length]		AS [Length],
		sys.columns.[precision]			AS [Precision],
		sys.columns.[scale]				AS [Scale],		
		sys.columns.[is_nullable]		AS [IsNullable],
		ISNULL(PrimaryKeys.IsPK,0)		AS [IsPK],
		sys.columns.[is_identity]		AS [IsIdentity],
		sys.columns.column_id			AS [ColumnOrdinal]
		
FROM	sys.objects 
		INNER JOIN sys.columns ON sys.objects.object_id = sys.columns.object_id
		INNER JOIN sys.types ON sys.columns.system_type_id = sys.types.system_type_id
		LEFT JOIN
		( 
			SELECT 	DISTINCT C.[TABLE_NAME]		AS [TableName],
					K.[COLUMN_NAME]				AS [ColumnName],
					1							AS [IsPK]					
			
			FROM 	INFORMATION_SCHEMA.KEY_COLUMN_USAGE K
					INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS C ON K.TABLE_NAME = C.TABLE_NAME
			WHERE	C.CONSTRAINT_TYPE = 'PRIMARY KEY'
		) PrimaryKeys ON PrimaryKeys.[TableName] = sys.Objects.[Name] AND PrimaryKeys.[ColumnName] = sys.columns.[Name]
		
WHERE	sys.objects.type = 'U'
AND		sys.types.[name] <> 'sysname'

ORDER	BY sys.Objects.name,sys.columns.column_id



-- SP & Functions
SELECT	sys.objects.name	AS [Name],
		syscomments.text	AS [Body] 
FROM	sys.objects
		INNER JOIN syscomments ON sys.objects.object_id = syscomments.id
WHERE	sys.objects.type = 'p'
AND		sys.objects.is_ms_shipped = 0
ORDER	BY sys.objects.name, syscomments.colid



-- Constraints
SELECT	C.CONSTRAINT_NAME	AS [ConstraintName],
		FK.TABLE_NAME		AS FKTable,
		CU.COLUMN_NAME		AS FKColumn,
		PK.TABLE_NAME		AS PKTable,
		PT.COLUMN_NAME		AS PKColumn

FROM	INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
		INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
		INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
		INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
		INNER JOIN 
		(
			SELECT	i1.TABLE_NAME, 
					i2.COLUMN_NAME
			FROM	INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
					INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
			WHERE	i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
		) PT ON PT.TABLE_NAME = PK.TABLE_NAME

ORDER BY
C.CONSTRAINT_NAME