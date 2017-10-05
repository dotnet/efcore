using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public delegate TValue SharedTableEntryValueFactory<out TValue>(string tableName, string schema, IComparer<IUpdateEntry> comparer);
}
