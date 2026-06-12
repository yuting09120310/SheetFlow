-- SheetFlow 固定資產驗收單 - 加回簽名欄位 & 調整簽核流程
-- 流程：驗收人(填表人)簽名 → 驗收單位主管簽核 → 請購單位主管簽核
-- Step 2 使用 LinkedRequestApplicantDepartmentManager 從綁定的請購單取得請購部門主管

DECLARE @templateId BIGINT = (SELECT [id] FROM [dbo].[form_templates] WHERE [name] = N'固定資產驗收單');

-- =====================================================
-- 1. 加回三個簽名欄位（Signature 類型）
-- =====================================================
IF NOT EXISTS (SELECT * FROM [dbo].[form_template_fields] WHERE [form_template_id] = @templateId AND [field_key] = N'acceptor')
BEGIN
    INSERT INTO [dbo].[form_template_fields] ([form_template_id], [field_name], [field_key], [field_type], [is_required], [sort_order], [options_json], [default_value], [is_visible], [created_at], [updated_at])
    VALUES
        (@templateId, N'驗收人', 'acceptor', 'Signature', 1, 20, NULL, NULL, 1, GETUTCDATE(), GETUTCDATE()),
        (@templateId, N'驗收單位主管', 'acceptance_unit_manager', 'Signature', 1, 21, NULL, NULL, 1, GETUTCDATE(), GETUTCDATE()),
        (@templateId, N'請購單位主管', 'request_unit_manager', 'Signature', 1, 22, NULL, NULL, 1, GETUTCDATE(), GETUTCDATE());

    PRINT N'已加回 3 個簽名欄位（驗收人、驗收單位主管、請購單位主管）';
END
GO

-- =====================================================
-- 2. 刪除舊簽核流程，重新建立
-- =====================================================
DECLARE @workflowId BIGINT = (SELECT [id] FROM [dbo].[approval_workflows] WHERE [form_template_id] = (SELECT [id] FROM [dbo].[form_templates] WHERE [name] = N'固定資產驗收單') AND [name] = N'固定資產驗收單簽核流程');

IF @workflowId IS NOT NULL
BEGIN
    DELETE FROM [dbo].[approval_workflow_steps] WHERE [approval_workflow_id] = @workflowId;
    DELETE FROM [dbo].[approval_workflows] WHERE [id] = @workflowId;
    PRINT N'已刪除舊簽核流程';
END

DECLARE @templateId BIGINT = (SELECT [id] FROM [dbo].[form_templates] WHERE [name] = N'固定資產驗收單');
DECLARE @now DATETIME = GETUTCDATE();

INSERT INTO [dbo].[approval_workflows] ([form_template_id], [department], [name], [is_active], [created_at], [updated_at])
VALUES (@templateId, NULL, N'固定資產驗收單簽核流程', 1, @now, @now);

SET @workflowId = SCOPE_IDENTITY();

-- Step 1: 驗收單位主管（填表人的部門主管）
INSERT INTO [dbo].[approval_workflow_steps] ([approval_workflow_id], [step_order], [approver_type], [approver_target], [created_at])
VALUES (@workflowId, 1, N'ApplicantDepartmentManager', NULL, @now);

-- Step 2: 請購單位主管（從綁定的請購單取得請購人的部門主管）
INSERT INTO [dbo].[approval_workflow_steps] ([approval_workflow_id], [step_order], [approver_type], [approver_target], [created_at])
VALUES (@workflowId, 2, N'LinkedRequestApplicantDepartmentManager', NULL, @now);

PRINT N'簽核流程已建立：Step1 驗收單位主管 → Step2 請購單位主管（取自請購單）';
GO

-- =====================================================
-- 3. 建立表單依賴：固定資產驗收單 必須綁定已核准的請購單
-- =====================================================
DECLARE @acceptanceTemplateId BIGINT = (SELECT [id] FROM [dbo].[form_templates] WHERE [name] = N'固定資產驗收單');
DECLARE @purchaseTemplateId BIGINT = (SELECT [id] FROM [dbo].[form_templates] WHERE [name] = N'請購單');

IF NOT EXISTS (SELECT * FROM [dbo].[form_template_dependencies] WHERE [form_template_id] = @acceptanceTemplateId AND [required_template_id] = @purchaseTemplateId)
BEGIN
    INSERT INTO [dbo].[form_template_dependencies] ([form_template_id], [required_template_id], [required_status], [created_at])
    VALUES (@acceptanceTemplateId, @purchaseTemplateId, N'Approved', GETUTCDATE());
    PRINT N'已建立依賴：固定資產驗收單 必須綁定已核准的請購單';
END
ELSE
BEGIN
    PRINT N'依賴已存在，略過';
END
GO

PRINT N'固定資產驗收單設定完成！';
GO
