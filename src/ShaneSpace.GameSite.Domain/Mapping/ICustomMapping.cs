using AutoMapper;

namespace ShaneSpace.GameSite.Domain.Mapping
{
    public interface ICustomMapping<TSource, TDest>
    {
        IMappingExpression<TSource, TDest> CreateMappings(IConfiguration configuration);
    }
}
