-- SheetFlow User Accounts Initialization Script
-- All passwords are: 123456

USE [SheetFlow];
GO

-- Admin
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'admin')
BEGIN
INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at]) VALUES ('admin', N'tRCzhK8YFHS5RGCR7GUjNg==.LL7KbAoUqlJq05xJ4ahLpcttv++3t4d1EfBEZ0jMaWQ=', N'系統管理員', 'admin@sheetflow.com', 'Admin', 1, GETUTCDATE(), GETUTCDATE());
END
GO

-- 會計部主管
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'acct_mgr')
BEGIN
INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at]) VALUES ('acct_mgr', N'Hq5ovLmNz5679RPwpwpusA==.Mhu1Pi4roZFQ6PhNbvi6nk9KFTHnrGZFgY8VKMACJog=', N'會計部主管', 'acct_mgr@sheetflow.com', 'Manager', 1, GETUTCDATE(), GETUTCDATE());
END
GO

-- 會計部組員
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'acct_user')
BEGIN
INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at]) VALUES ('acct_user', N'zano6HuiisZ6UDx+l6wSAg==.aETAJNS2/FhcVxdpL5IBO+Honu7mOgQfisoB6BWy9Qw=', N'會計部組員', 'acct_user@sheetflow.com', 'User', 1, GETUTCDATE(), GETUTCDATE());
END
GO

-- 財務部主管
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'fin_mgr')
BEGIN
INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at]) VALUES ('fin_mgr', N'4Gsw6vpKthXKWryL82fURA==.QFLcOSOYrT80r71BARNbiMNQmOFFPwXbMmlUOiO8m0I=', N'財務部主管', 'fin_mgr@sheetflow.com', 'Manager', 1, GETUTCDATE(), GETUTCDATE());
END
GO

-- 財務部組員
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'fin_user')
BEGIN
INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at]) VALUES ('fin_user', N'SCEgDQdkvGjtSaQugI4rbg==.4/TnNo4mIRyNKtA3e8e0KW+IG6hciRhqVA51Dcnvij8=', N'財務部組員', 'fin_user@sheetflow.com', 'User', 1, GETUTCDATE(), GETUTCDATE());
END
GO

-- 食安部主管
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'food_mgr')
BEGIN
INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at]) VALUES ('food_mgr', N'9/R8VMDgmwegOQEfdY+YdA==.mEeGOQ+NrBYLd+G5webBZMfDM4D/vZ/Tb92tkojFh18=', N'食安部主管', 'food_mgr@sheetflow.com', 'Manager', 1, GETUTCDATE(), GETUTCDATE());
END
GO

-- 食安部組員
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'food_user')
BEGIN
INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at]) VALUES ('food_user', N'YX0KleKor3Ffxfwx41n9uw==.b9Ng5VCXPe9rqjwcUCEGjJZkuldPcJA51kO5D24bDNA=', N'食安部組員', 'food_user@sheetflow.com', 'User', 1, GETUTCDATE(), GETUTCDATE());
END
GO

-- 資訊部主管
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'it_mgr')
BEGIN
INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at]) VALUES ('it_mgr', N'REApbpOe4Pi92CoKbXQNww==.Jxhmup8vJb2zvXaHoUx7ucLbueLDW4l6zpMNWNKQwEY=', N'資訊部主管', 'it_mgr@sheetflow.com', 'Manager', 1, GETUTCDATE(), GETUTCDATE());
END
GO

-- 資訊部組員
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'it_user')
BEGIN
INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at]) VALUES ('it_user', N'kUMZT/J2JJ7rGaG9HAYYKA==.OduJt/E2TDkBSJNJMDFV/asn4fK2IYWJkc2HclUFLeo=', N'資訊部組員', 'it_user@sheetflow.com', 'User', 1, GETUTCDATE(), GETUTCDATE());
END
GO

-- 企劃部主管
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'plan_mgr')
BEGIN
INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at]) VALUES ('plan_mgr', N'B2cHtbkGZqpPhMUbTRwH/Q==.Gu55rwoQY02tXB8ckNnuzi6WSX0IVkR8p3wM4XG7oA4=', N'企劃部主管', 'plan_mgr@sheetflow.com', 'Manager', 1, GETUTCDATE(), GETUTCDATE());
END
GO

-- 企劃部組員
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'plan_user')
BEGIN
INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at]) VALUES ('plan_user', N'lvGP/TRcGDHkCEXogpBi+A==.btYdwOz/Ih1FToctD4LVSrKvJChUpykaH5xGS6+GTJE=', N'企劃部組員', 'plan_user@sheetflow.com', 'User', 1, GETUTCDATE(), GETUTCDATE());
END
GO

PRINT 'SheetFlow user accounts initialization complete!';
GO
