IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO
BEGIN TRANSACTION;
CREATE TABLE [AdminUsers] (
    [Id] bigint NOT NULL IDENTITY,
    [DisplayName] nvarchar(120) NOT NULL,
    [NormalizedDisplayName] nvarchar(120) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [NormalizedPhone] nvarchar(20) NOT NULL,
    [PasswordHash] nvarchar(512) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_AdminUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [Pois] (
    [Id] bigint NOT NULL IDENTITY,
    [Slug] nvarchar(140) NOT NULL,
    [Latitude] decimal(9,6) NOT NULL,
    [Longitude] decimal(9,6) NOT NULL,
    [ImageUrl] nvarchar(500) NULL,
    [DisplayOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_Pois] PRIMARY KEY ([Id])
);

CREATE TABLE [Sponsors] (
    [Id] bigint NOT NULL IDENTITY,
    [Name] nvarchar(180) NOT NULL,
    [ContactNote] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Sponsors] PRIMARY KEY ([Id])
);

CREATE TABLE [VisitorSessions] (
    [Id] bigint NOT NULL IDENTITY,
    [SessionHash] nvarchar(64) NOT NULL,
    [DeviceType] nvarchar(12) NOT NULL,
    [FirstSeenAtUtc] datetime2 NOT NULL,
    [LastSeenAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_VisitorSessions] PRIMARY KEY ([Id])
);

CREATE TABLE [VisitRevenueRates] (
    [Id] bigint NOT NULL IDENTITY,
    [Amount] decimal(18,4) NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [EffectiveFromUtc] datetime2 NOT NULL,
    [EffectiveToUtc] datetime2 NULL,
    CONSTRAINT [PK_VisitRevenueRates] PRIMARY KEY ([Id])
);

