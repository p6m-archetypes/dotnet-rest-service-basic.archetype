namespace {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("{{ prefix_name }}")]
public class {{ PrefixName }}Entity : AbstractEntity<Guid>
{
    [Column("name")]
    [Required]
    public string? Name { get; set; }
}