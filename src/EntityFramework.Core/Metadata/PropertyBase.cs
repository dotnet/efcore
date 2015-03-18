// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    [DebuggerDisplay("{EntityType.FullName,nq}.{Name,nq}")]
    public abstract class PropertyBase : Annotatable, IPropertyBase
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected PropertyBase()
        {
        }

        protected PropertyBase([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Name = name;
        }

        public virtual string Name { get; }

        // TODO: Consider properties that are part of some complex/value type
        // Issue #246
        public abstract EntityType EntityType { get; }

        IEntityType IPropertyBase.EntityType
        {
            get { return EntityType; }
        }
    }
}
