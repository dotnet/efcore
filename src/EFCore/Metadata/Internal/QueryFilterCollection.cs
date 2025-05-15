// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;
internal class QueryFilterCollection : IReadOnlyCollection<IQueryFilter>
{
    private readonly string anonymousFilterKey = "_____ANONYMOUS_FILTER_____";
    private readonly Dictionary<string, IQueryFilter> filters = new();

    public int Count => filters.Count;

    public IEnumerator<IQueryFilter> GetEnumerator() => filters.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IQueryFilter? this[string? filterKey]
    {
        get
        {
            filterKey ??= anonymousFilterKey;
            return filters.GetValueOrDefault(filterKey);
        }
    }

    public IQueryFilter? Set(IQueryFilter? filter)
    {        
        if (filter == null)
        {
            filters.Remove(anonymousFilterKey);
        }
        else if (filter.Expression == null)
        {
            filters.Remove(filter.Key ?? anonymousFilterKey);
        }
        else if (filter.IsAnonymous)
        {
            if(filters.Count > 0 && !filters.ContainsKey(anonymousFilterKey))
            {
                throw new InvalidOperationException(CoreStrings.AnonymousAndNamedFiltersCombined);
            }
            filters[anonymousFilterKey] = filter;
        }
        else
        {
            if (filters.Count > 0 && filters.ContainsKey(anonymousFilterKey))
            {
                throw new InvalidOperationException(CoreStrings.AnonymousAndNamedFiltersCombined);
            }
            filters[filter.Key!] = filter;
        }

        return filter;
    }

    public bool Remove(string key) => filters.Remove(key);
}
