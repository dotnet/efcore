// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class CoreOptionsExtension : IDbContextOptionsExtension
    {
        public virtual IModel Model { get; [param: CanBeNull] set; }

        public virtual void ApplyServices(EntityFrameworkServicesBuilder builder)
        {
        }
    }
}
