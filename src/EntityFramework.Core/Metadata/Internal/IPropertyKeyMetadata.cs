// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public interface IPropertyKeyMetadata
    {
        IKey PrimaryKey { get; [param: CanBeNull] set; }
        IReadOnlyList<IKey> Keys { get; [param: CanBeNull] set; }
        IReadOnlyList<IForeignKey> ForeignKeys { get; [param: CanBeNull] set; }
    }
}
