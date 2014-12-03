// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Infrastructure
{
    /// <summary>
    ///     This interface provides access to the underlying services of the <see cref="Database" /> facade API such
    ///     that extension methods written for this service can use the underlying services without these details
    ///     showing up in the API used by application developers.
    /// </summary>
    public interface IDatabaseInternals
    {
        DataStoreCreator DataStoreCreator { get; }
        ILogger Logger { get; }
        IModel Model { get; }
    }
}
