// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    [DebuggerDisplay("{EntityType.Name,nq}.{Name,nq}")]
    public abstract class PropertyBase : Annotatable, IPropertyBase
    {
        protected PropertyBase([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Name = name;
        }

        public virtual string Name { get; }

        // TODO: Consider properties that are part of some complex/value type
        // Issue #246
        public abstract EntityType EntityType { get; }

        IEntityType IPropertyBase.EntityType => EntityType;
    }
}
