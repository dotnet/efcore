// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Base class used for configuring a relationship.
    /// </summary>
    public abstract class RelationshipBuilderBase : IInfrastructure<InternalRelationshipBuilder>
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
        protected RelationshipBuilderBase(
            [NotNull] IMutableEntityType principalEntityType,
            [NotNull] IMutableEntityType dependentEntityType,
            [NotNull] IMutableForeignKey foreignKey)
            : this(((ForeignKey)foreignKey).Builder, null)
        {
            PrincipalEntityType = principalEntityType;
            DependentEntityType = dependentEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected RelationshipBuilderBase(
            InternalRelationshipBuilder builder,
            RelationshipBuilderBase oldBuilder,
            bool foreignKeySet = false,
            bool principalKeySet = false,
            bool requiredSet = false)
        {
            Check.NotNull(builder, nameof(builder));

            Builder = builder;
            if (oldBuilder != null)
            {
                PrincipalEntityType = oldBuilder.PrincipalEntityType;
                DependentEntityType = oldBuilder.DependentEntityType;
                _foreignKeyProperties = foreignKeySet
                    ? builder.Metadata.Properties
                    : ((EntityType)oldBuilder.DependentEntityType).Builder.GetActualProperties(oldBuilder._foreignKeyProperties, null);
                _principalKeyProperties = principalKeySet
                    ? builder.Metadata.PrincipalKey.Properties
                    : ((EntityType)oldBuilder.PrincipalEntityType).Builder.GetActualProperties(oldBuilder._principalKeyProperties, null);
                _required = requiredSet
                    ? builder.Metadata.IsRequired
                    : oldBuilder._required;

                var foreignKey = builder.Metadata;
                ForeignKey.AreCompatible(
                    (EntityType)oldBuilder.PrincipalEntityType,
                    (EntityType)oldBuilder.DependentEntityType,
                    foreignKey.DependentToPrincipal?.GetIdentifyingMemberInfo(),
                    foreignKey.PrincipalToDependent?.GetIdentifyingMemberInfo(),
                    _foreignKeyProperties,
                    _principalKeyProperties,
                    foreignKey.IsUnique,
                    shouldThrow: true);
            }
        }

        /// <summary>
        ///     The principal entity type used to configure this relationship.
        /// </summary>
        protected virtual IMutableEntityType PrincipalEntityType { get; }

        /// <summary>
        ///     The dependent entity type used to configure this relationship.
        /// </summary>
        protected virtual IMutableEntityType DependentEntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalRelationshipBuilder Builder { get; [param: NotNull] set; }

        /// <summary>
        ///     The foreign key that represents this relationship.
        /// </summary>
        public virtual IMutableForeignKey Metadata => Builder.Metadata;

        /// <summary>
        ///     Gets the internal builder being used to configure this relationship.
        /// </summary>
        InternalRelationshipBuilder IInfrastructure<InternalRelationshipBuilder>.Instance => Builder;

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
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
