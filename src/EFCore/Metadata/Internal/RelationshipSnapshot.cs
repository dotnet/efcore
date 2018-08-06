using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationshipSnapshot
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationshipSnapshot(
            [NotNull] InternalRelationshipBuilder relationship,
            [CanBeNull] EntityType.Snapshot weakEntityTypeSnapshot)
        {
            Relationship = relationship;
            WeakEntityTypeSnapshot = weakEntityTypeSnapshot;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Relationship { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType.Snapshot WeakEntityTypeSnapshot { [DebuggerStepThrough] get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Attach([CanBeNull] InternalEntityTypeBuilder entityTypeBuilder = null)
        {
            entityTypeBuilder = entityTypeBuilder ?? Relationship.Metadata.DeclaringEntityType.Builder;
            var newRelationship = Relationship.Attach(entityTypeBuilder);

            if (newRelationship != null)
            {
                WeakEntityTypeSnapshot?.Attach(
                    newRelationship.Metadata.ResolveOtherEntityType(entityTypeBuilder.Metadata).Builder);
            }

            return newRelationship;
        }
    }
}
