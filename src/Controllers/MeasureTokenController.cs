using IssueTriage.src.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharpToken;
using System.Text;

namespace IssueTriage.src.Controllers;

[ApiController]
[Route("/api/measure-token")]
public class MeasureTokenController : ControllerBase
{
    private readonly string Vietnamese = """
        Cầu máng Segovia là một cầu máng dẫn nước của người La Mã tại
        Segovia, Tây Ban Nha. Công trình được xây dựng vào khoảng thế kỷ
        thứ nhất sau Công nguyên để dẫn nước từ các con suối trên núi
        cách đó mười bảy ki-lô-mét.
    """;
    private readonly string English = """
        The Aqueduct of Segovia is a Roman aqueduct in Segovia, Spain. 
        It was built around the first century AD to channel water from 
        springs in the mountains seventeen kilometres away.
    """;
    [HttpGet]
    public IActionResult MeasureToken()
    {
        var cl100k = GptEncoding.GetEncoding("cl100k_base");
        var o200k = GptEncoding.GetEncoding("o200k_base");

        List<EncodingResult> result = [];
        foreach (GptEncoding encoding in new[] { cl100k, o200k })
        {
            var vietnameseTokens = encoding.CountTokens(Vietnamese);
            var englishTokens = encoding.CountTokens(English);
            result.Add(new()
            {
                Name = (encoding == cl100k) ? "cl100k" : "o200k",
                VietnameseToken = vietnameseTokens,
                EnglishToken = englishTokens,
                Ratio = Math.Round((double)vietnameseTokens / englishTokens, 2)
            });
        }
        return Ok(result);
    }
}
