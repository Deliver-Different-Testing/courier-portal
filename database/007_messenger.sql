-- =====================================================
-- Feature: Courier Messenger
-- Description: REST-based messaging between admin and couriers
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Conversations')
BEGIN
    CREATE TABLE [dbo].[Conversations] (
        [Id]        INT IDENTITY(1,1) NOT NULL,
        [CourierId] INT NOT NULL,
        [Subject]   NVARCHAR(300) NULL,
        [Status]    NVARCHAR(50) NOT NULL DEFAULT 'open',
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_Conversations] PRIMARY KEY CLUSTERED ([Id])
    );

    CREATE INDEX [IX_Conversations_CourierId] ON [dbo].[Conversations] ([CourierId]);
    CREATE INDEX [IX_Conversations_Status] ON [dbo].[Conversations] ([Status]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConversationMessages')
BEGIN
    CREATE TABLE [dbo].[ConversationMessages] (
        [Id]             INT IDENTITY(1,1) NOT NULL,
        [ConversationId] INT NOT NULL,
        [SenderType]     NVARCHAR(50) NOT NULL,
        [SenderName]     NVARCHAR(200) NOT NULL,
        [MessageText]    NVARCHAR(MAX) NOT NULL,
        [SentAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ReadAt]         DATETIME2 NULL,
        CONSTRAINT [PK_ConversationMessages] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_ConversationMessages_Conversation] FOREIGN KEY ([ConversationId]) REFERENCES [dbo].[Conversations]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_ConversationMessages_ConversationId] ON [dbo].[ConversationMessages] ([ConversationId]);
    CREATE INDEX [IX_ConversationMessages_SentAt] ON [dbo].[ConversationMessages] ([SentAt]);
END
GO
