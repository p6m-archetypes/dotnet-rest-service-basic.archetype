using System.ComponentModel.DataAnnotations;

namespace {{ PrefixName }}{{ SuffixName }}.API.Dtos;

public class Get{{ PrefixName }}sRequest
{
    [Range(1, int.MaxValue)]
    public int StartPage { get; set; } = 1;
    
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}