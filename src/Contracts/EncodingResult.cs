namespace IssueTriage.src.Contracts;

public class EncodingResult
{
    public string Name { get; set; } = string.Empty;
    public int VietnameseToken { get; set; }
    public int EnglishToken { get; set; }
    public double Ratio { get; set;}
}
