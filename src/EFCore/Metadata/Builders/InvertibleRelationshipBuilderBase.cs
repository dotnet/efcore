// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Base class used for configuring an invertible relationship.
    /// </summary>
    public abstract class InvertibleRelationshipBuilderBase : IInfrastructure<InternalRelationshipBuilder>
    {
        private readonly IReadOnlyList<Property> _foreignKeyProperties;
        private readonly IReadOnlyList<Property> _principalKeyProperties;
        private readonly bool? _required;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected InvertibleRelationshipBuilderBase(
            [NotNull] IMutableEntityType declaringEntityType,
            [NotNull] IMutableEntityType relatedEntityType,
            [NotNull] IMutableForeignKey foreignKey)
            : this(((ForeignKey)foreignKey).Builder, null)
        {
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));
            Check.NotNull(relatedEntityType, nameof(relatedEntityType));
            Check.NotNull(foreignKey, nameof(foreignKey));

            DeclaringEntityType = declaringEntityType;
            RelatedEntityType = relatedEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected InvertibleRelationshipBuilderBase(
            InternalRelationshipBuilder builder,
            InvertibleRelationshipBuilderBase oldBuilder,
            bool inverted = false,
            bool foreignKeySet = false,
            bool principalKeySet = false,
            bool requiredSet = false)
        {
            Builder = builder;

            if (oldBuilder != null)
            {
                if (inverted)
                {
                    if (oldBuilder._foreignKeyProperties != null
                        || oldBuilder._principalKeyProperties != null)
                    {
                        throw new InvalidOperationException(CoreStrings.RelationshipCannotBeInverted);
                    }
                }

                DeclaringEntityType = oldBuilder.DeclaringEntityType;
                RelatedEntityType = oldBuilder.RelatedEntityType;

                _foreignKeyProperties = foreignKeySet
                    ? builder.Metadata.Properties
                    : oldBuilder._foreignKeyProperties;
                _principalKeyProperties = principalKeySet
                    ? builder.Metadata.PrincipalKey.Properties
                    : oldBuilder._principalKeyProperties;
                _required = requiredSet
                    ? builder.Metadata.IsRequired
                    : oldBuilder._required;

                var foreignKey = builder.Metadata;
                ForeignKey.AreCompatible(
                    foreignKey.PrincipalEntityType,
                    foreignKey.DeclaringEntityType,
                    foreignKey.DependentToPrincipal?.GetIdentifyingMemberInfo(),
                    foreignKey.PrincipalToDependent?.GetIdentifyingMemberInfo(),
                    _foreignKeyProperties,
                    _principalKeyProperties,
                    foreignKey.IsUnique,
                    shouldThrow: true);
            }
        }

        /// <summary>
        ///     Gets the first entity type used to configure this relationship.
        /// </summary>
        protected virtual IMutableEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets the second entity type used to configure this relationship.
        /// </summary>
        protected virtual IMutableEntityType RelatedEntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalRelationshipBuilder Builder { get; [param: NotNull] set; }

        /// <summary>
        ///     Gets the internal builder being used to configure this relationship.
        /// </summary>
        InternalRelationshipBuilder IInfrastructure<InternalRelationshipBuilder>.Instance => Builder;

        /// <summary>
        ///     The foreign key that represents this relationship.
        /// </summary>
        public virtual IMutableForeignKey Metadata => Builder.Metadata;

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
