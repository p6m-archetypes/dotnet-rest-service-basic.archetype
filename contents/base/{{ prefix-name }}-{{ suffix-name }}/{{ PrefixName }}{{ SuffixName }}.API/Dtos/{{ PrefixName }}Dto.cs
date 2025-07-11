using System.ComponentModel.DataAnnotations;

namespace {{ PrefixName }}{{ SuffixName }}.API.Dtos;

public class {{ PrefixName }}Dto
{
    public string? Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}