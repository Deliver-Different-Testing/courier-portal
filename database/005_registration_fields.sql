-- =====================================================
-- Feature: Registration Settings
-- Description: Configurable applicant portal fields
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RegistrationFields')
BEGIN
    CREATE TABLE [dbo].[RegistrationFields] (
        [Id]         INT IDENTITY(1,1) NOT NULL,
        [FieldName]  NVARCHAR(200) NOT NULL,
        [FieldType]  NVARCHAR(50) NOT NULL DEFAULT 'text',
        [IsRequired] BIT NOT NULL DEFAULT 0,
        [IsVisible]  BIT NOT NULL DEFAULT 1,
        [SortOrder]  INT NOT NULL DEFAULT 0,
        [Section]    NVARCHAR(100) NULL,
        [HelpText]   NVARCHAR(500) NULL,
        CONSTRAINT [PK_RegistrationFields] PRIMARY KEY CLUSTERED ([Id])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RegistrationFieldOptions')
BEGIN
    CREATE TABLE [dbo].[RegistrationFieldOptions] (
        [Id]         INT IDENTITY(1,1) NOT NULL,
        [FieldId]    INT NOT NULL,
        [OptionText] NVARCHAR(300) NOT NULL,
        [SortOrder]  INT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_RegistrationFieldOptions] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_RegistrationFieldOptions_Field] FOREIGN KEY ([FieldId]) REFERENCES [dbo].[RegistrationFields]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_RegistrationFieldOptions_FieldId] ON [dbo].[RegistrationFieldOptions] ([FieldId]);
END
GO
