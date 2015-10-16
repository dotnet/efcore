// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Infrastructure
{
    public interface IKeyValue
    {
        IKey Key { get; }
        object Value { get; }
    }
}
