// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a one-to-many relationship.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class ReferenceCollectionBuilderBase : IInfrastructure<IMutableModel>, IInfrastructure<InternalRelationshipBuilder>
    {
        private readonly IReadOnlyList<Property> _foreignKeyProperties;
        private readonly IReadOnlyList<Property> _principalKeyProperties;
        private readonly bool? _required;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReferenceCollectionBuilderBase(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            [NotNull] InternalRelationshipBuilder builder)
            : this(builder, null)
        {
            Check.NotNull(builder, nameof(builder));

            PrincipalEntityType = principalEntityType;
            DependentEntityType = dependentEntityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected ReferenceCollectionBuilderBase(
            InternalRelationshipBuilder builder,
            ReferenceCollectionBuilderBase oldBuilder,
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
                    : DependentEntityType.Builder.GetActualProperties(oldBuilder._foreignKeyProperties, null);
                _principalKeyProperties = principalKeySet
                    ? builder.Metadata.PrincipalKey.Properties
                    : PrincipalEntityType.Builder.GetActualProperties(oldBuilder._principalKeyProperties, null);
                _required = requiredSet
                    ? builder.Metadata.IsRequired
                    : oldBuilder._required;

                var foreignKey = builder.Metadata;
                ForeignKey.AreCompatible(
                    PrincipalEntityType,
                    DependentEntityType,
                    foreignKey.DependentToPrincipal?.GetIdentifyingMemberInfo(),
                    foreignKey.PrincipalToDependent?.GetIdentifyingMemberInfo(),
                    _foreignKeyProperties,
                    _principalKeyProperties,
                    foreignKey.IsUnique,
                    _required,
                    true);
            }
        }

        /// <summary>
        ///     Gets the principal entity type used to configure this relationship.
        /// </summary>
        protected virtual EntityType PrincipalEntityType { get; }

        /// <summary>
        ///     Gets the dependent entity type used to configure this relationship.
        /// </summary>
        protected virtual EntityType DependentEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder Builder { get; [param: NotNull] set; }

        /// <summary>
        ///     The foreign key that represents this relationship.
        /// </summary>
        public virtual IMutableForeignKey Metadata => Builder.Metadata;

        /// <summary>
        ///     The model that this relationship belongs to.
        /// </summary>
        IMutableModel IInfrastructure<IMutableModel>.Instance => Builder.ModelBuilder.Metadata;

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
