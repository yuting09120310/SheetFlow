-- SheetFlow 請購單 - 新增簽核簽名欄位
-- 請購單有 3 個簽核步驟，需要 3 個簽名欄位

DECLARE @templateId BIGINT = (SELECT [id] FROM [dbo].[form_templates] WHERE [name] = N'請購單');

-- 新增三個簽核簽名欄位
IF NOT EXISTS (SELECT * FROM [dbo].[form_template_fields] WHERE [form_template_id] = @templateId AND [field_key] = N'department_manager_signature')
BEGIN
    INSERT INTO [dbo].[form_template_fields] ([form_template_id], [field_name], [field_key], [field_type], [is_required], [sort_order], [options_json], [default_value], [is_visible], [created_at], [updated_at])
    VALUES
        (@templateId, N'部門主管簽名', 'department_manager_signature', 'Signature', 1, 15, NULL, NULL, 1, GETUTCDATE(), GETUTCDATE()),
        (@templateId, N'資訊部主管簽名', 'it_manager_signature', 'Signature', 1, 16, NULL, NULL, 1, GETUTCDATE(), GETUTCDATE()),
        (@templateId, N'會計部主管簽名', 'accounting_manager_signature', 'Signature', 1, 17, NULL, NULL, 1, GETUTCDATE(), GETUTCDATE());

    PRINT N'請購單已新增 3 個簽核簽名欄位';
END
ELSE
BEGIN
    PRINT N'簽核簽名欄位已存在，略過';
END
GO

PRINT N'請購單簽名欄位設定完成！';
GO
