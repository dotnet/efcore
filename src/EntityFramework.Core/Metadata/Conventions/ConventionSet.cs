// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;

namespace Microsoft.Data.Entity.Metadata.Conventions
{
    public class ConventionSet
    {
        public virtual IList<IEntityTypeConvention> EntityTypeAddedConventions { get; } = new List<IEntityTypeConvention>();
        public virtual IList<IBaseTypeConvention> BaseEntityTypeSetConventions { get; } = new List<IBaseTypeConvention>();
        public virtual IList<IForeignKeyConvention> ForeignKeyAddedConventions { get; } = new List<IForeignKeyConvention>();
        public virtual IList<IForeignKeyRemovedConvention> ForeignKeyRemovedConventions { get; } = new List<IForeignKeyRemovedConvention>();
        public virtual IList<IKeyConvention> KeyAddedConventions { get; } = new List<IKeyConvention>();
        public virtual IList<IModelConvention> ModelBuiltConventions { get; } = new List<IModelConvention>();
        public virtual IList<IModelConvention> ModelInitializedConventions { get; } = new List<IModelConvention>();
        public virtual IList<INavigationConvention> NavigationAddedConventions { get; } = new List<INavigationConvention>();
        public virtual IList<IPropertyConvention> PropertyAddedConventions { get; } = new List<IPropertyConvention>();
    }
}
