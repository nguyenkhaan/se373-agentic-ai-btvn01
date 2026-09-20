using System.ComponentModel.DataAnnotations;
using IssueTriage.src.Contracts;
using IssueTriage.src.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace IssueTriage.src.Pages;

public class IndexModel(FunctionCallingService functionCallingService) : PageModel
{
    [BindProperty, Required, StringLength(10_000)]
    public string Issue { get; set; } = "Nút thanh toán trả HTTP 500 với mọi thẻ Visa từ 14:30. Hãy triage issue và cho biết team nào cần xử lý.";
    public TriageResult? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return;
        try
        {
            Result = await functionCallingService.TriageIssue(Issue.Trim(), cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Triage issue: {ex}");
            ErrorMessage = "Triage không hoàn tất. Kiểm tra API URL, model và quyền truy cập.";
        }
    }
}
