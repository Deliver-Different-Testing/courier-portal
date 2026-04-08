-- =====================================================
-- Feature: Document Type Settings
-- Description: Configurable document categories
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DocumentTypes')
BEGIN
    CREATE TABLE [dbo].[DocumentTypes] (
        [Id]            INT IDENTITY(1,1) NOT NULL,
        [Name]          NVARCHAR(200) NOT NULL,
        [Description]   NVARCHAR(1000) NULL,
        [IsRequired]    BIT NOT NULL DEFAULT 0,
        [RenewalMonths] INT NULL,
        [Category]      NVARCHAR(100) NULL,
        [SortOrder]     INT NOT NULL DEFAULT 0,
        [IsActive]      BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_DocumentTypes] PRIMARY KEY CLUSTERED ([Id])
    );

    CREATE INDEX [IX_DocumentTypes_Category] ON [dbo].[DocumentTypes] ([Category]);
    CREATE INDEX [IX_DocumentTypes_IsActive] ON [dbo].[DocumentTypes] ([IsActive]);
END
GO
