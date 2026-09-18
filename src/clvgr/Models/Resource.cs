namespace clvgr.Models;

public partial class Resource
{
    public string TagString => string.Join(", ", Tags?.Select(t => $"#{t}") ?? []);
}
