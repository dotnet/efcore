// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Query
{
    public interface IAsyncEnumerator<out T> : IAsyncEnumerator
    {
        new T Current { get; }
    }
}
