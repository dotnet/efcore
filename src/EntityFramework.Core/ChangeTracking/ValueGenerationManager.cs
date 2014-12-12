// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ValueGenerationManager
    {
        private readonly DbContextService<ValueGeneratorCache> _valueGeneratorCache;
        private readonly DbContextService<DataStoreServices> _dataStoreServices;
        private readonly ForeignKeyValuePropagator _foreignKeyValuePropagator;

        public ValueGenerationManager(
            [NotNull] DbContextService<ValueGeneratorCache> valueGeneratorCache,
            [NotNull] DbContextService<DataStoreServices> dataStoreServices,
            [NotNull] ForeignKeyValuePropagator foreignKeyValuePropagator)
        {
            Check.NotNull(valueGeneratorCache, "valueGeneratorCache");
            Check.NotNull(dataStoreServices, "dataStoreServices");
            Check.NotNull(foreignKeyValuePropagator, "foreignKeyValuePropagator");

            _valueGeneratorCache = valueGeneratorCache;
            _dataStoreServices = dataStoreServices;
            _foreignKeyValuePropagator = foreignKeyValuePropagator;
        }

        public virtual void Generate([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            foreach (var property in entry.EntityType.Properties)
            {
                var isForeignKey = property.IsForeignKey();

                if ((property.GenerateValueOnAdd || isForeignKey)
                    && entry.HasDefaultValue(property))
                {
                    if (isForeignKey)
                    {
                        _foreignKeyValuePropagator.PropagateValue(entry, property);
                    }
                    else
                    {
                        var generatedValue = _valueGeneratorCache.Service.GetGenerator(property)?.Next(property, _dataStoreServices);

                        SetGeneratedValue(entry, generatedValue, property);
                    }
                }
            }
        }

        public virtual async Task GenerateAsync([NotNull] StateEntry entry, CancellationToken cancellationToken)
        {
            Check.NotNull(entry, "entry");

            foreach (var property in entry.EntityType.Properties)
            {
                var isForeignKey = property.IsForeignKey();

                if ((property.GenerateValueOnAdd || isForeignKey)
                    && entry.HasDefaultValue(property))
                {
                    if (isForeignKey)
                    {
                        await _foreignKeyValuePropagator.PropagateValueAsync(entry, property, cancellationToken).WithCurrentCulture();
                    }
                    else
                    {
                        var valueGenerator = _valueGeneratorCache.Service.GetGenerator(property);
                        var generatedValue = valueGenerator == null
                            ? null
                            : await valueGenerator.NextAsync(property, _dataStoreServices, cancellationToken).WithCurrentCulture();

                        SetGeneratedValue(entry, generatedValue, property);
                    }
                }
            }
        }

        private static void SetGeneratedValue(StateEntry entry, GeneratedValue generatedValue, IProperty property)
        {
            if (generatedValue != null)
            {
                entry[property] = generatedValue.Value;

                if (generatedValue.IsTemporary)
                {
                    entry.MarkAsTemporary(property);
                }
            }
        }
    }
}
