namespace {{ PrefixName }}{{ SuffixName }}.API.Dtos;

public class Get{{ PrefixName }}sResponse
{
    public List<{{ PrefixName }}Dto> {{ PrefixName }}s { get; set; } = new();
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
    public int NextPage { get; set; }
    public int PreviousPage { get; set; }
    public int TotalPages { get; set; }
    public long TotalElements { get; set; }
}