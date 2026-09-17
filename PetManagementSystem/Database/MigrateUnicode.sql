USE [QuanLyThuCung];
GO

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

DECLARE @sql NVARCHAR(MAX) = N'';

-- Gỡ các DEFAULT constraint đang phụ thuộc vào các cột sẽ đổi kiểu.
SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(dc.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(dc.parent_object_id))
	+ N' DROP CONSTRAINT ' + QUOTENAME(dc.name) + N';' + CHAR(13) + CHAR(10)
FROM sys.default_constraints dc
INNER JOIN sys.columns c
	ON c.object_id = dc.parent_object_id
   AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id IN (
		OBJECT_ID(N'dbo.permissions'),
		OBJECT_ID(N'dbo.roles'),
		OBJECT_ID(N'dbo.users'),
		OBJECT_ID(N'dbo.staff'),
		OBJECT_ID(N'dbo.veterinarians')
	)
  AND c.name IN (N'code', N'name', N'description', N'module', N'username', N'password_hash', N'full_name', N'email', N'phone_number', N'address', N'position', N'specialty', N'license_no', N'status');

SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(kc.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(kc.parent_object_id))
	+ N' DROP CONSTRAINT ' + QUOTENAME(kc.name) + N';' + CHAR(13) + CHAR(10)
FROM sys.key_constraints kc
WHERE kc.[type] = 'UQ'
  AND kc.parent_object_id IN (
	  OBJECT_ID(N'dbo.permissions'),
	  OBJECT_ID(N'dbo.roles'),
	  OBJECT_ID(N'dbo.users'),
	  OBJECT_ID(N'dbo.veterinarians')
  );

IF @sql <> N''
	EXEC sys.sp_executesql @sql;

ALTER TABLE dbo.permissions ALTER COLUMN [code] NVARCHAR(60) NOT NULL;
ALTER TABLE dbo.permissions ALTER COLUMN [name] NVARCHAR(40) NOT NULL;
ALTER TABLE dbo.permissions ALTER COLUMN [description] NVARCHAR(255) NULL;
ALTER TABLE dbo.permissions ALTER COLUMN [module] NVARCHAR(40) NOT NULL;

ALTER TABLE dbo.roles ALTER COLUMN [name] NVARCHAR(40) NOT NULL;
ALTER TABLE dbo.roles ALTER COLUMN [description] NVARCHAR(255) NULL;
ALTER TABLE dbo.roles ALTER COLUMN [status] NVARCHAR(20) NOT NULL;

ALTER TABLE dbo.users ALTER COLUMN [username] NVARCHAR(40) NOT NULL;
ALTER TABLE dbo.users ALTER COLUMN [password_hash] NVARCHAR(255) NOT NULL;
ALTER TABLE dbo.users ALTER COLUMN [full_name] NVARCHAR(40) NULL;
ALTER TABLE dbo.users ALTER COLUMN [email] NVARCHAR(100) NOT NULL;
ALTER TABLE dbo.users ALTER COLUMN [phone_number] NVARCHAR(40) NULL;
ALTER TABLE dbo.users ALTER COLUMN [address] NVARCHAR(255) NULL;
ALTER TABLE dbo.users ALTER COLUMN [status] NVARCHAR(20) NOT NULL;

ALTER TABLE dbo.staff ALTER COLUMN [position] NVARCHAR(60) NULL;
ALTER TABLE dbo.staff ALTER COLUMN [status] NVARCHAR(20) NOT NULL;

ALTER TABLE dbo.veterinarians ALTER COLUMN [specialty] NVARCHAR(100) NULL;
ALTER TABLE dbo.veterinarians ALTER COLUMN [license_no] NVARCHAR(50) NULL;
ALTER TABLE dbo.veterinarians ALTER COLUMN [status] NVARCHAR(20) NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_permissions_code')
	ALTER TABLE dbo.permissions ADD CONSTRAINT UQ_permissions_code UNIQUE ([code]);
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_permissions_name')
	ALTER TABLE dbo.permissions ADD CONSTRAINT UQ_permissions_name UNIQUE ([name]);
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_roles_name')
	ALTER TABLE dbo.roles ADD CONSTRAINT UQ_roles_name UNIQUE ([name]);
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_users_username')
	ALTER TABLE dbo.users ADD CONSTRAINT UQ_users_username UNIQUE ([username]);
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_users_email')
	ALTER TABLE dbo.users ADD CONSTRAINT UQ_users_email UNIQUE ([email]);
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_veterinarians_license_no')
	ALTER TABLE dbo.veterinarians ADD CONSTRAINT UQ_veterinarians_license_no UNIQUE ([license_no]);

-- Tạo lại DEFAULT cho các cột trạng thái sau khi đổi kiểu.
IF OBJECT_ID(N'dbo.roles') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_roles_status')
	ALTER TABLE dbo.roles ADD CONSTRAINT DF_roles_status DEFAULT (N'active') FOR [status];
IF OBJECT_ID(N'dbo.users') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_users_status')
	ALTER TABLE dbo.users ADD CONSTRAINT DF_users_status DEFAULT (N'active') FOR [status];
IF OBJECT_ID(N'dbo.staff') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_staff_status')
	ALTER TABLE dbo.staff ADD CONSTRAINT DF_staff_status DEFAULT (N'active') FOR [status];
IF OBJECT_ID(N'dbo.veterinarians') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_veterinarians_status')
	ALTER TABLE dbo.veterinarians ADD CONSTRAINT DF_veterinarians_status DEFAULT (N'active') FOR [status];

COMMIT TRANSACTION;
GO

SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (N'permissions', N'roles', N'users', N'staff', N'veterinarians')
  AND DATA_TYPE IN (N'varchar', N'nvarchar')
ORDER BY TABLE_NAME, ORDINAL_POSITION;
GO
