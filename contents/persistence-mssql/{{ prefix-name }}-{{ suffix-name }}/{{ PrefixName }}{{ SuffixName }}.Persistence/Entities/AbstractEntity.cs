using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;

public class AbstractEntity <TEntityId> : AbstractModified
{
    [Key]
    [Column("id")]
    [Required]
    public TEntityId Id { get; set; } = default!;
    
}