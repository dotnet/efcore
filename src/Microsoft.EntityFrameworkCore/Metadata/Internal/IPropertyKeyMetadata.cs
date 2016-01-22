// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public interface IPropertyKeyMetadata
    {
        IKey PrimaryKey { get; }
        IReadOnlyList<IKey> Keys { get; }
        IReadOnlyList<IForeignKey> ForeignKeys { get; }
    }
}
