using Microsoft.EntityFrameworkCore;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Context;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;
using Microsoft.Extensions.Logging;

namespace {{ PrefixName }}{{ SuffixName }}.Persistence.Repositories;

public class {{ PrefixName }}Repository : BaseRepository<{{ PrefixName }}Entity, Guid>, I{{ PrefixName }}Repository
{
    public {{ PrefixName }}Repository(AppDbContext context, ILogger<{{ PrefixName }}Repository> logger) 
        : base(context, logger)
    {
    }
}