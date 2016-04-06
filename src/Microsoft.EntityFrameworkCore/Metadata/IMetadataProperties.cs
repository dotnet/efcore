// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IMetadataProperties
    {
        /// <summary>
        /// Gets the properties that are associated with this metadata element. These properties may be declaired on a base type
        /// in an inheritance hierarchy.
        /// </summary>
        IReadOnlyList<IProperty> Properties { get; }
    }
}
