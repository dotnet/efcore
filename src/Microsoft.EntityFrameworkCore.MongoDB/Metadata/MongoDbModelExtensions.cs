using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public static class MongoDbModelExtensions
    {
        public static InternalEntityTypeBuilder ComplexType<TEntity>([NotNull] this IModel model,
                ConfigurationSource configurationSource)
            => Check.NotNull(model, nameof(model)).ComplexType(typeof(TEntity), configurationSource);

        public static InternalEntityTypeBuilder ComplexType([NotNull] this IModel model,
                [NotNull] Type entityClrType,
                ConfigurationSource configurationSource)
            => Check.NotNull(model, nameof(model))
                .ComplexType(new EntityType(entityClrType, model.AsModel(), configurationSource));

        public static InternalEntityTypeBuilder ComplexType([NotNull] this IModel model,
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(entityType, nameof(entityType));
            new MongoDbModelAnnotations(model).ComplexTypes.Add(entityType);
            return new InternalEntityTypeBuilder(entityType.AsEntityType(), model.AsModel().Builder);
        }

        public static IEnumerable<IEntityType> GetComplexTypes([NotNull] this IModel model)
            => new MongoDbModelAnnotations(Check.NotNull(model, nameof(model))).ComplexTypes;

        public static InternalEntityTypeBuilder FindComplexType<TEntity>([NotNull] this IModel model)
            => Check.NotNull(model, nameof(model)).FindComplexType(typeof(TEntity));

        public static InternalEntityTypeBuilder FindComplexType([NotNull] this IModel model,
            [NotNull] Type entityClrType)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(entityClrType, nameof(entityClrType));
            return new MongoDbModelAnnotations(model)
                .ComplexTypes
                .FirstOrDefault(entityType => entityType.ClrType == entityClrType)
                ?.AsEntityType()
                .Builder;
        }
    }
}