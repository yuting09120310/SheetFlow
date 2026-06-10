using ClosedXML.Excel;
using SheetFlow.Models;

namespace SheetFlow.Services;

public class ExcelTemplateParser
{
    public List<ParsedField> Parse(Stream excelStream)
    {
        var fields = new List<ParsedField>();

        using var workbook = new XLWorkbook(excelStream);
        var sheet = workbook.Worksheet(1);
        var firstRow = sheet.Row(1);

        int colCount = firstRow.CellsUsed().Count();
        for (int col = 1; col <= colCount; col++)
        {
            var cell = firstRow.Cell(col);
            var header = cell.GetString()?.Trim();

            if (string.IsNullOrEmpty(header))
                continue;

            var field = new ParsedField
            {
                FieldName = header,
                FieldKey = GenerateFieldKey(header),
                FieldType = InferFieldType(header),
                IsRequired = true,
                SortOrder = col
            };
            fields.Add(field);
        }

        return fields;
    }

    private string GenerateFieldKey(string name)
    {
        var key = name.ToLower()
            .Replace(" ", "_")
            .Replace("-", "_")
            .Replace(".", "_");
        return "field_" + key;
    }

    private string InferFieldType(string header)
    {
        var h = header.ToLower();
        if (h.Contains("日期") || h.Contains("date") || h.Contains("時間") || h == "date")
            return "Date";
        if (h.Contains("數量") || h.Contains("單價") || h.Contains("金額") || h.Contains("價格") ||
            h.Contains("number") || h.Contains("數量") || h.Contains("price") || h.Contains("amount"))
            return "Number";
        if (h.Contains("說明") || h.Contains("備註") || h.Contains("描述") || h.Contains("textarea") ||
            h.Contains("description") || h.Contains("remark") || h.Contains("note"))
            return "Textarea";
        if (h.Contains("類別") || h.Contains("類型") || h.Contains("分類") || h.Contains("select") ||
            h.Contains("category") || h.Contains("type"))
            return "Select";
        return "Text";
    }

    public class ParsedField
    {
        public string FieldName { get; set; } = string.Empty;
        public string FieldKey { get; set; } = string.Empty;
        public string FieldType { get; set; } = "Text";
        public bool IsRequired { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
