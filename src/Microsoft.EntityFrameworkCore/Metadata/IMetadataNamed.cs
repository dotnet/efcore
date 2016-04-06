// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IMetadataNamed
    {
        /// <summary>
        ///     Gets the name of the metadata element.
        /// </summary>
        string Name { get; }
    }
}
