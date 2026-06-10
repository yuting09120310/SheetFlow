-- SheetFlow 表單樣板種子資料
-- 需求申請單 & 請購單

-- =====================================================
-- 1. 需求申請單
-- =====================================================
IF NOT EXISTS (SELECT * FROM [dbo].[form_templates] WHERE [name] = N'需求申請單')
BEGIN
    DECLARE @templateId1 BIGINT;
    DECLARE @now DATETIME = GETUTCDATE();
    DECLARE @adminId BIGINT = (SELECT [id] FROM [dbo].[users] WHERE [username] = 'admin');

    INSERT INTO [dbo].[form_templates] ([name], [description], [excel_file_path], [is_active], [created_by], [created_at], [updated_at])
    VALUES (N'需求申請單', N'內部需求申請表單，適用於軟硬體、網路等資源需求申請', NULL, 1, @adminId, @now, @now);

    SET @templateId1 = SCOPE_IDENTITY();

    INSERT INTO [dbo].[form_template_fields] ([form_template_id], [field_name], [field_key], [field_type], [is_required], [sort_order], [options_json], [default_value], [is_visible], [created_at], [updated_at])
    VALUES
        (@templateId1, N'申請日期', 'apply_date', 'Date', 1, 1, NULL, NULL, 1, @now, @now),
        (@templateId1, N'申請部門', 'department', 'Select', 1, 2, N'資訊部,業務部,財務部,人事部,生產部,採購部,研發部,其他', NULL, 1, @now, @now),
        (@templateId1, N'申請人', 'applicant', 'Text', 1, 3, NULL, NULL, 1, @now, @now),
        (@templateId1, N'需求類別', 'category', 'Select', 1, 4, N'硬體設備,軟體授權,網路資源,辦公設備,人力需求,其他', NULL, 1, @now, @now),
        (@templateId1, N'需求名稱', 'requirement_name', 'Text', 1, 5, NULL, NULL, 1, @now, @now),
        (@templateId1, N'需求說明', 'description', 'Textarea', 1, 6, NULL, NULL, 1, @now, @now),
        (@templateId1, N'需求數量', 'quantity', 'Number', 1, 7, NULL, N'1', 1, @now, @now),
        (@templateId1, N'期望完成日', 'expected_date', 'Date', 0, 8, NULL, NULL, 1, @now, @now),
        (@templateId1, N'優先級', 'priority', 'Select', 1, 9, N'高,中,低', N'中', 1, @now, @now),
        (@templateId1, N'備註', 'remarks', 'Textarea', 0, 10, NULL, NULL, 1, @now, @now);

    PRINT N'需求申請單 已建立';
END
GO

-- =====================================================
-- 2. 請購單
-- =====================================================
IF NOT EXISTS (SELECT * FROM [dbo].[form_templates] WHERE [name] = N'請購單')
BEGIN
    DECLARE @templateId2 BIGINT;
    DECLARE @now DATETIME = GETUTCDATE();
    DECLARE @adminId BIGINT = (SELECT [id] FROM [dbo].[users] WHERE [username] = 'admin');

    INSERT INTO [dbo].[form_templates] ([name], [description], [excel_file_path], [is_active], [created_by], [created_at], [updated_at])
    VALUES (N'請購單', N'採購請購表單，用於提出物品或服務之採購申請', NULL, 1, @adminId, @now, @now);

    SET @templateId2 = SCOPE_IDENTITY();

    INSERT INTO [dbo].[form_template_fields] ([form_template_id], [field_name], [field_key], [field_type], [is_required], [sort_order], [options_json], [default_value], [is_visible], [created_at], [updated_at])
    VALUES
        (@templateId2, N'申請日期', 'apply_date', 'Date', 1, 1, NULL, NULL, 1, @now, @now),
        (@templateId2, N'申請部門', 'department', 'Select', 1, 2, N'資訊部,業務部,財務部,人事部,生產部,採購部,研發部,其他', NULL, 1, @now, @now),
        (@templateId2, N'申請人', 'applicant', 'Text', 1, 3, NULL, NULL, 1, @now, @now),
        (@templateId2, N'品項名稱', 'item_name', 'Text', 1, 4, NULL, NULL, 1, @now, @now),
        (@templateId2, N'規格說明', 'specification', 'Textarea', 0, 5, NULL, NULL, 1, @now, @now),
        (@templateId2, N'數量', 'quantity', 'Number', 1, 6, NULL, N'1', 1, @now, @now),
        (@templateId2, N'單位', 'unit', 'Select', 1, 7, N'個,箱,組,套,台,只,式,批,其他', N'個', 1, @now, @now),
        (@templateId2, N'預估單價', 'unit_price', 'Number', 1, 8, NULL, N'0', 1, @now, @now),
        (@templateId2, N'預估總價', 'total_price', 'Number', 1, 9, NULL, N'0', 1, @now, @now),
        (@templateId2, N'需求日期', 'required_date', 'Date', 1, 10, NULL, NULL, 1, @now, @now),
        (@templateId2, N'廠商名稱', 'supplier', 'Text', 0, 11, NULL, NULL, 1, @now, @now),
        (@templateId2, N'請購原因', 'reason', 'Textarea', 1, 12, NULL, NULL, 1, @now, @now),
        (@templateId2, N'備註', 'remarks', 'Textarea', 0, 13, NULL, NULL, 1, @now, @now);

    PRINT N'請購單 已建立';
END
GO

PRINT N'表單樣板種子資料匯入完成！';
GO
