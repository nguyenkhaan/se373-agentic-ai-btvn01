using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using IssueTriage.src.Contracts;
using IssueTriage.src.Services;

namespace IssueTriage.src.Pages.Pages;

public class IndexModel(FunctionCallingService triageService) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Nhập mô tả issue trước khi chạy triage."), StringLength(10_000)]
    public string Issue { get; set; } = "Nút thanh toán trả HTTP 500 với mọi thẻ Visa từ 14:30. Hãy triage issue và cho biết team nào cần xử lý.";

    public TriageResult? Result { get; private set; }
    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
    }

    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return;

        try
        {
            Result = await triageService.TriageIssue(Issue.Trim(), cancellationToken);
        }
        catch (Exception)
        {
            ErrorMessage = "Triage không hoàn tất. Kiểm tra API URL, model và quyền truy cập.";
        }
    }
}