CREATE TABLE [AuditLogs] (
    [Id] bigint NOT NULL IDENTITY,
    [AdminUserId] bigint NOT NULL,
    [Action] nvarchar(120) NOT NULL,
    [EntityType] nvarchar(80) NOT NULL,
    [EntityId] nvarchar(80) NOT NULL,
    [SafeDetails] nvarchar(1000) NULL,
    [OccurredAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_AdminUsers_AdminUserId] FOREIGN KEY ([AdminUserId]) REFERENCES [AdminUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PoiTopics] (
    [Id] bigint NOT NULL IDENTITY,
    [PoiId] bigint NOT NULL,
    [Category] nvarchar(20) NOT NULL,
    [DisplayOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_PoiTopics] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PoiTopics_Pois_PoiId] FOREIGN KEY ([PoiId]) REFERENCES [Pois] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PoiTranslations] (
    [Id] bigint NOT NULL IDENTITY,
    [PoiId] bigint NOT NULL,
    [LanguageCode] nvarchar(10) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Summary] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_PoiTranslations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PoiTranslations_Pois_PoiId] FOREIGN KEY ([PoiId]) REFERENCES [Pois] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Advertisements] (
    [Id] bigint NOT NULL IDENTITY,
    [SponsorId] bigint NULL,
    [Name] nvarchar(160) NOT NULL,
    [ImageUrl] nvarchar(500) NOT NULL,
    [TargetUrl] nvarchar(500) NOT NULL,
    [Placement] nvarchar(10) NOT NULL,
    [StartsAtUtc] datetime2 NOT NULL,
    [EndsAtUtc] datetime2 NULL,
    [DisplayOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Advertisements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Advertisements_Sponsors_SponsorId] FOREIGN KEY ([SponsorId]) REFERENCES [Sponsors] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [SponsorshipAgreements] (
    [Id] bigint NOT NULL IDENTITY,
    [SponsorId] bigint NOT NULL,
    [CommittedAmount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [Status] nvarchar(30) NOT NULL,
    [StartsAtUtc] datetime2 NOT NULL,
    [EndsAtUtc] datetime2 NULL,
    CONSTRAINT [PK_SponsorshipAgreements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SponsorshipAgreements_Sponsors_SponsorId] FOREIGN KEY ([SponsorId]) REFERENCES [Sponsors] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Visits] (
    [Id] bigint NOT NULL IDENTITY,
    [VisitorSessionId] bigint NOT NULL,
    [VisitRevenueRateId] bigint NULL,
    [AppliedRateAmount] decimal(18,4) NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [OccurredAtUtc] datetime2 NOT NULL,
    [IsValid] bit NOT NULL,
    CONSTRAINT [PK_Visits] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Visits_VisitRevenueRates_VisitRevenueRateId] FOREIGN KEY ([VisitRevenueRateId]) REFERENCES [VisitRevenueRates] ([Id]),
    CONSTRAINT [FK_Visits_VisitorSessions_VisitorSessionId] FOREIGN KEY ([VisitorSessionId]) REFERENCES [VisitorSessions] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [TopicTranslations] (
    [Id] bigint NOT NULL IDENTITY,
    [PoiTopicId] bigint NOT NULL,
    [LanguageCode] nvarchar(10) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [IsPublished] bit NOT NULL,
    CONSTRAINT [PK_TopicTranslations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TopicTranslations_PoiTopics_PoiTopicId] FOREIGN KEY ([PoiTopicId]) REFERENCES [PoiTopics] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [SponsorshipPayments] (
    [Id] bigint NOT NULL IDENTITY,
    [SponsorshipAgreementId] bigint NOT NULL,
    [ReceivedAmount] decimal(18,2) NOT NULL,
    [ReceivedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_SponsorshipPayments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SponsorshipPayments_SponsorshipAgreements_SponsorshipAgreementId] FOREIGN KEY ([SponsorshipAgreementId]) REFERENCES [SponsorshipAgreements] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_AdminUsers_NormalizedDisplayName_NormalizedPhone] ON [AdminUsers] ([NormalizedDisplayName], [NormalizedPhone]);

CREATE INDEX [IX_Advertisements_Placement_IsActive_StartsAtUtc] ON [Advertisements] ([Placement], [IsActive], [StartsAtUtc]);

CREATE INDEX [IX_Advertisements_SponsorId] ON [Advertisements] ([SponsorId]);

CREATE INDEX [IX_AuditLogs_AdminUserId] ON [AuditLogs] ([AdminUserId]);

CREATE INDEX [IX_AuditLogs_OccurredAtUtc] ON [AuditLogs] ([OccurredAtUtc]);

CREATE UNIQUE INDEX [IX_Pois_Slug] ON [Pois] ([Slug]);

CREATE UNIQUE INDEX [IX_PoiTopics_PoiId_Category_DisplayOrder] ON [PoiTopics] ([PoiId], [Category], [DisplayOrder]);

CREATE UNIQUE INDEX [IX_PoiTranslations_PoiId_LanguageCode] ON [PoiTranslations] ([PoiId], [LanguageCode]);

CREATE INDEX [IX_SponsorshipAgreements_SponsorId] ON [SponsorshipAgreements] ([SponsorId]);

CREATE INDEX [IX_SponsorshipPayments_SponsorshipAgreementId] ON [SponsorshipPayments] ([SponsorshipAgreementId]);

CREATE UNIQUE INDEX [IX_TopicTranslations_PoiTopicId_LanguageCode] ON [TopicTranslations] ([PoiTopicId], [LanguageCode]);

CREATE INDEX [IX_VisitorSessions_LastSeenAtUtc] ON [VisitorSessions] ([LastSeenAtUtc]);

CREATE UNIQUE INDEX [IX_VisitorSessions_SessionHash] ON [VisitorSessions] ([SessionHash]);

CREATE UNIQUE INDEX [IX_VisitRevenueRates_EffectiveFromUtc] ON [VisitRevenueRates] ([EffectiveFromUtc]);

CREATE INDEX [IX_Visits_VisitorSessionId_OccurredAtUtc] ON [Visits] ([VisitorSessionId], [OccurredAtUtc]);

CREATE INDEX [IX_Visits_VisitRevenueRateId] ON [Visits] ([VisitRevenueRateId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261026094918_InitialCreate', N'9.0.11');

ALTER TABLE [VisitorSessions] ADD [BrowserFamily] nvarchar(24) NOT NULL DEFAULT N'';

ALTER TABLE [VisitorSessions] ADD [OperatingSystem] nvarchar(24) NOT NULL DEFAULT N'';

CREATE TABLE [AdvertisementEvents] (
    [Id] bigint NOT NULL IDENTITY,
    [AdvertisementId] bigint NOT NULL,
    [EventType] nvarchar(16) NOT NULL,
    [SessionHash] nvarchar(64) NULL,
    [OccurredAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_AdvertisementEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AdvertisementEvents_Advertisements_AdvertisementId] FOREIGN KEY ([AdvertisementId]) REFERENCES [Advertisements] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AdvertisementEvents_AdvertisementId_OccurredAtUtc] ON [AdvertisementEvents] ([AdvertisementId], [OccurredAtUtc]);

CREATE INDEX [IX_AdvertisementEvents_EventType_OccurredAtUtc] ON [AdvertisementEvents] ([EventType], [OccurredAtUtc]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261026101427_AddOperationsManagement', N'9.0.11');

COMMIT;
GO
