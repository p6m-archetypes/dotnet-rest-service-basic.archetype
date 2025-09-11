namespace {{ PrefixName }}{{ SuffixName }}.Persistence.Models;

public class Page<T>
{
    public List<T> Items { get; set; } = new();
    public long TotalElements { get; set; }
}