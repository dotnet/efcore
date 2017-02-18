using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class MongoDbModelValidator : CoreModelValidator
    {
        public MongoDbModelValidator(
            [NotNull] ILogger<MongoDbModelValidator> loggerFactory)
            : base(Check.NotNull(loggerFactory, nameof(loggerFactory)))
        {
        }

        public override void Validate([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));
            base.Validate(model);

            EnsureDistinctCollectionNames(model);
            ValidateDerivedTypes(model);
        }

        protected virtual void EnsureDistinctCollectionNames([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));
            var tables = new HashSet<string>();
            var duplicateCollectionNames = model
                .GetEntityTypes()
                .Where(et => et.BaseType == null)
                .Select(entityType => new
                {
                    new MongoDbEntityTypeAnnotations(entityType).CollectionName,
                    DisplayName = entityType.DisplayName()
                })
                .Where(tuple => !tables.Add(tuple.CollectionName));
            foreach (var tuple in duplicateCollectionNames)
            {
                ShowError($"Duplicate collection name \"{tuple.CollectionName}\" defined on entity type \"{tuple.DisplayName}\".");
            }
        }

        protected virtual void ValidateDerivedTypes([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));
            var discriminatorSet = new HashSet<Tuple<IEntityType,string>>();
            IEnumerable<IEntityType> derivedTypes = model.GetEntityTypes()
                .Where(entityType => entityType.BaseType != null && entityType.ClrType.IsInstantiable());
            foreach (IEntityType entityType in derivedTypes)
            {
                ValidateDiscriminator(entityType, discriminatorSet);
            }
        }

        private void ValidateDiscriminator(IEntityType entityType, ISet<Tuple<IEntityType,string>> discriminatorSet)
        {
            var annotations = new MongoDbEntityTypeAnnotations(entityType);
            if (string.IsNullOrWhiteSpace(annotations.Discriminator))
            {
                ShowError($"Missing discriminator value for entity type {entityType.DisplayName()}.");
            }
            if (!discriminatorSet.Add(Tuple.Create(entityType.RootType(), annotations.Discriminator)))
            {
                ShowError($"Duplicate discriminator value {annotations.Discriminator} for root entity type {entityType.RootType().DisplayName()} (defined on {entityType.DisplayName()}).");
            }
        }
    }
}
