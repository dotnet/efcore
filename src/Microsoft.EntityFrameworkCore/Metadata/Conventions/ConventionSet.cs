// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Base implementation for a set of conventions used to build a model. This base implementation is an empty set of conventions.
    /// </summary>
    public class ConventionSet
    {
        /// <summary>
        ///     Conventions to run when an entity type is added to the model.
        /// </summary>
        public virtual IList<IEntityTypeConvention> EntityTypeAddedConventions { get; } = new List<IEntityTypeConvention>();

        /// <summary>
        ///     Conventions to run when an entity type is ignored.
        /// </summary>
        public virtual IList<IEntityTypeIgnoredConvention> EntityTypeIgnoredConventions { get; } = new List<IEntityTypeIgnoredConvention>();

        /// <summary>
        ///     Conventions to run when a property is ignored.
        /// </summary>
        public virtual IList<IEntityTypeMemberIgnoredConvention> EntityTypeMemberIgnoredConventions { get; } = new List<IEntityTypeMemberIgnoredConvention>();

        /// <summary>
        ///     Conventions to run when a base entity type is configured for an inheritance hierarchy.
        /// </summary>
        public virtual IList<IBaseTypeConvention> BaseEntityTypeSetConventions { get; } = new List<IBaseTypeConvention>();

        /// <summary>
        ///     Conventions to run when a foreign key is added.
        /// </summary>
        public virtual IList<IForeignKeyConvention> ForeignKeyAddedConventions { get; } = new List<IForeignKeyConvention>();

        /// <summary>
        ///     Conventions to run when a foreign key is removed.
        /// </summary>
        public virtual IList<IForeignKeyRemovedConvention> ForeignKeyRemovedConventions { get; } = new List<IForeignKeyRemovedConvention>();

        /// <summary>
        ///     Conventions to run when a key is added.
        /// </summary>
        public virtual IList<IKeyConvention> KeyAddedConventions { get; } = new List<IKeyConvention>();

        /// <summary>
        ///     Conventions to run when a key is removed.
        /// </summary>
        public virtual IList<IKeyRemovedConvention> KeyRemovedConventions { get; } = new List<IKeyRemovedConvention>();

        /// <summary>
        ///     Conventions to run when a primary key is configured.
        /// </summary>
        public virtual IList<IPrimaryKeyConvention> PrimaryKeySetConventions { get; } = new List<IPrimaryKeyConvention>();

        /// <summary>
        ///     Conventions to run when an index is added.
        /// </summary>
        public virtual IList<IIndexConvention> IndexAddedConventions { get; } = new List<IIndexConvention>();

        /// <summary>
        ///     Conventions to run when an index is added.
        /// </summary>
        public virtual IList<IIndexRemovedConvention> IndexRemovedConventions { get; } = new List<IIndexRemovedConvention>();

        /// <summary>
        ///     Conventions to run when the uniqueness of an index is changed.
        /// </summary>
        public virtual IList<IIndexUniquenessConvention> IndexUniquenessConventions { get; } = new List<IIndexUniquenessConvention>();

        /// <summary>
        ///     Conventions to run when the principal end of a relationship is configured.
        /// </summary>
        public virtual IList<IPrincipalEndConvention> PrincipalEndSetConventions { get; } = new List<IPrincipalEndConvention>();

        /// <summary>
        ///     Conventions to run when model building is completed.
        /// </summary>
        public virtual IList<IModelConvention> ModelBuiltConventions { get; } = new List<IModelConvention>();

        /// <summary>
        ///     Conventions to run to setup the initial model.
        /// </summary>
        public virtual IList<IModelConvention> ModelInitializedConventions { get; } = new List<IModelConvention>();

        /// <summary>
        ///     Conventions to run when a navigation property is added.
        /// </summary>
        public virtual IList<INavigationConvention> NavigationAddedConventions { get; } = new List<INavigationConvention>();

        /// <summary>
        ///     Conventions to run when a navigation property is removed.
        /// </summary>
        public virtual IList<INavigationRemovedConvention> NavigationRemovedConventions { get; } = new List<INavigationRemovedConvention>();

        /// <summary>
        ///     Conventions to run when the uniqueness of a foreign key is changed.
        /// </summary>
        public virtual IList<IForeignKeyUniquenessConvention> ForeignKeyUniquenessConventions { get; } = new List<IForeignKeyUniquenessConvention>();

        /// <summary>
        ///     Conventions to run when a property is added.
        /// </summary>
        public virtual IList<IPropertyConvention> PropertyAddedConventions { get; } = new List<IPropertyConvention>();

        /// <summary>
        ///     Conventions to run when the nullability of a property is changed.
        /// </summary>
        public virtual IList<IPropertyNullableConvention> PropertyNullableChangedConventions { get; } = new List<IPropertyNullableConvention>();

        /// <summary>
        ///     Conventions to run when the field of a property is changed.
        /// </summary>
        public virtual IList<IPropertyFieldChangedConvention> PropertyFieldChangedConventions { get; } =
            new List<IPropertyFieldChangedConvention>();
    }
}
