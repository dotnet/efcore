using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore
{
    public static class KustoEntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder ToContainer(
            this EntityTypeBuilder entityTypeBuilder,
            string? name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            entityTypeBuilder.Metadata.SetContainer(name);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ToContainer<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string? name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToContainer((EntityTypeBuilder)entityTypeBuilder, name);

        public static IConventionEntityTypeBuilder? ToContainer(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetContainer(name, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetContainer(name, fromDataAnnotation);

            return entityTypeBuilder;
        }

        public static bool CanSetContainer(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(KustoAnnotationNames.ContainerName, name, fromDataAnnotation);
        }

        public static EntityTypeBuilder HasPartitionKey(
            this EntityTypeBuilder entityTypeBuilder,
            string? name,
            params string[]? additionalPropertyNames)
        {
            Check.NullButNotEmpty(name, nameof(name));
            Check.HasNoEmptyElements(additionalPropertyNames, nameof(additionalPropertyNames));

            if (name is null)
            {
                entityTypeBuilder.Metadata.SetPartitionKeyPropertyNames(null);
            }
            else
            {
                var propertyNames = new List<string> { name };
                propertyNames.AddRange(additionalPropertyNames);
                entityTypeBuilder.Metadata.SetPartitionKeyPropertyNames(propertyNames);
            }

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> HasPartitionKey<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string? name,
            params string[]? additionalPropertyNames)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)HasPartitionKey((EntityTypeBuilder)entityTypeBuilder, name, additionalPropertyNames);

        public static EntityTypeBuilder<TEntity> HasPartitionKey<TEntity, TProperty>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            Expression<Func<TEntity, TProperty>> propertyExpression)
            where TEntity : class
        {
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            entityTypeBuilder.Metadata.SetPartitionKeyPropertyNames(
                propertyExpression.GetMemberAccessList().Select(e => e.GetSimpleMemberName()).ToList());

            return entityTypeBuilder;
        }

        public static IConventionEntityTypeBuilder? HasPartitionKey(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
            => entityTypeBuilder.HasPartitionKey(name == null ? null : [name], fromDataAnnotation);

        public static bool CanSetPartitionKey(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
            => entityTypeBuilder.CanSetPartitionKey(name == null ? null : [name], fromDataAnnotation);

        public static IConventionEntityTypeBuilder? HasPartitionKey(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            IReadOnlyList<string>? propertyNames,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetPartitionKey(propertyNames, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetPartitionKeyPropertyNames(propertyNames, fromDataAnnotation);

            return entityTypeBuilder;
        }

        public static bool CanSetPartitionKey(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            IReadOnlyList<string>? names,
            bool fromDataAnnotation = false)
            => entityTypeBuilder.CanSetAnnotation(
                KustoAnnotationNames.PartitionKeyNames,
                names is null ? names : Check.HasNoEmptyElements(names, nameof(names)),
                fromDataAnnotation);

        public static EntityTypeBuilder UseETagConcurrency(this EntityTypeBuilder entityTypeBuilder)
        {
            entityTypeBuilder.Property<string>("_etag")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> UseETagConcurrency<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)UseETagConcurrency((EntityTypeBuilder)entityTypeBuilder);
    }
}
