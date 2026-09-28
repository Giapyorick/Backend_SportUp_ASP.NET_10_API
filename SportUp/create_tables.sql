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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928184451_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [Levels] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(200) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Levels] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928184451_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [SportCategories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Status] nvarchar(200) NOT NULL,
        CONSTRAINT [PK_SportCategories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928184451_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [Teams] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [LogoUrl] nvarchar(255) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [Status] nvarchar(100) NULL,
        CONSTRAINT [PK_Teams] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928184451_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [Venues] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [MapUrl] nvarchar(200) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Venues] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928184451_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [Matches] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [SportCategoryId] int NOT NULL,
        [VenueId] int NOT NULL,
        [TargetLevelId] int NOT NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NOT NULL,
        [TotalSlots] int NOT NULL,
        [AvailableSlots] int NOT NULL,
        [PricePerSlot] decimal(18,2) NOT NULL,
        [Note] nvarchar(1000) NOT NULL,
        [CreatedByUserId] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Matches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Matches_Levels_TargetLevelId] FOREIGN KEY ([TargetLevelId]) REFERENCES [Levels] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Matches_SportCategories_SportCategoryId] FOREIGN KEY ([SportCategoryId]) REFERENCES [SportCategories] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Matches_Venues_VenueId] FOREIGN KEY ([VenueId]) REFERENCES [Venues] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928184451_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_Matches_SportCategoryId] ON [Matches] ([SportCategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928184451_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_Matches_TargetLevelId] ON [Matches] ([TargetLevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928184451_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_Matches_VenueId] ON [Matches] ([VenueId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928184451_InitialSqlServerMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260928184451_InitialSqlServerMigration', N'10.0.12');
END;

COMMIT;
GO


BEGIN TRANSACTION;

-- 1. Thêm dữ liệu mẫu vào bảng Levels (Cấp độ)
INSERT INTO [Levels] ([Name], [Description], [Status])
VALUES 
(N'Cơ bản', N'Dành cho người mới bắt đầu làm quen môn thể thao', N'Active'),
(N'Trung bình', N'Đã nắm vững kỹ năng cơ bản, chơi phong trào thường xuyên', N'Active'),
(N'Nâng cao', N'Trình độ bán chuyên hoặc thi đấu giải phong trào', N'Active');

-- 2. Thêm dữ liệu mẫu vào bảng SportCategories (Môn thể thao)
INSERT INTO [SportCategories] ([Name], [Description], [Status])
VALUES 
(N'Bóng đá', N'Sân cỏ nhân tạo 5 người và 7 người', N'Active'),
(N'Cầu lông', N'Sân thảm tiêu chuẩn trong nhà', N'Active'),
(N'Pickleball', N'Môn thể thao vợt hiện đại trên sân cứng', N'Active'),
(N'Bóng rổ', N'Sân bóng rổ nửa sân (3x3) hoặc nguyên sân (5x5)', N'Active');

-- 3. Thêm dữ liệu mẫu vào bảng Venues (Địa điểm thi đấu)
INSERT INTO [Venues] ([Name], [Address], [MapUrl], [Status])
VALUES 
(N'Sân bóng Chảo Lửa', N'30 Phan Thúc Duyện, Phường 4, Tân Bình, TP.HCM', N'https://maps.app.goo.gl/example1', N'Active'),
(N'CLB Cầu Lông Viettel', N'158 Hoàng Hoa Thám, Tân Bình, TP.HCM', N'https://maps.app.goo.gl/example2', N'Active'),
(N'Sân Pickleball D-Sport Club', N'Số 1 Đường số 8, KDC Him Lam, Quận 7, TP.HCM', N'https://maps.app.goo.gl/example3', N'Active');

-- 4. Thêm dữ liệu mẫu vào bảng Teams (Đội / Nhóm thể thao)
INSERT INTO [Teams] ([Name], [Description], [LogoUrl], [CreatedAt], [Status])
VALUES 
(N'FC Chiến Binh', N'Đội bóng giao lưu vui vẻ tối thứ 4 hàng tuần', N'https://placehold.co/150x150?text=FC+ChienBinh', SYSUTCDATETIME(), N'Active'),
(N'Pickleball Master Club', N'Hội người chơi Pickleball khu vực Nam Sài Gòn', N'https://placehold.co/150x150?text=Pickleball+Club', SYSUTCDATETIME(), N'Active');

-- 5. Thêm dữ liệu mẫu vào bảng Matches (Kèo đấu / Trận giao lưu)
-- Chú ý: Cần ID khớp với Levels (1-3), SportCategories (1-4), Venues (1-3) ở trên
INSERT INTO [Matches] (
    [Title], 
    [SportCategoryId], 
    [VenueId], 
    [TargetLevelId], 
    [StartTime], 
    [EndTime], 
    [TotalSlots], 
    [AvailableSlots], 
    [PricePerSlot], 
    [Note], 
    [CreatedByUserId], 
    [CreatedAt]
)
VALUES 
(
    N'Giao lưu bóng đá sân 7 - Tìm 3 bạn đá cánh', 
    1, -- Bóng đá
    1, -- Sân Chảo Lửa
    2, -- Trình độ Trung bình
    DATEADD(DAY, 1, SYSUTCDATETIME()), 
    DATEADD(MINUTE, 90, DATEADD(DAY, 1, SYSUTCDATETIME())), 
    14, 
    3, 
    50000.00, 
    N'Mang áo pitch xanh/cam, tiền sân chia đều tại chỗ, không chơi xấu.', 
    N'user_admin_01', 
    SYSUTCDATETIME()
),
(
    N'Đánh cầu lông đôi nam nữ cuối tuần', 
    2, -- Cầu lông
    2, -- Sân Viettel
    1, -- Cơ bản
    DATEADD(DAY, 2, SYSUTCDATETIME()), 
    DATEADD(HOUR, 2, DATEADD(DAY, 2, SYSUTCDATETIME())), 
    6, 
    2, 
    75000.00, 
    N'Cầu Thành Công do chủ kèo chuẩn bị sẵn, mang theo vợt cá nhân.', 
    N'user_admin_02', 
    SYSUTCDATETIME()
),
(
    N'Trải nghiệm Pickleball giao lưu buổi sáng', 
    3, -- Pickleball
    3, -- Sân D-Sport Club
    1, -- Cơ bản
    DATEADD(DAY, 3, SYSUTCDATETIME()), 
    DATEADD(HOUR, 2, DATEADD(DAY, 3, SYSUTCDATETIME())), 
    4, 
    1, 
    100000.00, 
    N'Có vợt mượn miễn phí cho ai chưa có dụng cụ.', 
    N'user_admin_01', 
    SYSUTCDATETIME()
);

COMMIT;
GO

