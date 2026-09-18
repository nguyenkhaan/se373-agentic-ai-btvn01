using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace IssueTriage.src.Contracts;

public class MinimalTriageRequest
{
    [Required(ErrorMessage = "Issue is required")]
    [StringLength(10_000 , MinimumLength = 1)]
     public string Issue { get; set; } = string.Empty;
}
