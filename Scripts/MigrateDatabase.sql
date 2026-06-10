-- SheetFlow Database Migration: Approval Workflows & Form Dependencies
-- Run this against the SheetFlow database

USE [SheetFlow];
GO

-- 1. Add department column to users
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND name = 'department')
BEGIN
    ALTER TABLE [dbo].[users] ADD [department] NVARCHAR(50) NULL;
    PRINT 'Added department column to users';
END
GO

-- 2. Approval workflows table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[approval_workflows]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[approval_workflows] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [form_template_id] BIGINT NOT NULL,
        [department] NVARCHAR(50) NULL,
        [name] NVARCHAR(100) NOT NULL,
        [is_active] BIT NOT NULL DEFAULT 1,
        [created_at] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX [IX_approval_workflows_template] ON [dbo].[approval_workflows] ([form_template_id]);
    PRINT 'Created approval_workflows table';
END
GO

-- 3. Approval workflow steps table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[approval_workflow_steps]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[approval_workflow_steps] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [approval_workflow_id] BIGINT NOT NULL,
        [step_order] INT NOT NULL,
        [approver_type] NVARCHAR(30) NOT NULL,
        [approver_target] NVARCHAR(100) NULL,
        [created_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX [IX_approval_workflow_steps_workflow] ON [dbo].[approval_workflow_steps] ([approval_workflow_id]);
    PRINT 'Created approval_workflow_steps table';
END
GO

-- 4. Approval step instances table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[approval_step_instances]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[approval_step_instances] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [form_request_id] BIGINT NOT NULL,
        [step_order] INT NOT NULL,
        [approver_type] NVARCHAR(30) NOT NULL,
        [approver_target] NVARCHAR(100) NULL,
        [assigned_user_id] BIGINT NULL,
        [status] NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        [approved_at] DATETIME NULL,
        [rejected_at] DATETIME NULL,
        [comment] NVARCHAR(1000) NULL,
        [created_at] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX [IX_approval_step_instances_request] ON [dbo].[approval_step_instances] ([form_request_id]);
    CREATE INDEX [IX_approval_step_instances_assigned] ON [dbo].[approval_step_instances] ([assigned_user_id], [status]);
    PRINT 'Created approval_step_instances table';
END
GO

-- 5. Form template dependencies table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[form_template_dependencies]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[form_template_dependencies] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [form_template_id] BIGINT NOT NULL,
        [required_template_id] BIGINT NOT NULL,
        [required_status] NVARCHAR(20) NOT NULL DEFAULT 'Approved',
        [created_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX [IX_form_template_dependencies_template] ON [dbo].[form_template_dependencies] ([form_template_id]);
    PRINT 'Created form_template_dependencies table';
END
GO

-- 6. Form request dependencies table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[form_request_dependencies]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[form_request_dependencies] (
        [id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [form_request_id] BIGINT NOT NULL,
        [required_request_id] BIGINT NOT NULL,
        [created_at] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX [IX_form_request_dependencies_request] ON [dbo].[form_request_dependencies] ([form_request_id]);
    CREATE INDEX [IX_form_request_dependencies_required] ON [dbo].[form_request_dependencies] ([required_request_id]);
    PRINT 'Created form_request_dependencies table';
END
GO

PRINT 'SheetFlow migration complete!';
GO
