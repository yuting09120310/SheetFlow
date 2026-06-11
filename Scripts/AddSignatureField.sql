USE [SheetFlow];
GO

-- Add Signature field to Purchase Request (Template ID: 2)
IF NOT EXISTS (SELECT 1 FROM [dbo].[form_template_fields] WHERE [form_template_id] = 2 AND [field_key] = 'signature')
BEGIN
    INSERT INTO [dbo].[form_template_fields] ([form_template_id], [field_name], [field_key], [field_type], [is_required], [sort_order], [is_visible], [created_at], [updated_at])
    VALUES (2, N'人員簽名', 'signature', 'Signature', 1, 14, 1, GETUTCDATE(), GETUTCDATE());
    PRINT 'Added Signature field to Purchase Request template';
END
GO

PRINT 'Done';
GO
