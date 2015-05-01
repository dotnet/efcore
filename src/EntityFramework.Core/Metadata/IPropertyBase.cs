// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IPropertyBase : IAnnotatable
    {
        string Name { get; }
        IEntityType EntityType { get; }
    }
}
