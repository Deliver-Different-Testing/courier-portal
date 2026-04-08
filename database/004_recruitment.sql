-- =====================================================
-- Feature: Recruitment Advertising
-- Description: Job posting management
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'JobPostings')
BEGIN
    CREATE TABLE [dbo].[JobPostings] (
        [Id]          INT IDENTITY(1,1) NOT NULL,
        [Title]       NVARCHAR(300) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [Location]    NVARCHAR(200) NULL,
        [VehicleType] NVARCHAR(100) NULL,
        [PayRate]     DECIMAL(10,2) NULL,
        [Status]      NVARCHAR(50) NOT NULL DEFAULT 'draft',
        [PostedAt]    DATETIME2 NULL,
        [ClosesAt]    DATETIME2 NULL,
        CONSTRAINT [PK_JobPostings] PRIMARY KEY CLUSTERED ([Id])
    );

    CREATE INDEX [IX_JobPostings_Status] ON [dbo].[JobPostings] ([Status]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'JobApplications')
BEGIN
    CREATE TABLE [dbo].[JobApplications] (
        [Id]          INT IDENTITY(1,1) NOT NULL,
        [PostingId]   INT NOT NULL,
        [ApplicantId] INT NOT NULL,
        [Status]      NVARCHAR(50) NOT NULL DEFAULT 'applied',
        [AppliedAt]   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_JobApplications] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_JobApplications_Posting] FOREIGN KEY ([PostingId]) REFERENCES [dbo].[JobPostings]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_JobApplications_PostingId] ON [dbo].[JobApplications] ([PostingId]);
    CREATE INDEX [IX_JobApplications_ApplicantId] ON [dbo].[JobApplications] ([ApplicantId]);
END
GO
