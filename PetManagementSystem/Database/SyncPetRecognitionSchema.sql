USE [PetsManager];
GO

IF OBJECT_ID(N'dbo.pets', N'U') IS NULL
	THROW 51000, 'Table dbo.pets is missing. Restore the base PetsManager schema before applying recognition schema.', 1;
GO

IF OBJECT_ID(N'dbo.staff', N'U') IS NULL
	THROW 51000, 'Table dbo.staff is missing. Restore the base PetsManager schema before applying recognition schema.', 1;
GO

IF OBJECT_ID(N'dbo.pet_recognition_history', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[pet_recognition_history](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[staff_id] [int] NOT NULL,
		[image_url] [nvarchar](500) NOT NULL,
		[selected_pet_id] [int] NULL,
		[detected_species] [nvarchar](50) NULL,
		[detected_breed] [nvarchar](100) NULL,
		[top_similarity] [decimal](5, 4) NULL,
		[execution_time_ms] [int] NULL,
		[created_at] [datetime2](7) NOT NULL,
		CONSTRAINT [PK_pet_recognition_history] PRIMARY KEY CLUSTERED ([id] ASC)
	) ON [PRIMARY];
END;
GO

IF NOT EXISTS (
	SELECT 1
	FROM sys.indexes
	WHERE name = N'IX_pet_recognition_history_staff_id'
	  AND object_id = OBJECT_ID(N'dbo.pet_recognition_history'))
BEGIN
	CREATE INDEX [IX_pet_recognition_history_staff_id]
		ON [dbo].[pet_recognition_history]([staff_id]);
END;
GO

IF NOT EXISTS (
	SELECT 1
	FROM sys.indexes
	WHERE name = N'IX_pet_recognition_history_selected_pet_id'
	  AND object_id = OBJECT_ID(N'dbo.pet_recognition_history'))
BEGIN
	CREATE INDEX [IX_pet_recognition_history_selected_pet_id]
		ON [dbo].[pet_recognition_history]([selected_pet_id]);
END;
GO

IF NOT EXISTS (
	SELECT 1
	FROM sys.foreign_keys
	WHERE name = N'FK_pet_recognition_history_staff'
	  AND parent_object_id = OBJECT_ID(N'dbo.pet_recognition_history'))
BEGIN
	ALTER TABLE [dbo].[pet_recognition_history] WITH CHECK
		ADD CONSTRAINT [FK_pet_recognition_history_staff]
		FOREIGN KEY ([staff_id]) REFERENCES [dbo].[staff]([id]);
	ALTER TABLE [dbo].[pet_recognition_history]
		CHECK CONSTRAINT [FK_pet_recognition_history_staff];
END;
GO

IF NOT EXISTS (
	SELECT 1
	FROM sys.foreign_keys
	WHERE name = N'FK_pet_recognition_history_pet'
	  AND parent_object_id = OBJECT_ID(N'dbo.pet_recognition_history'))
BEGIN
	ALTER TABLE [dbo].[pet_recognition_history] WITH CHECK
		ADD CONSTRAINT [FK_pet_recognition_history_pet]
		FOREIGN KEY ([selected_pet_id]) REFERENCES [dbo].[pets]([id]);
	ALTER TABLE [dbo].[pet_recognition_history]
		CHECK CONSTRAINT [FK_pet_recognition_history_pet];
END;
GO
