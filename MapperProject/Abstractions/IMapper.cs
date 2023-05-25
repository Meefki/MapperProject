using MapperProject.Models;

namespace MapperProject.Abstractions;

public interface IMapper
{
    void AddConfiguration<TDest, TSource>(Action<Configuration<TDest, TSource>> configurationAction)
        where TDest : class
        where TSource : class;

    void AddConfiguration<TDest, TSource>(Configuration<TDest, TSource> configuration)
        where TDest : class
        where TSource : class;
    TDest Map<TDest, TSource>(TSource source, TDest? dest = null)
        where TDest : class
        where TSource : class;
}
