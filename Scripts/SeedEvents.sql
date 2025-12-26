-- =============================================
-- Event Management System - Table Creation and Seed Data
-- Description: Creates all Event-related tables and seeds sample data
-- =============================================

-- Check if tables already exist, if so, skip creation
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Events]') AND type in (N'U'))
BEGIN
    -- Create Events table
    CREATE TABLE [dbo].[Events] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [ShortDescription] NVARCHAR(500) NULL,
        [Slug] NVARCHAR(200) NOT NULL,
        [EventDate] DATETIME2 NOT NULL,
        [EventEndDate] DATETIME2 NULL,
        [EventTime] NVARCHAR(50) NULL,
        [City] NVARCHAR(100) NOT NULL,
        [Venue] NVARCHAR(200) NOT NULL,
        [Address] NVARCHAR(500) NULL,
        [MapUrl] NVARCHAR(500) NULL,
        [Price] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Currency] NVARCHAR(10) NULL DEFAULT 'VND',
        [FeaturedImageUrl] NVARCHAR(500) NULL,
        [GalleryImages] NVARCHAR(MAX) NULL,
        [MaxCapacity] INT NULL,
        [RegisteredCount] INT NOT NULL DEFAULT 0,
        [IsRegistrationOpen] BIT NOT NULL DEFAULT 1,
        [RegistrationDeadline] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsFeatured] BIT NOT NULL DEFAULT 0,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Upcoming',
        [MetaTitle] NVARCHAR(200) NULL,
        [MetaDescription] NVARCHAR(500) NULL,
        [Tags] NVARCHAR(500) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );
    PRINT 'Events table created successfully.';
END
ELSE
    PRINT 'Events table already exists.';

-- Create EventRegistrations table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EventRegistrations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EventRegistrations] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [EventId] UNIQUEIDENTIFIER NOT NULL,
        [UserId] UNIQUEIDENTIFIER NULL,
        [FullName] NVARCHAR(200) NOT NULL,
        [Email] NVARCHAR(100) NOT NULL,
        [PhoneNumber] NVARCHAR(20) NULL,
        [NumberOfAttendees] INT NOT NULL DEFAULT 1,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Confirmed',
        [RegistrationDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CancellationDate] DATETIME2 NULL,
        [CancellationReason] NVARCHAR(500) NULL,
        [AmountPaid] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [PaymentStatus] NVARCHAR(50) NULL DEFAULT 'Pending',
        [PaymentMethod] NVARCHAR(50) NULL,
        [TransactionId] NVARCHAR(100) NULL,
        [Notes] NVARCHAR(500) NULL,
        [SpecialRequirements] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_EventRegistrations_Events] FOREIGN KEY ([EventId]) REFERENCES [Events]([Id]) ON DELETE CASCADE
    );
    PRINT 'EventRegistrations table created successfully.';
END
ELSE
    PRINT 'EventRegistrations table already exists.';

-- Create EventReviews table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EventReviews]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EventReviews] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [EventId] UNIQUEIDENTIFIER NOT NULL,
        [UserId] UNIQUEIDENTIFIER NULL,
        [ReviewerName] NVARCHAR(200) NOT NULL,
        [ReviewerEmail] NVARCHAR(100) NULL,
        [Rating] INT NOT NULL,
        [Comment] NVARCHAR(1000) NOT NULL,
        [IsApproved] BIT NOT NULL DEFAULT 0,
        [IsFeatured] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_EventReviews_Events] FOREIGN KEY ([EventId]) REFERENCES [Events]([Id]) ON DELETE CASCADE,
        CONSTRAINT [CK_EventReviews_Rating] CHECK ([Rating] >= 1 AND [Rating] <= 5)
    );
    PRINT 'EventReviews table created successfully.';
END
ELSE
    PRINT 'EventReviews table already exists.';

-- Create EventHighlights table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EventHighlights]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EventHighlights] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [EventId] UNIQUEIDENTIFIER NOT NULL,
        [ImageUrl] NVARCHAR(500) NOT NULL,
        [Title] NVARCHAR(200) NULL,
        [Description] NVARCHAR(500) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_EventHighlights_Events] FOREIGN KEY ([EventId]) REFERENCES [Events]([Id]) ON DELETE CASCADE
    );
    PRINT 'EventHighlights table created successfully.';
END
ELSE
    PRINT 'EventHighlights table already exists.';

-- Create EventNotificationSubscriptions table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EventNotificationSubscriptions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EventNotificationSubscriptions] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Email] NVARCHAR(100) NOT NULL,
        [City] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );
    PRINT 'EventNotificationSubscriptions table created successfully.';
END
ELSE
    PRINT 'EventNotificationSubscriptions table already exists.';

GO

-- =============================================
-- Seed Sample Event Data
-- =============================================

