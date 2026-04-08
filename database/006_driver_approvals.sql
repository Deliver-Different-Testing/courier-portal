-- =====================================================
-- Feature: Driver Approval
-- Description: Multi-tenant courier approval workflow
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DriverApprovals')
BEGIN
    CREATE TABLE [dbo].[DriverApprovals] (
        [Id]          INT IDENTITY(1,1) NOT NULL,
        [CourierId]   INT NOT NULL,
        [TenantId]    INT NOT NULL,
        [Status]      NVARCHAR(50) NOT NULL DEFAULT 'pending',
        [RequestedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ReviewedAt]  DATETIME2 NULL,
        [ReviewedBy]  NVARCHAR(200) NULL,
        [Notes]       NVARCHAR(2000) NULL,
        CONSTRAINT [PK_DriverApprovals] PRIMARY KEY CLUSTERED ([Id])
    );

    CREATE INDEX [IX_DriverApprovals_CourierId] ON [dbo].[DriverApprovals] ([CourierId]);
    CREATE INDEX [IX_DriverApprovals_TenantId] ON [dbo].[DriverApprovals] ([TenantId]);
    CREATE INDEX [IX_DriverApprovals_Status] ON [dbo].[DriverApprovals] ([Status]);
END
GO
