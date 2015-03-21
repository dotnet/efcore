// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledForeignKey : NoAnnotations
    {
        private readonly IModel _model;

        protected CompiledForeignKey(IModel model)
        {
            _model = model;
        }

        protected abstract ForeignKeyDefinition Definition { get; }

        public IReadOnlyList<IProperty> Properties => Definition.DependentPropertyIndexes.Select(i => EntityType.GetProperties().ElementAt(i)).ToArray();

        public IReadOnlyList<IProperty> ReferencedProperties => Definition.PrincipalPropertyIndexes.Select(i => PrincipalEntityType.GetProperties().ElementAt(i)).ToArray();

        public IKey PrincipalKey => PrincipalEntityType.GetPrimaryKey();

        public IEntityType PrincipalEntityType => _model.EntityTypes[Definition.PrincipalIndex];

        public IEntityType EntityType => _model.EntityTypes[Definition.DependentIndex];

        public bool IsRequired => Properties.Any(p => !p.IsNullable);

        public bool IsUnique => false;

        protected struct ForeignKeyDefinition
        {
            public short DependentIndex;
            public short PrincipalIndex;
            public short[] DependentPropertyIndexes;
            public short[] PrincipalPropertyIndexes;

            public ForeignKeyDefinition(
                short dependentIndex, short[] dependentPropertyIndexes, short principalIndex, short[] principalPropertyIndexes)
            {
                DependentIndex = dependentIndex;
                DependentPropertyIndexes = dependentPropertyIndexes;
                PrincipalIndex = principalIndex;
                PrincipalPropertyIndexes = principalPropertyIndexes;
            }
        }
    }
}
