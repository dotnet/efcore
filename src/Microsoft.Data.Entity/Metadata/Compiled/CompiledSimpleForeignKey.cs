// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledSimpleForeignKey : NoAnnotations
    {
        private readonly IModel _model;

        protected CompiledSimpleForeignKey(IModel model)
        {
            _model = model;
        }

        protected abstract ForeignKeyDefinition Definition { get; }

        public IReadOnlyList<IProperty> DependentProperties
        {
            get { return new[] { DependentType.Properties[Definition.DependentPropertyIndex] }; }
        }

        public IReadOnlyList<IProperty> PrincipalProperties
        {
            get { return PrincipalType.Key; }
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
            public short DependentPropertyIndex;
            public short PrincipalIndex;

            public ForeignKeyDefinition(short dependentIndex, short dependentPropertyIndex, short principalIndex)
            {
                DependentIndex = dependentIndex;
                DependentPropertyIndex = dependentPropertyIndex;
                PrincipalIndex = principalIndex;
            }
        }
    }
}
