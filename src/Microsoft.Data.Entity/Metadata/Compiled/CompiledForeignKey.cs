// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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

        public IReadOnlyList<IProperty> DependentProperties
        {
            get { return Definition.DependentPropertyIndexes.Select(i => DependentType.Properties[i]).ToArray(); }
        }

        public IReadOnlyList<IProperty> PrincipalProperties
        {
            get { return Definition.PrincipalPropertyIndexes.Select(i => PrincipalType.Properties[i]).ToArray(); }
        }

        public IEntityType PrincipalType
        {
            get { return _model.EntityTypes[Definition.PrincipalIndex]; }
        }

        public IEntityType DependentType
        {
            get { return _model.EntityTypes[Definition.DependentIndex]; }
        }

        public bool IsRequired
        {
            get { return DependentProperties.Any(p => !p.IsNullable); }
        }

        public bool IsUnique
        {
            get { return false; }
        }

        public string StorageName
        {
            get { return null; }
        }

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
