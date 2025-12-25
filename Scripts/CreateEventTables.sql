-- Create Event Management Tables
-- Run this script to create all event-related tables

-- 1. Events Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Events]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Events] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Title] NVARCHAR(200) NOT NULL,
        [Slug] NVARCHAR(250) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [ShortDescription] NVARCHAR(500) NULL,
        [EventDate] DATETIME2 NOT NULL,
        [EventEndDate] DATETIME2 NULL,
        [EventTime] NVARCHAR(50) NULL,
        [City] NVARCHAR(100) NULL,
        [Venue] NVARCHAR(200) NULL,
        [Address] NVARCHAR(500) NULL,
        [MapUrl] NVARCHAR(1000) NULL,
        [Price] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Currency] NVARCHAR(10) NULL DEFAULT 'VND',
        [FeaturedImageUrl] NVARCHAR(1000) NULL,
        [GalleryImages] NVARCHAR(MAX) NULL,
        [MaxCapacity] INT NULL,
        [RegisteredCount] INT NOT NULL DEFAULT 0,
        [IsRegistrationOpen] BIT NOT NULL DEFAULT 1,
        [RegistrationDeadline] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsFeatured] BIT NOT NULL DEFAULT 0,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
        [MetaTitle] NVARCHAR(200) NULL,
        [MetaDescription] NVARCHAR(500) NULL,
        [Tags] NVARCHAR(500) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_Events_Slug ON [dbo].[Events]([Slug]);
    CREATE INDEX IX_Events_EventDate ON [dbo].[Events]([EventDate]);
    CREATE INDEX IX_Events_City ON [dbo].[Events]([City]);
    CREATE INDEX IX_Events_Status ON [dbo].[Events]([Status]);
    CREATE INDEX IX_Events_IsActive_IsFeatured ON [dbo].[Events]([IsActive], [IsFeatured]);

    PRINT 'Events table created successfully';
END
ELSE
BEGIN
    PRINT 'Events table already exists';
END
GO

-- 2. EventRegistrations Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EventRegistrations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EventRegistrations] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [EventId] UNIQUEIDENTIFIER NOT NULL,
        [UserId] UNIQUEIDENTIFIER NULL,
        [FullName] NVARCHAR(200) NOT NULL,
        [Email] NVARCHAR(200) NOT NULL,
        [PhoneNumber] NVARCHAR(20) NULL,
        [NumberOfAttendees] INT NOT NULL DEFAULT 1,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Confirmed',
        [RegistrationDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CancellationDate] DATETIME2 NULL,
        [CancellationReason] NVARCHAR(500) NULL,
        [AmountPaid] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [PaymentStatus] NVARCHAR(50) NULL DEFAULT 'Pending',
        [PaymentMethod] NVARCHAR(50) NULL,
        [TransactionId] NVARCHAR(200) NULL,
        [Notes] NVARCHAR(1000) NULL,
        [SpecialRequirements] NVARCHAR(1000) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_EventRegistrations_Events FOREIGN KEY ([EventId]) REFERENCES [dbo].[Events]([Id]) ON DELETE CASCADE,
        CONSTRAINT FK_EventRegistrations_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE SET NULL
    );

    CREATE INDEX IX_EventRegistrations_EventId ON [dbo].[EventRegistrations]([EventId]);
    CREATE INDEX IX_EventRegistrations_UserId ON [dbo].[EventRegistrations]([UserId]);
    CREATE INDEX IX_EventRegistrations_Email ON [dbo].[EventRegistrations]([Email]);
    CREATE INDEX IX_EventRegistrations_Status ON [dbo].[EventRegistrations]([Status]);

    PRINT 'EventRegistrations table created successfully';
END
ELSE
BEGIN
    PRINT 'EventRegistrations table already exists';
END
GO

