// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IModel : IMetadata
    {
        IEntityType Entity([NotNull] object instance);
        IEntityType Entity([NotNull] Type type);
        IEnumerable<IEntityType> Entities { get; }
    }
}
