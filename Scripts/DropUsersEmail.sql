USE [SheetFlow];
GO
DECLARE @ConstraintName NVARCHAR(255);
SELECT @ConstraintName = name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('users') AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('users') AND name = 'email');
IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE [users] DROP CONSTRAINT [' + @ConstraintName + ']');
    PRINT 'Dropped default constraint on email';
END
GO
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND name = 'email')
BEGIN
    ALTER TABLE [dbo].[users] DROP COLUMN [email];
    PRINT 'Dropped email from users';
END
GO
PRINT 'Done';
GO
