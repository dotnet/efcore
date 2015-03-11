// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Internal
{
    public class GenericBoxedValueReader<TValue> : IBoxedValueReader
    {
        public virtual object ReadValue(IValueReader valueReader, int index)
            => valueReader.IsNull(index) ? null : (object)valueReader.ReadValue<TValue>(index);
    }
}
