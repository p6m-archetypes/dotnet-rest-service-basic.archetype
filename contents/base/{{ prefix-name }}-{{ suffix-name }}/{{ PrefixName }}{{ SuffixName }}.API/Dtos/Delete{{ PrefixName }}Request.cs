using System.ComponentModel.DataAnnotations;

namespace {{ PrefixName }}{{ SuffixName }}.API.Dtos;

public class Delete{{ PrefixName }}Request  
{
    [Required]
    public string Id { get; set; } = string.Empty;
}