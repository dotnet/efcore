// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Data.Entity.Query
{
    public interface IAsyncEnumerable<out T> : IAsyncEnumerable, IEnumerable<T> // TODO: Consider whether to keep IE<T> here
    {
        new IAsyncEnumerator<T> GetAsyncEnumerator();
    }
}
