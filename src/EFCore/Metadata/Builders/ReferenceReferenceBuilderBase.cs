using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ReferenceReferenceBuilderBase : IInfrastructure<IMutableModel>, IInfrastructure<InternalRelationshipBuilder>
    {
        private readonly IReadOnlyList<Property> _foreignKeyProperties;
        private readonly IReadOnlyList<Property> _principalKeyProperties;
        private readonly bool? _required;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReferenceReferenceBuilderBase(
            [NotNull] EntityType declaringEntityType,
            [NotNull] EntityType relatedEntityType,
            [NotNull] InternalRelationshipBuilder builder)
            : this(builder, null)
        {
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));
            Check.NotNull(relatedEntityType, nameof(relatedEntityType));
            Check.NotNull(builder, nameof(builder));

            DeclaringEntityType = declaringEntityType;
            RelatedEntityType = relatedEntityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected ReferenceReferenceBuilderBase(
            InternalRelationshipBuilder builder,
            ReferenceReferenceBuilderBase oldBuilder,
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
                    foreignKey.DependentToPrincipal?.PropertyInfo,
                    foreignKey.PrincipalToDependent?.PropertyInfo,
                    _foreignKeyProperties,
                    _principalKeyProperties,
                    foreignKey.IsUnique,
                    _required,
                    shouldThrow: true);
            }
        }

        /// <summary>
        ///     Gets the first entity type used to configure this relationship.
        /// </summary>
        protected virtual EntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets the second entity type used to configure this relationship.
        /// </summary>
        protected virtual EntityType RelatedEntityType { get; }

        /// <summary>
        ///     Gets the internal builder being used to configure this relationship.
        /// </summary>
        protected virtual InternalRelationshipBuilder Builder { get; }

        /// <summary>
        ///     Gets the internal builder being used to configure this relationship.
        /// </summary>
        InternalRelationshipBuilder IInfrastructure<InternalRelationshipBuilder>.Instance => Builder;

        /// <summary>
        ///     The foreign key that represents this relationship.
        /// </summary>
        public virtual IMutableForeignKey Metadata => Builder.Metadata;

        /// <summary>
        ///     The model that this relationship belongs to.
        /// </summary>
        IMutableModel IInfrastructure<IMutableModel>.Instance => Builder.ModelBuilder.Metadata;
    }
}