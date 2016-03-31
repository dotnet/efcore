// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IMetadataElement
    {
        /// <summary>
        ///     Gets the entity type this metadata element is declaired on.
        /// </summary>
        IEntityType DeclaringEntityType { get; }
    }
}
