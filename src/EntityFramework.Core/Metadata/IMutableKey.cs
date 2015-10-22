// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IMutableKey : IMutableAnnotatable, IKey
    {
        new IReadOnlyList<IMutableProperty> Properties { get; }
        new IMutableEntityType DeclaringEntityType { get; }
    }
}