-- 3. EventReviews Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EventReviews]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EventReviews] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [EventId] UNIQUEIDENTIFIER NOT NULL,
        [UserId] UNIQUEIDENTIFIER NULL,
        [ReviewerName] NVARCHAR(200) NOT NULL,
        [ReviewerEmail] NVARCHAR(200) NULL,
        [Rating] INT NOT NULL CHECK ([Rating] >= 1 AND [Rating] <= 5),
        [Comment] NVARCHAR(2000) NOT NULL,
        [IsApproved] BIT NOT NULL DEFAULT 0,
        [ApprovedAt] DATETIME2 NULL,
        [ApprovedBy] UNIQUEIDENTIFIER NULL,
        [IsFeatured] BIT NOT NULL DEFAULT 0,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_EventReviews_Events FOREIGN KEY ([EventId]) REFERENCES [dbo].[Events]([Id]) ON DELETE CASCADE,
        CONSTRAINT FK_EventReviews_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE SET NULL
    );

    CREATE INDEX IX_EventReviews_EventId ON [dbo].[EventReviews]([EventId]);
    CREATE INDEX IX_EventReviews_UserId ON [dbo].[EventReviews]([UserId]);
    CREATE INDEX IX_EventReviews_IsApproved ON [dbo].[EventReviews]([IsApproved]);
    CREATE INDEX IX_EventReviews_EventId_IsApproved ON [dbo].[EventReviews]([EventId], [IsApproved]);

    PRINT 'EventReviews table created successfully';
END
ELSE
BEGIN
    -- Check and add missing columns if table exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EventReviews]') AND name = 'ApprovedAt')
    BEGIN
        ALTER TABLE [dbo].[EventReviews] ADD [ApprovedAt] DATETIME2 NULL;
        PRINT 'Added ApprovedAt column to EventReviews table';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EventReviews]') AND name = 'ApprovedBy')
    BEGIN
        ALTER TABLE [dbo].[EventReviews] ADD [ApprovedBy] UNIQUEIDENTIFIER NULL;
        PRINT 'Added ApprovedBy column to EventReviews table';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EventReviews]') AND name = 'DisplayOrder')
    BEGIN
        ALTER TABLE [dbo].[EventReviews] ADD [DisplayOrder] INT NOT NULL DEFAULT 0;
        PRINT 'Added DisplayOrder column to EventReviews table';
    END

    PRINT 'EventReviews table already exists';
END
GO

-- 4. EventHighlights Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EventHighlights]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EventHighlights] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [EventId] UNIQUEIDENTIFIER NOT NULL,
        [ImageUrl] NVARCHAR(1000) NOT NULL,
        [Title] NVARCHAR(200) NULL,
        [Description] NVARCHAR(500) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_EventHighlights_Events FOREIGN KEY ([EventId]) REFERENCES [dbo].[Events]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX IX_EventHighlights_EventId ON [dbo].[EventHighlights]([EventId]);
    CREATE INDEX IX_EventHighlights_IsActive ON [dbo].[EventHighlights]([IsActive]);

    PRINT 'EventHighlights table created successfully';
END
ELSE
BEGIN
    PRINT 'EventHighlights table already exists';
END
GO

-- 5. EventNotificationSubscriptions Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EventNotificationSubscriptions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EventNotificationSubscriptions] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Email] NVARCHAR(200) NOT NULL,
        [City] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [SubscribedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UnsubscribedAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_EventNotificationSubscriptions_Email ON [dbo].[EventNotificationSubscriptions]([Email]);
    CREATE INDEX IX_EventNotificationSubscriptions_City ON [dbo].[EventNotificationSubscriptions]([City]);
    CREATE INDEX IX_EventNotificationSubscriptions_IsActive ON [dbo].[EventNotificationSubscriptions]([IsActive]);

    PRINT 'EventNotificationSubscriptions table created successfully';
END
ELSE
BEGIN
    -- Check and add missing columns if table exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EventNotificationSubscriptions]') AND name = 'SubscribedAt')
    BEGIN
        ALTER TABLE [dbo].[EventNotificationSubscriptions] ADD [SubscribedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE();
        PRINT 'Added SubscribedAt column to EventNotificationSubscriptions table';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EventNotificationSubscriptions]') AND name = 'UnsubscribedAt')
    BEGIN
        ALTER TABLE [dbo].[EventNotificationSubscriptions] ADD [UnsubscribedAt] DATETIME2 NULL;
        PRINT 'Added UnsubscribedAt column to EventNotificationSubscriptions table';
    END

    PRINT 'EventNotificationSubscriptions table already exists';
END
GO

PRINT 'All Event tables created successfully!';
