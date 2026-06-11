-- HR Employee Profiles Migration
USE [SheetFlow];
GO

-- HR Employee Profiles table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[hr_employee_profiles]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[hr_employee_profiles] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [username] NVARCHAR(50) NOT NULL UNIQUE,
        [full_name] NVARCHAR(50) NOT NULL,
        [highest_education] NVARCHAR(50) NULL,
        [school_name] NVARCHAR(100) NULL,
        [birthday] DATE NULL,
        [phone] NVARCHAR(20) NULL,
        [address] NVARCHAR(200) NULL,
        [id_number] NVARCHAR(10) NULL,
        [created_at] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX [IX_hr_employee_profiles_username] ON [dbo].[hr_employee_profiles] ([username]);
    PRINT 'Created hr_employee_profiles table';
END
GO

-- Seed fake employee data
IF NOT EXISTS (SELECT 1 FROM [dbo].[hr_employee_profiles])
BEGIN
    INSERT INTO [dbo].[hr_employee_profiles] ([username],[full_name],[highest_education],[school_name],[birthday],[phone],[address],[id_number],[created_at],[updated_at])
    VALUES
    ('admin', N'王志明', N'碩士', N'國立台灣大學', '1985-03-15', '0912-345-678', N'台北市大安區信義路三段100號', 'A123456789', GETUTCDATE(), GETUTCDATE()),
    ('acct_mgr', N'陳雅婷', N'碩士', N'國立政治大學', '1988-07-22', '0923-456-789', N'台北市中山區南京東路二段50號', 'B234567890', GETUTCDATE(), GETUTCDATE()),
    ('acct_user', N'林小芳', N'學士', N'輔仁大學', '1992-11-08', '0934-567-890', N'新北市板橋區中山路一段200號', 'C345678901', GETUTCDATE(), GETUTCDATE()),
    ('fin_mgr', N'張建宏', N'碩士', N'國立台灣大學', '1986-05-30', '0945-678-901', N'台北市信義區松仁路88號', 'D456789012', GETUTCDATE(), GETUTCDATE()),
    ('fin_user', N'黃美玲', N'學士', N'東吳大學', '1993-09-14', '0956-789-012', N'台北市內湖區瑞光路300號', 'E567890123', GETUTCDATE(), GETUTCDATE()),
    ('food_mgr', N'劉家豪', N'碩士', N'國立中興大學', '1987-12-03', '0967-890-123', N'台中市西屯區台灣大道二段100號', 'F678901234', GETUTCDATE(), GETUTCDATE()),
    ('food_user', N'吳佩珊', N'學士', N'逢甲大學', '1994-04-25', '0978-901-234', N'台中市北屯區崇德路三段150號', 'G789012345', GETUTCDATE(), GETUTCDATE()),
    ('it_mgr', N'蔡宇庭', N'碩士', N'國立交通大學', '1989-08-17', '0989-012-345', N'新竹市東區光復路二段100號', 'H890123456', GETUTCDATE(), GETUTCDATE()),
    ('it_user', N'鄭凱文', N'學士', N'國立清華大學', '1995-01-20', '0990-123-456', N'新竹市北區中正路200號', 'I901234567', GETUTCDATE(), GETUTCDATE()),
    ('plan_mgr', N'蘇怡君', N'碩士', N'國立台灣師範大學', '1990-06-11', '0911-234-567', N'台北市大安區羅斯福路四段50號', 'J012345678', GETUTCDATE(), GETUTCDATE()),
    ('plan_user', N'周雅琪', N'學士', N'文化大學', '1996-10-05', '0922-345-678', N'台北市士林區華岡路55號', 'K123456780', GETUTCDATE(), GETUTCDATE());

    PRINT 'Seeded HR employee profiles';
END
GO

PRINT 'HR Employee Profiles migration complete!';
GO
