using System.ComponentModel.DataAnnotations;

namespace SheetFlow.ViewModels;

public class ExportViewModel
{
    [Display(Name = "選擇表單")]
    public long? FormTemplateId { get; set; }

    [Display(Name = "開始日期")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [Display(Name = "結束日期")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Display(Name = "狀態")]
    public string? Status { get; set; }

    public List<SheetFlow.Models.FormTemplate> Templates { get; set; } = new();
}
