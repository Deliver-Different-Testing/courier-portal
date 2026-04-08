-- =====================================================
-- Feature: Quiz Builder / Player
-- Description: Training quiz creation and completion tracking
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Quizzes')
BEGIN
    CREATE TABLE [dbo].[Quizzes] (
        [Id]          INT IDENTITY(1,1) NOT NULL,
        [Title]       NVARCHAR(300) NOT NULL,
        [Description] NVARCHAR(2000) NULL,
        [Category]    NVARCHAR(100) NULL,
        [PassMark]    INT NOT NULL DEFAULT 80,
        [IsRequired]  BIT NOT NULL DEFAULT 0,
        [IsActive]    BIT NOT NULL DEFAULT 1,
        [Created]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_Quizzes] PRIMARY KEY CLUSTERED ([Id])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuizQuestions')
BEGIN
    CREATE TABLE [dbo].[QuizQuestions] (
        [Id]           INT IDENTITY(1,1) NOT NULL,
        [QuizId]       INT NOT NULL,
        [QuestionText] NVARCHAR(2000) NOT NULL,
        [QuestionType] NVARCHAR(50) NOT NULL DEFAULT 'multi-choice',
        [SortOrder]    INT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_QuizQuestions] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_QuizQuestions_Quiz] FOREIGN KEY ([QuizId]) REFERENCES [dbo].[Quizzes]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_QuizQuestions_QuizId] ON [dbo].[QuizQuestions] ([QuizId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuizOptions')
BEGIN
    CREATE TABLE [dbo].[QuizOptions] (
        [Id]         INT IDENTITY(1,1) NOT NULL,
        [QuestionId] INT NOT NULL,
        [OptionText] NVARCHAR(500) NOT NULL,
        [IsCorrect]  BIT NOT NULL DEFAULT 0,
        [SortOrder]  INT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_QuizOptions] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_QuizOptions_Question] FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[QuizQuestions]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_QuizOptions_QuestionId] ON [dbo].[QuizOptions] ([QuestionId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuizAttempts')
BEGIN
    CREATE TABLE [dbo].[QuizAttempts] (
        [Id]          INT IDENTITY(1,1) NOT NULL,
        [QuizId]      INT NOT NULL,
        [CourierId]   INT NOT NULL,
        [Score]       INT NOT NULL DEFAULT 0,
        [Passed]      BIT NOT NULL DEFAULT 0,
        [StartedAt]   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CompletedAt] DATETIME2 NULL,
        CONSTRAINT [PK_QuizAttempts] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_QuizAttempts_Quiz] FOREIGN KEY ([QuizId]) REFERENCES [dbo].[Quizzes]([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_QuizAttempts_QuizId] ON [dbo].[QuizAttempts] ([QuizId]);
    CREATE INDEX [IX_QuizAttempts_CourierId] ON [dbo].[QuizAttempts] ([CourierId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuizAttemptAnswers')
BEGIN
    CREATE TABLE [dbo].[QuizAttemptAnswers] (
        [Id]               INT IDENTITY(1,1) NOT NULL,
        [AttemptId]        INT NOT NULL,
        [QuestionId]       INT NOT NULL,
        [SelectedOptionId] INT NULL,
        [TextAnswer]       NVARCHAR(2000) NULL,
        CONSTRAINT [PK_QuizAttemptAnswers] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_QuizAttemptAnswers_Attempt] FOREIGN KEY ([AttemptId]) REFERENCES [dbo].[QuizAttempts]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_QuizAttemptAnswers_Question] FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[QuizQuestions]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_QuizAttemptAnswers_Option] FOREIGN KEY ([SelectedOptionId]) REFERENCES [dbo].[QuizOptions]([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_QuizAttemptAnswers_AttemptId] ON [dbo].[QuizAttemptAnswers] ([AttemptId]);
END
GO
