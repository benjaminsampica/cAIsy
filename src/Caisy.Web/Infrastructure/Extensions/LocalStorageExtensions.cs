using System.Runtime.InteropServices;

namespace Caisy.Web.Infrastructure.Extensions;

public static class LocalStorageExtensions
{
    /// <summary>
    ///     Local storage operates only in key-value pairs. In order to correlate data together as if we have a table, we append a table identifer to each key in order to simulate rows of a particular table.
    ///     This method returns the table identifier which is based on the table name.
    /// </summary>
    public static long GetTableId<T>() where T : class
    {
        var inputBytes = MemoryMarshal.AsBytes(typeof(T).Name.AsSpan());
        var tableId = BitConverter.ToInt32(inputBytes);

        return tableId;
    }
}
