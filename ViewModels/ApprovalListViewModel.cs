using SheetFlow.Models;

namespace SheetFlow.ViewModels;

public class ApproveViewModel
{
    public long Id { get; set; }
    public string? Comment { get; set; }
}

public class RejectViewModel
{
    public long Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "請填寫退回原因")]
    public string Comment { get; set; } = string.Empty;
}
