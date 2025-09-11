using AutoBogus;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Entities;

namespace {{ PrefixName }}{{ SuffixName }}.UnitTests.TestBuilders;

public class {{ PrefixName }}EntityBuilder : AutoFaker<{{ PrefixName }}Entity>
{
    public {{ PrefixName }}EntityBuilder()
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Name, f => f.Company.CompanyName());
        RuleFor(x => x.Created, f => f.Date.Past());
        RuleFor(x => x.Updated, f => f.Date.Recent());
    }

    public {{ PrefixName }}EntityBuilder WithName(string name)
    {
        RuleFor(x => x.Name, name);
        return this;
    }

    public {{ PrefixName }}EntityBuilder WithId(Guid id)
    {
        RuleFor(x => x.Id, id);
        return this;
    }
}