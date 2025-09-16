namespace LazyRegion.Core;

public interface ILazyRegion
{
    void Set(object content, object dataContext = null);
}