-- Check if events already exist to avoid duplicates
IF NOT EXISTS (SELECT 1 FROM Events WHERE Slug = 'coffee-tasting-workshop-hanoi')
BEGIN
    DECLARE @Event1Id UNIQUEIDENTIFIER = NEWID();
    DECLARE @Event2Id UNIQUEIDENTIFIER = NEWID();
    DECLARE @Event3Id UNIQUEIDENTIFIER = NEWID();
    DECLARE @Event4Id UNIQUEIDENTIFIER = NEWID();
    DECLARE @Event5Id UNIQUEIDENTIFIER = NEWID();

    -- Insert Sample Events
    INSERT INTO [Events] ([Id], [Title], [Description], [ShortDescription], [Slug], [EventDate], [EventEndDate], [EventTime],
                          [City], [Venue], [Address], [MapUrl], [Price], [Currency], [FeaturedImageUrl], [GalleryImages],
                          [MaxCapacity], [RegisteredCount], [IsRegistrationOpen], [RegistrationDeadline],
                          [IsActive], [IsFeatured], [Status], [MetaTitle], [MetaDescription], [Tags], [DisplayOrder],
                          [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
    -- Event 1: Coffee Tasting Workshop (Hanoi)
    (@Event1Id,
     'Coffee Tasting Workshop - Discover Vietnamese Coffee',
     'Join us for an immersive coffee tasting experience where you''ll learn about the rich history of Vietnamese coffee culture, explore different brewing methods, and taste a variety of premium coffee beans from across Vietnam. Our expert baristas will guide you through the sensory journey of coffee appreciation, from understanding flavor notes to mastering the perfect brew. This workshop is perfect for coffee enthusiasts and anyone wanting to deepen their knowledge of specialty coffee.',
     'Discover the art of Vietnamese coffee tasting with expert baristas in this hands-on workshop.',
     'coffee-tasting-workshop-hanoi',
     DATEADD(DAY, 14, GETUTCDATE()), -- Event in 2 weeks
     NULL,
     '14:00 - 16:30',
     'Hanoi',
     'GOK Cafe Hanoi Central',
     '123 Tran Hung Dao Street, Hoan Kiem District, Hanoi',
     'https://maps.google.com/?q=GOK+Cafe+Hanoi',
     250000,
     'VND',
     'https://images.unsplash.com/photo-1511920170033-f8396924c348',
     'https://images.unsplash.com/photo-1495474472287-4d71bcdd2085,https://images.unsplash.com/photo-1509042239860-f550ce710b93',
     25,
     8,
     1,
     DATEADD(DAY, 12, GETUTCDATE()),
     1,
     1,
     'Published',
     'Coffee Tasting Workshop Hanoi | GOK Cafe',
     'Learn the art of coffee tasting in our exclusive workshop at GOK Cafe Hanoi. Book your spot today!',
     'coffee,workshop,tasting,hanoi,specialty coffee',
     1,
     GETUTCDATE(),
     GETUTCDATE(),
     0),

    -- Event 2: Latte Art Competition (Ho Chi Minh City)
    (@Event2Id,
     'Latte Art Championship - Show Your Skills',
     'Calling all baristas and coffee artists! Join our Latte Art Championship and compete for exciting prizes. Whether you''re a professional barista or an enthusiastic amateur, this is your chance to showcase your creativity and skills. The competition will feature multiple rounds, with judges evaluating technique, creativity, and presentation. Participants will have access to premium espresso machines and fresh milk. Prizes include professional barista equipment, gift vouchers, and the prestigious title of GOK Cafe Latte Art Champion.',
     'Compete in our Latte Art Championship and win amazing prizes!',
     'latte-art-championship-hcmc',
     DATEADD(DAY, 21, GETUTCDATE()), -- Event in 3 weeks
     DATEADD(DAY, 21, GETUTCDATE()),
     '09:00 - 17:00',
     'Ho Chi Minh City',
     'GOK Cafe Saigon Hub',
     '456 Nguyen Hue Boulevard, District 1, Ho Chi Minh City',
     'https://maps.google.com/?q=GOK+Cafe+Saigon',
     150000,
     'VND',
     'https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd',
     'https://images.unsplash.com/photo-1442512595331-e89e73853f31,https://images.unsplash.com/photo-1497935586351-b67a49e012bf',
     50,
     32,
     1,
     DATEADD(DAY, 18, GETUTCDATE()),
     1,
     1,
     'Published',
     'Latte Art Championship HCMC | GOK Cafe',
     'Join the Latte Art Championship at GOK Cafe Saigon. Register now for your chance to win!',
     'latte art,competition,barista,ho chi minh city,coffee art',
     2,
     GETUTCDATE(),
     GETUTCDATE(),
     0),

    -- Event 3: Coffee Farm Tour (Da Lat)
    (@Event3Id,
     'Coffee Farm Tour - From Bean to Cup',
     'Escape to the highlands of Da Lat for an unforgettable coffee farm experience. This full-day tour takes you to our partner coffee plantations where you''ll witness the entire coffee production process firsthand. Walk through lush coffee fields, learn about sustainable farming practices, participate in coffee cherry picking (seasonal), observe the processing and roasting methods, and enjoy fresh farm-to-cup coffee. The tour includes transportation, lunch with local specialties, and a bag of freshly roasted coffee beans to take home. Limited spots available!',
     'Visit coffee farms in Da Lat and learn about coffee production from bean to cup.',
     'coffee-farm-tour-dalat',
     DATEADD(DAY, 30, GETUTCDATE()), -- Event in 1 month
     DATEADD(DAY, 30, GETUTCDATE()),
     '07:00 - 18:00',
     'Da Lat',
     'Highland Coffee Plantations',
     'Xuan Truong Commune, Da Lat City, Lam Dong Province',
     'https://maps.google.com/?q=Coffee+Farm+Dalat',
     850000,
     'VND',
     'https://images.unsplash.com/photo-1447933601403-0c6688de566e',
     'https://images.unsplash.com/photo-1559056199-641a0ac8b55e,https://images.unsplash.com/photo-1587734195503-904fca47e0e9',
     30,
     15,
     1,
     DATEADD(DAY, 25, GETUTCDATE()),
     1,
     1,
     'Published',
     'Coffee Farm Tour Da Lat | GOK Cafe Experience',
     'Experience coffee cultivation firsthand with our exclusive farm tour in Da Lat. Book your adventure now!',
     'coffee farm,da lat,tour,coffee production,sustainable farming',
     3,
     GETUTCDATE(),
     GETUTCDATE(),
     0),

    -- Event 4: Brewing Masterclass (Hanoi)
    (@Event4Id,
     'Home Brewing Masterclass - Perfect Your Technique',
     'Master the art of brewing coffee at home with our comprehensive brewing masterclass. This intensive session covers multiple brewing methods including pour-over, French press, AeroPress, cold brew, and Vietnamese phin filter. Learn the science behind extraction, understand coffee-to-water ratios, control brewing variables, and discover how to adjust your technique to highlight different flavor profiles. Each participant receives a brewing starter kit and detailed recipe cards. Perfect for home coffee enthusiasts looking to elevate their daily coffee experience.',
     'Learn professional brewing techniques for making perfect coffee at home.',
     'home-brewing-masterclass-hanoi',
     DATEADD(DAY, 45, GETUTCDATE()), -- Event in 1.5 months
     NULL,
     '10:00 - 13:00',
     'Hanoi',
     'GOK Cafe Training Center',
     '789 Ba Trieu Street, Hai Ba Trung District, Hanoi',
     'https://maps.google.com/?q=GOK+Cafe+Training+Hanoi',
     350000,
     'VND',
     'https://images.unsplash.com/photo-1495474472287-4d71bcdd2085',
     'https://images.unsplash.com/photo-1506619216599-9d16d0903dfd,https://images.unsplash.com/photo-1610889556528-9a770e32642f',
     20,
     5,
     1,
     DATEADD(DAY, 42, GETUTCDATE()),
     1,
     0,
     'Published',
     'Home Brewing Masterclass | GOK Cafe',
     'Join our brewing masterclass and learn to make cafe-quality coffee at home. Limited seats available!',
     'brewing,masterclass,coffee making,hanoi,home brewing',
     4,
     GETUTCDATE(),
     GETUTCDATE(),
     0),

    -- Event 5: Coffee & Music Night (Ho Chi Minh City)
    (@Event5Id,
     'Coffee & Live Music Night',
     'Unwind and enjoy an evening of great coffee and live acoustic music at GOK Cafe. This monthly event features local talented musicians performing jazz, folk, and indie music in an intimate coffee house setting. Savor our special evening menu featuring unique coffee cocktails, premium single-origin brews, and delicious pastries. Whether you''re a music lover, coffee enthusiast, or just looking for a relaxing night out, this event offers the perfect blend of culture and community. Advance booking recommended for best seating.',
     'Enjoy live acoustic music with specialty coffee in a cozy atmosphere.',
     'coffee-music-night-hcmc',
     DATEADD(DAY, 7, GETUTCDATE()), -- Event in 1 week
     NULL,
     '19:00 - 22:00',
     'Ho Chi Minh City',
     'GOK Cafe Acoustic Lounge',
     '321 Le Loi Street, District 1, Ho Chi Minh City',
     'https://maps.google.com/?q=GOK+Cafe+Lounge+Saigon',
     100000,
     'VND',
     'https://images.unsplash.com/photo-1501339847302-ac426a4a7cbb',
     'https://images.unsplash.com/photo-1514525253161-7a46d19cd819,https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3',
     60,
     42,
     1,
     DATEADD(DAY, 6, GETUTCDATE()),
     1,
     1,
     'Published',
     'Coffee & Live Music Night | GOK Cafe Saigon',
     'Experience live acoustic performances while enjoying specialty coffee at GOK Cafe. Book your table now!',
     'live music,coffee,acoustic,ho chi minh city,night event',
     5,
     GETUTCDATE(),
     GETUTCDATE(),
     0);

    PRINT 'Sample events inserted successfully.';

    -- Insert Sample Event Reviews
    INSERT INTO [EventReviews] ([Id], [EventId], [UserId], [ReviewerName], [ReviewerEmail], [Rating], [Comment], [IsApproved], [IsFeatured], [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
    (NEWID(), @Event1Id, NULL, 'Nguyen Van A', 'nguyenvana@email.com', 5, 'Amazing workshop! The baristas were very knowledgeable and passionate. I learned so much about Vietnamese coffee culture.', 1, 1, GETUTCDATE(), GETUTCDATE(), 0),
    (NEWID(), @Event1Id, NULL, 'Tran Thi B', 'tranthib@email.com', 5, 'Best coffee tasting experience ever! The variety of beans and brewing methods was impressive.', 1, 1, GETUTCDATE(), GETUTCDATE(), 0),
    (NEWID(), @Event2Id, NULL, 'Le Van C', 'levanc@email.com', 4, 'Great competition with very talented participants. The atmosphere was energetic and fun!', 1, 0, GETUTCDATE(), GETUTCDATE(), 0),
    (NEWID(), @Event5Id, NULL, 'Pham Thi D', 'phamthid@email.com', 5, 'Perfect blend of coffee and music. The acoustic performances were beautiful and the coffee was excellent.', 1, 1, GETUTCDATE(), GETUTCDATE(), 0),
    (NEWID(), @Event5Id, NULL, 'Hoang Van E', 'hoangvane@email.com', 5, 'Loved the intimate atmosphere and the talented musicians. Will definitely come back next month!', 1, 0, GETUTCDATE(), GETUTCDATE(), 0);

    PRINT 'Sample event reviews inserted successfully.';

    -- Insert Sample Event Highlights
    INSERT INTO [EventHighlights] ([Id], [EventId], [ImageUrl], [Title], [Description], [DisplayOrder], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
    (NEWID(), @Event1Id, 'https://images.unsplash.com/photo-1442411210769-b95c4632195e', 'Expert Guidance', 'Learn from professional baristas', 1, 1, GETUTCDATE(), GETUTCDATE(), 0),
    (NEWID(), @Event1Id, 'https://images.unsplash.com/photo-1495474472287-4d71bcdd2085', 'Premium Beans', 'Taste rare Vietnamese coffee varieties', 2, 1, GETUTCDATE(), GETUTCDATE(), 0),
    (NEWID(), @Event2Id, 'https://images.unsplash.com/photo-1511920170033-f8396924c348', 'Live Competition', 'Watch skilled baristas compete', 1, 1, GETUTCDATE(), GETUTCDATE(), 0),
    (NEWID(), @Event3Id, 'https://images.unsplash.com/photo-1447933601403-0c6688de566e', 'Farm Experience', 'Walk through coffee plantations', 1, 1, GETUTCDATE(), GETUTCDATE(), 0),
    (NEWID(), @Event3Id, 'https://images.unsplash.com/photo-1559056199-641a0ac8b55e', 'Coffee Processing', 'See how beans are processed', 2, 1, GETUTCDATE(), GETUTCDATE(), 0);

    PRINT 'Sample event highlights inserted successfully.';

    -- Insert Sample Notification Subscriptions
    INSERT INTO [EventNotificationSubscriptions] ([Id], [Email], [City], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
    (NEWID(), 'coffee.lover1@email.com', 'Hanoi', 1, GETUTCDATE(), GETUTCDATE(), 0),
    (NEWID(), 'coffee.lover2@email.com', 'Ho Chi Minh City', 1, GETUTCDATE(), GETUTCDATE(), 0),
    (NEWID(), 'coffee.lover3@email.com', NULL, 1, GETUTCDATE(), GETUTCDATE(), 0);

    PRINT 'Sample notification subscriptions inserted successfully.';

    PRINT '==============================================';
    PRINT 'Event Management System setup completed!';
    PRINT '==============================================';
    PRINT 'Summary:';
    PRINT '- 5 Events created';
    PRINT '- 5 Event Reviews created';
    PRINT '- 5 Event Highlights created';
    PRINT '- 3 Notification Subscriptions created';
    PRINT '==============================================';
END
ELSE
BEGIN
    PRINT 'Sample events already exist. Skipping seed data insertion.';
END

GO
