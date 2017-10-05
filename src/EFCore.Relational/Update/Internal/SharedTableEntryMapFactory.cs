namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public delegate SharedTableEntryMap<TValue> SharedTableEntryMapFactory<TValue>(SharedTableEntryValueFactory<TValue> valueFactory);
}
