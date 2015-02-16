// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.ValueGeneration.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class ValueGenerationManager
    {
        private readonly DbContextService<ValueGeneratorCache> _valueGeneratorCache;
        private readonly DbContextService<DataStoreServices> _dataStoreServices;
        private readonly KeyPropagator _keyPropagator;

        public ValueGenerationManager(
            [NotNull] DbContextService<ValueGeneratorCache> valueGeneratorCache,
            [NotNull] DbContextService<DataStoreServices> dataStoreServices,
            [NotNull] KeyPropagator keyPropagator)
        {
            _valueGeneratorCache = valueGeneratorCache;
            _dataStoreServices = dataStoreServices;
            _keyPropagator = keyPropagator;
        }

        public virtual void Generate([NotNull] InternalEntityEntry entry)
        {
            foreach (var property in entry.EntityType.Properties)
            {
                var isForeignKey = property.IsForeignKey();

                if ((property.GenerateValueOnAdd || isForeignKey)
                    && property.IsSentinelValue(entry[property]))
                {
                    if (isForeignKey)
                    {
                        _keyPropagator.PropagateValue(entry, property);
                    }
                    else
                    {
                        var valueGenerator = _valueGeneratorCache.Service.GetGenerator(property);
                        Debug.Assert(valueGenerator != null);

                        var generatedValue = valueGenerator.Next(_dataStoreServices);
                        SetGeneratedValue(entry, property, generatedValue, valueGenerator.GeneratesTemporaryValues);
                    }
                }
            }
        }

        public virtual bool MayGetTemporaryValue([NotNull] IProperty property)
            => property.GenerateValueOnAdd
               && _valueGeneratorCache.Service.GetGenerator(property).GeneratesTemporaryValues;

        private static void SetGeneratedValue(InternalEntityEntry entry, IProperty property, object generatedValue, bool isTemporary)
        {
            if (generatedValue != null)
            {
                entry[property] = generatedValue;

                if (isTemporary)
                {
                    entry.MarkAsTemporary(property);
                }
            }
        }
    }
}
