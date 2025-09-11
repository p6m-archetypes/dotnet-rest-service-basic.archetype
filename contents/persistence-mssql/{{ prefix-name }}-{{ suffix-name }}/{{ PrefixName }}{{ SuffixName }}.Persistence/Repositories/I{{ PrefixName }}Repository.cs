using {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Models;

namespace {{ PrefixName }}{{ SuffixName }}.Persistence.Repositories;

public interface I{{ PrefixName }}Repository
{
    void Save({{ PrefixName }}Entity entity);
    Task<{{ PrefixName }}Entity?> FindByIdAsync(Guid id);
    Task<Page<{{ PrefixName }}Entity>> FindAsync(PageRequest request);
    void Update({{ PrefixName }}Entity entity);
    void Delete({{ PrefixName }}Entity entity);
    Task SaveChangesAsync();
}