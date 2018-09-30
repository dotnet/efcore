// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        public virtual IList<IEntityTypeAddedConvention> EntityTypeAddedConventions { get; } = new List<IEntityTypeAddedConvention>();

        /// <summary>
        ///     Conventions to run when an entity type is ignored.
        /// </summary>
        public virtual IList<IEntityTypeIgnoredConvention> EntityTypeIgnoredConventions { get; } = new List<IEntityTypeIgnoredConvention>();

        /// <summary>
        ///     Conventions to run when an entity type is removed.
        /// </summary>
        public virtual IList<IEntityTypeRemovedConvention> EntityTypeRemovedConventions { get; } = new List<IEntityTypeRemovedConvention>();

        /// <summary>
        ///     Conventions to run when a property is ignored.
        /// </summary>
        public virtual IList<IEntityTypeMemberIgnoredConvention> EntityTypeMemberIgnoredConventions { get; } = new List<IEntityTypeMemberIgnoredConvention>();

        /// <summary>
        ///     Conventions to run when the base entity type is changed.
        /// </summary>
        public virtual IList<IBaseTypeChangedConvention> BaseEntityTypeChangedConventions { get; } = new List<IBaseTypeChangedConvention>();

        /// <summary>
        ///     Conventions to run when an annotation is set or removed on an entity type.
        /// </summary>
        public virtual IList<IEntityTypeAnnotationChangedConvention> EntityTypeAnnotationChangedConventions { get; }
            = new List<IEntityTypeAnnotationChangedConvention>();

        /// <summary>
        ///     Conventions to run when an annotation is set or removed on a model.
        /// </summary>
        public virtual IList<IModelAnnotationChangedConvention> ModelAnnotationChangedConventions { get; }
            = new List<IModelAnnotationChangedConvention>();

        /// <summary>
        ///     Conventions to run when a foreign key is added.
        /// </summary>
        public virtual IList<IForeignKeyAddedConvention> ForeignKeyAddedConventions { get; } = new List<IForeignKeyAddedConvention>();

        /// <summary>
        ///     Conventions to run when a foreign key is removed.
        /// </summary>
        public virtual IList<IForeignKeyRemovedConvention> ForeignKeyRemovedConventions { get; } = new List<IForeignKeyRemovedConvention>();

        /// <summary>
        ///     Conventions to run when a key is added.
        /// </summary>
        public virtual IList<IKeyAddedConvention> KeyAddedConventions { get; } = new List<IKeyAddedConvention>();

        /// <summary>
        ///     Conventions to run when a key is removed.
        /// </summary>
        public virtual IList<IKeyRemovedConvention> KeyRemovedConventions { get; } = new List<IKeyRemovedConvention>();

        /// <summary>
        ///     Conventions to run when a primary key is changed.
        /// </summary>
        public virtual IList<IPrimaryKeyChangedConvention> PrimaryKeyChangedConventions { get; } = new List<IPrimaryKeyChangedConvention>();

        /// <summary>
        ///     Conventions to run when an index is added.
        /// </summary>
        public virtual IList<IIndexAddedConvention> IndexAddedConventions { get; } = new List<IIndexAddedConvention>();

        /// <summary>
        ///     Conventions to run when an index is removed.
        /// </summary>
        public virtual IList<IIndexRemovedConvention> IndexRemovedConventions { get; } = new List<IIndexRemovedConvention>();

        /// <summary>
        ///     Conventions to run when the uniqueness of an index is changed.
        /// </summary>
        public virtual IList<IIndexUniquenessChangedConvention> IndexUniquenessChangedConventions { get; } = new List<IIndexUniquenessChangedConvention>();

        /// <summary>
        ///     Conventions to run when an annotation is changed on an index.
        /// </summary>
        public virtual IList<IIndexAnnotationChangedConvention> IndexAnnotationChangedConventions { get; } = new List<IIndexAnnotationChangedConvention>();

        /// <summary>
        ///     Conventions to run when the principal end of a relationship is configured.
        /// </summary>
        public virtual IList<IPrincipalEndChangedConvention> PrincipalEndChangedConventions { get; } = new List<IPrincipalEndChangedConvention>();

        /// <summary>
        ///     Conventions to run when model building is completed.
        /// </summary>
        public virtual IList<IModelBuiltConvention> ModelBuiltConventions { get; } = new List<IModelBuiltConvention>();

        /// <summary>
        ///     Conventions to run to setup the initial model.
        /// </summary>
        public virtual IList<IModelInitializedConvention> ModelInitializedConventions { get; } = new List<IModelInitializedConvention>();

        /// <summary>
        ///     Conventions to run when a navigation property is added.
        /// </summary>
        public virtual IList<INavigationAddedConvention> NavigationAddedConventions { get; } = new List<INavigationAddedConvention>();

        /// <summary>
        ///     Conventions to run when a navigation property is removed.
        /// </summary>
        public virtual IList<INavigationRemovedConvention> NavigationRemovedConventions { get; } = new List<INavigationRemovedConvention>();

        /// <summary>
        ///     Conventions to run when the uniqueness of a foreign key is changed.
        /// </summary>
        public virtual IList<IForeignKeyUniquenessChangedConvention> ForeignKeyUniquenessChangedConventions { get; } = new List<IForeignKeyUniquenessChangedConvention>();

        /// <summary>
        ///     Conventions to run when the uniqueness of a foreign key is changed.
        /// </summary>
        public virtual IList<IForeignKeyRequirednessChangedConvention> ForeignKeyRequirednessChangedConventions { get; } = new List<IForeignKeyRequirednessChangedConvention>();

        /// <summary>
        ///     Conventions to run when the ownership of a foreign key is changed.
        /// </summary>
        public virtual IList<IForeignKeyOwnershipChangedConvention> ForeignKeyOwnershipChangedConventions { get; } = new List<IForeignKeyOwnershipChangedConvention>();

        /// <summary>
        ///     Conventions to run when a property is added.
        /// </summary>
        public virtual IList<IPropertyAddedConvention> PropertyAddedConventions { get; } = new List<IPropertyAddedConvention>();

        /// <summary>
        ///     Conventions to run when the nullability of a property is changed.
        /// </summary>
        public virtual IList<IPropertyNullabilityChangedConvention> PropertyNullabilityChangedConventions { get; } = new List<IPropertyNullabilityChangedConvention>();

        /// <summary>
        ///     Conventions to run when the field of a property is changed.
        /// </summary>
        public virtual IList<IPropertyFieldChangedConvention> PropertyFieldChangedConventions { get; } =
            new List<IPropertyFieldChangedConvention>();

        /// <summary>
        ///     Conventions to run when an annotation is changed on a property.
        /// </summary>
        public virtual IList<IPropertyAnnotationChangedConvention> PropertyAnnotationChangedConventions { get; } = new List<IPropertyAnnotationChangedConvention>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ConventionSet CreateConventionSet([NotNull] DbContext context)
        {
            var conventionSet = new CompositeConventionSetBuilder(
                    context.GetService<IEnumerable<IConventionSetBuilder>>().ToList())
                .AddConventions(
                    context.GetService<ICoreConventionSetBuilder>().CreateConventionSet());

            conventionSet.ModelBuiltConventions.Add(new ValidatingConvention(context.GetService<IModelValidator>()));

            return conventionSet;
        }
    }
}
