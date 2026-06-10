using ClosedXML.Excel;
using SheetFlow.Models;
using SheetFlow.Repositories;

namespace SheetFlow.Services;

public class ExcelExportService
{
    private readonly IFormRequestRepository _requestRepo;
    private readonly IFormTemplateRepository _templateRepo;

    public ExcelExportService(
        IFormRequestRepository requestRepo,
        IFormTemplateRepository templateRepo)
    {
        _requestRepo = requestRepo;
        _templateRepo = templateRepo;
    }

    public async Task<byte[]> ExportSingleAsync(long requestId)
    {
        var request = await _requestRepo.GetByIdAsync(requestId)
            ?? throw new Exception("申請單不存在");
        var values = (await _requestRepo.GetValuesAsync(requestId)).ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("申請明細");

        ws.Cell(1, 1).Value = "申請單號";
        ws.Cell(1, 2).Value = request.RequestNo;
        ws.Cell(2, 1).Value = "表單名稱";
        ws.Cell(2, 2).Value = request.FormName;
        ws.Cell(3, 1).Value = "申請人";
        ws.Cell(3, 2).Value = request.ApplicantName;
        ws.Cell(4, 1).Value = "申請時間";
        ws.Cell(4, 2).Value = request.SubmittedAt?.ToString("yyyy-MM-dd HH:mm");
        ws.Cell(5, 1).Value = "狀態";
        ws.Cell(5, 2).Value = request.Status;

        int row = 7;
        ws.Cell(row, 1).Value = "欄位名稱";
        ws.Cell(row, 2).Value = "填寫內容";
        row++;

        foreach (var v in values)
        {
            ws.Cell(row, 1).Value = v.FieldName;
            ws.Cell(row, 2).Value = v.FieldValue;
            row++;
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportMultipleAsync(long? templateId, DateTime? startDate, DateTime? endDate, string? status)
    {
        var requests = (await _requestRepo.SearchAsync(templateId, null, startDate, endDate, status)).ToList();

        List<FormTemplateField>? fields = null;
        if (templateId.HasValue)
        {
            fields = (await _templateRepo.GetVisibleFieldsAsync(templateId.Value)).ToList();
        }

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("申請列表");

        int col = 1;
        ws.Cell(1, col++).Value = "申請單號";
        ws.Cell(1, col++).Value = "表單名稱";
        ws.Cell(1, col++).Value = "申請人";
        ws.Cell(1, col++).Value = "申請時間";
        ws.Cell(1, col++).Value = "狀態";

        if (fields != null)
        {
            foreach (var f in fields)
                ws.Cell(1, col++).Value = f.FieldName;
        }

        int row = 2;
        foreach (var r in requests)
        {
            col = 1;
            ws.Cell(row, col++).Value = r.RequestNo;
            ws.Cell(row, col++).Value = r.FormName;
            ws.Cell(row, col++).Value = r.ApplicantName;
            ws.Cell(row, col++).Value = r.SubmittedAt?.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, col++).Value = r.Status;

            if (fields != null)
            {
                var values = (await _requestRepo.GetValuesAsync(r.Id)).ToList();
                foreach (var f in fields)
                {
                    var val = values.FirstOrDefault(v => v.FieldKey == f.FieldKey);
                    ws.Cell(row, col++).Value = val?.FieldValue ?? string.Empty;
                }
            }
            row++;
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
