-- Move email to HR profiles & add password column
USE [SheetFlow];
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[hr_employee_profiles]') AND name = 'email')
BEGIN
    ALTER TABLE [dbo].[hr_employee_profiles] ADD [email] NVARCHAR(255) NULL;
    ALTER TABLE [dbo].[hr_employee_profiles] ADD [password] NVARCHAR(255) NULL;
    PRINT 'Added email & password to hr_employee_profiles';
END
GO

UPDATE h SET h.[email] = u.[email]
FROM [dbo].[hr_employee_profiles] h
JOIN [dbo].[users] u ON h.[username] = u.[username]
WHERE h.[email] IS NULL AND u.[email] IS NOT NULL;
PRINT 'Migrated emails';
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND name = 'email')
BEGIN
    ALTER TABLE [dbo].[users] DROP COLUMN [email];
    PRINT 'Dropped email from users';
END
GO

PRINT 'HR Profile migration complete!';
GO
