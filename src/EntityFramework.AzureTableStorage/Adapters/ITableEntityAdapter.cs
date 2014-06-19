using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Adapters
{
    public interface ITableEntityAdapter<out T> : ITableEntity
    {
        T Entity { get; }
    }
}