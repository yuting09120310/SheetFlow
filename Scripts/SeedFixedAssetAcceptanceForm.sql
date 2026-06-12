-- SheetFlow 表單樣板種子資料
-- 固定資產驗收單 (天仁茶業股份有限公司)

-- =====================================================
-- 固定資產驗收單
-- =====================================================
IF NOT EXISTS (SELECT * FROM [dbo].[form_templates] WHERE [name] = N'固定資產驗收單')
BEGIN
    DECLARE @templateId BIGINT;
    DECLARE @now DATETIME = GETUTCDATE();
    DECLARE @adminId BIGINT = (SELECT [id] FROM [dbo].[users] WHERE [username] = 'admin');

    INSERT INTO [dbo].[form_templates] ([name], [description], [excel_file_path], [is_active], [created_by], [created_at], [updated_at])
    VALUES (N'固定資產驗收單', N'天仁茶業股份有限公司固定資產驗收表單', NULL, 1, @adminId, @now, @now);

    SET @templateId = SCOPE_IDENTITY();

    INSERT INTO [dbo].[form_template_fields] ([form_template_id], [field_name], [field_key], [field_type], [is_required], [sort_order], [options_json], [default_value], [is_visible], [created_at], [updated_at])
    VALUES
        -- 表頭資訊
        (@templateId, N'驗收單位', 'acceptance_unit', 'Text', 1, 1, NULL, NULL, 1, @now, @now),
        (@templateId, N'請購單位', 'request_unit', 'Text', 1, 2, NULL, NULL, 1, @now, @now),
        (@templateId, N'請購日期', 'request_date', 'Date', 1, 3, NULL, NULL, 1, @now, @now),
        (@templateId, N'財產編號', 'asset_number', 'Text', 1, 4, NULL, NULL, 1, @now, @now),
        (@templateId, N'存放地點', 'storage_location', 'Text', 1, 5, NULL, NULL, 1, @now, @now),
        (@templateId, N'附件', 'attachment', 'Text', 0, 6, NULL, NULL, 1, @now, @now),

        -- 驗收記錄 - 項目（下拉選單）
        (@templateId, N'項目', 'item', 'Select', 1, 7, N'生財設備,裝修設備,運輸設備,雜項設備,雜項購置,電腦軟體成本,保養修繕,其他費用', NULL, 1, @now, @now),

        -- 驗收記錄 - 溫盤（下拉選單）
        (@templateId, N'溫盤', 'warm_disk', 'Select', 0, 8, N'溫室氣體盤查相關設備（企業服務部勾選）', NULL, 1, @now, @now),

        -- 驗收明細
        (@templateId, N'驗收品名、廠牌名稱、規格型式、附屬設備', 'item_description', 'Textarea', 1, 9, NULL, NULL, 1, @now, @now),
        (@templateId, N'數量', 'quantity', 'Number', 1, 10, NULL, N'1', 1, @now, @now),
        (@templateId, N'單位', 'unit', 'Text', 1, 11, NULL, NULL, 1, @now, @now),
        (@templateId, N'單價', 'unit_price', 'Number', 1, 12, NULL, N'0', 1, @now, @now),
        (@templateId, N'小計', 'subtotal', 'Number', 1, 13, NULL, N'0', 1, @now, @now),

        -- 驗收狀況與金額
        (@templateId, N'驗收狀況', 'acceptance_status', 'Select', 1, 14, N'合格,不合格', NULL, 1, @now, @now),
        (@templateId, N'說明', 'description', 'Textarea', 0, 15, NULL, NULL, 1, @now, @now),
        (@templateId, N'發票號碼', 'invoice_number', 'Text', 1, 16, NULL, NULL, 1, @now, @now),
        (@templateId, N'總金額', 'total_amount', 'Number', 1, 17, NULL, N'0', 1, @now, @now),
        (@templateId, N'保險單號', 'insurance_number', 'Text', 0, 18, NULL, NULL, 1, @now, @now),
        (@templateId, N'運輸設備車牌號碼', 'vehicle_plate_number', 'Text', 0, 19, NULL, NULL, 1, @now, @now),

        -- 簽核欄位
        (@templateId, N'請購單位主管', 'request_unit_manager', 'Signature', 1, 20, NULL, NULL, 1, @now, @now),
        (@templateId, N'驗收單位主管', 'acceptance_unit_manager', 'Signature', 1, 21, NULL, NULL, 1, @now, @now),
        (@templateId, N'驗收人', 'acceptor', 'Signature', 1, 22, NULL, NULL, 1, @now, @now),

        -- 會簽財務部門
        (@templateId, N'會計科目', 'accounting_subject', 'Text', 0, 23, NULL, NULL, 1, @now, @now),
        (@templateId, N'成本', 'cost', 'Number', 0, 24, NULL, N'0', 1, @now, @now),
        (@templateId, N'耐用年限', 'useful_life', 'Text', 0, 25, NULL, NULL, 1, @now, @now),
        (@templateId, N'殘值', 'residual_value', 'Number', 0, 26, NULL, N'0', 1, @now, @now);

    PRINT N'固定資產驗收單 已建立';
END
GO

PRINT N'固定資產驗收單種子資料匯入完成！';
GO
