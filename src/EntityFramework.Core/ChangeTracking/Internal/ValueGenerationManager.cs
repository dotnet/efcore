// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class ValueGenerationManager : IValueGenerationManager
    {
        private readonly IValueGeneratorSelector _valueGeneratorSelector;
        private readonly IKeyPropagator _keyPropagator;

        public ValueGenerationManager(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector,
            [NotNull] IKeyPropagator keyPropagator)
        {
            _valueGeneratorSelector = valueGeneratorSelector;
            _keyPropagator = keyPropagator;
        }

        public virtual void Generate(InternalEntityEntry entry)
        {
            foreach (var property in entry.EntityType.GetProperties())
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
                        var valueGenerator = _valueGeneratorSelector.Select(property);
                        Debug.Assert(valueGenerator != null);

                        var generatedValue = valueGenerator.Next();
                        SetGeneratedValue(entry, property, generatedValue, valueGenerator.GeneratesTemporaryValues);
                    }
                }
            }
        }

        public virtual bool MayGetTemporaryValue(IProperty property)
            => property.GenerateValueOnAdd && _valueGeneratorSelector.Select(property).GeneratesTemporaryValues;

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
