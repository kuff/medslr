using System.IO;

public interface IDataProvider
{
    public ref readonly MemoryStream GetDataStream();
}
