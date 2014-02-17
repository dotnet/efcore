// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public sealed class LazyMetadataRef<T>
        where T : class, new()
    {
        private T _value;

        public T Value
        {
            get
            {
                if (_value == null)
                {
                    var value = new T();

                    Interlocked.CompareExchange(ref _value, value, null);
                }

                return _value;
            }
        }
    }
}
