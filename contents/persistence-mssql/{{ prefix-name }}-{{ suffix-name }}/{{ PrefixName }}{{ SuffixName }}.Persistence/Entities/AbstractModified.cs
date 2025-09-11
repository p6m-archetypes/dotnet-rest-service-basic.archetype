using System.ComponentModel.DataAnnotations.Schema;

namespace {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;

public class AbstractModified : AbstractCreated
{
            
    [Column("modified")]
    public DateTime? Updated { get; set; }
}