-- =====================================================
-- Feature: Compliance Profiles
-- Description: Configurable document requirement templates per courier type
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ComplianceProfiles')
BEGIN
    CREATE TABLE [dbo].[ComplianceProfiles] (
        [Id]          INT IDENTITY(1,1) NOT NULL,
        [Name]        NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [CourierType]  NVARCHAR(100) NOT NULL,
        [IsDefault]   BIT NOT NULL DEFAULT 0,
        [Created]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ComplianceProfiles] PRIMARY KEY CLUSTERED ([Id])
    );

    CREATE INDEX [IX_ComplianceProfiles_CourierType] ON [dbo].[ComplianceProfiles] ([CourierType]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ComplianceProfileRequirements')
BEGIN
    CREATE TABLE [dbo].[ComplianceProfileRequirements] (
        [Id]             INT IDENTITY(1,1) NOT NULL,
        [ProfileId]      INT NOT NULL,
        [DocumentTypeId] INT NOT NULL,
        [IsRequired]     BIT NOT NULL DEFAULT 1,
        [RenewalMonths]  INT NULL,
        [GraceDays]      INT NULL,
        CONSTRAINT [PK_ComplianceProfileRequirements] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_ComplianceProfileReq_Profile] FOREIGN KEY ([ProfileId]) REFERENCES [dbo].[ComplianceProfiles]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ComplianceProfileReq_DocType] FOREIGN KEY ([DocumentTypeId]) REFERENCES [dbo].[DocumentTypes]([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_ComplianceProfileReq_ProfileId] ON [dbo].[ComplianceProfileRequirements] ([ProfileId]);
    CREATE INDEX [IX_ComplianceProfileReq_DocTypeId] ON [dbo].[ComplianceProfileRequirements] ([DocumentTypeId]);
END
GO
