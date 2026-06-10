-- SheetFlow Database Initialization Script
-- Run this script against your SQL Server to create the database and tables

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'StoreFlow')
BEGIN
    CREATE DATABASE [StoreFlow];
END
GO

USE [StoreFlow];
GO

-- Users table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[users] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [username] NVARCHAR(50) NOT NULL,
        [password_hash] NVARCHAR(255) NOT NULL,
        [display_name] NVARCHAR(100) NOT NULL,
        [email] NVARCHAR(255) NOT NULL DEFAULT '',
        [line_user_id] NVARCHAR(255) NULL,
        [role] NVARCHAR(20) NOT NULL DEFAULT 'User',
        [is_active] BIT NOT NULL DEFAULT 1,
        [created_at] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE UNIQUE INDEX [IX_users_username] ON [dbo].[users] ([username]);
END
GO

-- Form templates table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[form_templates]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[form_templates] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL,
        [description] NVARCHAR(500) NULL,
        [excel_file_path] NVARCHAR(500) NULL,
        [is_active] BIT NOT NULL DEFAULT 1,
        [created_by] BIGINT NOT NULL,
        [created_at] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Form template fields table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[form_template_fields]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[form_template_fields] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [form_template_id] BIGINT NOT NULL,
        [field_name] NVARCHAR(100) NOT NULL,
        [field_key] NVARCHAR(100) NOT NULL,
        [field_type] NVARCHAR(20) NOT NULL DEFAULT 'Text',
        [is_required] BIT NOT NULL DEFAULT 1,
        [sort_order] INT NOT NULL DEFAULT 0,
        [options_json] NVARCHAR(MAX) NULL,
        [default_value] NVARCHAR(500) NULL,
        [is_visible] BIT NOT NULL DEFAULT 1,
        [created_at] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX [IX_form_template_fields_template] ON [dbo].[form_template_fields] ([form_template_id]);
END
GO

-- Form requests table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[form_requests]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[form_requests] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [request_no] NVARCHAR(50) NOT NULL,
        [form_template_id] BIGINT NOT NULL,
        [applicant_id] BIGINT NOT NULL,
        [status] NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        [submitted_at] DATETIME NULL,
        [approved_at] DATETIME NULL,
        [rejected_at] DATETIME NULL,
        [created_at] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX [IX_form_requests_applicant] ON [dbo].[form_requests] ([applicant_id]);
    CREATE INDEX [IX_form_requests_status] ON [dbo].[form_requests] ([status]);
END
GO

-- Form request values table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[form_request_values]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[form_request_values] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [form_request_id] BIGINT NOT NULL,
        [field_id] BIGINT NOT NULL,
        [field_key] NVARCHAR(100) NOT NULL,
        [field_name] NVARCHAR(100) NOT NULL,
        [field_value] NVARCHAR(MAX) NULL,
        [created_at] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX [IX_form_request_values_request] ON [dbo].[form_request_values] ([form_request_id]);
END
GO

-- Approval logs table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[approval_logs]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[approval_logs] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [form_request_id] BIGINT NOT NULL,
        [action] NVARCHAR(20) NOT NULL,
        [actor_id] BIGINT NOT NULL,
        [comment] NVARCHAR(1000) NULL,
        [created_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX [IX_approval_logs_request] ON [dbo].[approval_logs] ([form_request_id]);
END
GO

-- Notification logs table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[notification_logs]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[notification_logs] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [form_request_id] BIGINT NOT NULL,
        [notify_type] NVARCHAR(20) NOT NULL,
        [recipient] NVARCHAR(255) NOT NULL,
        [subject] NVARCHAR(255) NOT NULL,
        [content] NVARCHAR(MAX) NOT NULL,
        [is_success] BIT NOT NULL DEFAULT 0,
        [error_message] NVARCHAR(MAX) NULL,
        [sent_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Seed default admin user (password: admin123)
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'admin')
BEGIN
    INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at])
    VALUES ('admin', 'JqiNmq8O5c3ZyoMbBVuX1A==./CNVrLEihWclRfn4ohL8ldkd/MB+F6R4waH1knu6xYg=', N'系統管理員', 'admin@sheetflow.com', 'Admin', 1, GETUTCDATE(), GETUTCDATE());
END
GO

-- Seed default manager user (password: manager123)
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'manager')
BEGIN
    INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at])
    VALUES ('manager', 'A3kpHT09ElRZL75sYIlSzg==.0BFcOPzNqaPxs4MZSaTQebAlmnkzaJGsalTt4mGhDV0=', N'主管', 'manager@sheetflow.com', 'Manager', 1, GETUTCDATE(), GETUTCDATE());
END
GO

-- Seed default user (password: user123)
IF NOT EXISTS (SELECT * FROM [dbo].[users] WHERE [username] = 'user')
BEGIN
    INSERT INTO [dbo].[users] ([username], [password_hash], [display_name], [email], [role], [is_active], [created_at], [updated_at])
    VALUES ('user', 'RmDLY+DM7Ykl3L+FVSrJ5Q==.dkxDPs44KRECbe42O2jg72XTmPvSIQCjSDCDcjXcEiE=', N'使用者', 'user@sheetflow.com', 'User', 1, GETUTCDATE(), GETUTCDATE());
END
GO

PRINT 'SheetFlow database initialization complete!';
GO
