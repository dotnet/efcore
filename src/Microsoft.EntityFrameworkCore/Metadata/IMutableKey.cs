// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a primary or alternate key on an entity.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IKey" /> represents a ready-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutableKey : IMutableAnnotatable, IKey, IMutableMetadataElement, IMutableMetadataProperties
    {
    }
}
