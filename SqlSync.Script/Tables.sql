/********************************************************************************
Script name	: SqlSync.Schema
Description	: Actual DB tablese for SqlSync solution
********************************************************************************/

PRINT '-> dbo.SystemOption'
GO
IF OBJECT_ID('SystemOption') IS NULL
BEGIN
	CREATE TABLE dbo.[SystemOption]
	(
		OptionName	NVARCHAR(256) NOT NULL,
		OptionValue	NVARCHAR(MAX) NOT NULL,
		CONSTRAINT PK_SystemOption PRIMARY KEY CLUSTERED 
		(
			OptionName
		)
	)
END
GO

--Metadata

PRINT '-> dbo.ListMapping'
GO
IF OBJECT_ID('ListMapping') IS NULL
BEGIN
	CREATE TABLE dbo.ListMapping
	(
		[Id] 		UNIQUEIDENTIFIER NOT NULL 
				ROWGUIDCOL CONSTRAINT [DF_ListMapping_Id] DEFAULT(NEWID()),
		[ListName] 	NVARCHAR(256) NOT NULL,
		[TableName] 	NVARCHAR(256) NOT NULL,
		[Key]		NVARCHAR(255) NULL,
		CONSTRAINT [PK_ListMapping] PRIMARY KEY CLUSTERED 
		(
			[Id]
		)
	)
END
GO

PRINT '-> dbo.ListMappingField'
GO

IF OBJECT_ID('ListMappingField') IS NULL
BEGIN
	CREATE TABLE dbo.ListMappingField
	(
		[Id] 		UNIQUEIDENTIFIER NOT NULL 
					ROWGUIDCOL CONSTRAINT [DF_ListMappingField_Id] DEFAULT(NEWID()),
		[ListMappingId]         UNIQUEIDENTIFIER NOT NULL,
		[ItemName] 		NVARCHAR(256) NOT NULL,
		[FieldName] 		NVARCHAR(256) NOT NULL,
		CONSTRAINT [PK_ListMappingField] PRIMARY KEY CLUSTERED 
		(
			[Id]
		)
	)
END
GO
