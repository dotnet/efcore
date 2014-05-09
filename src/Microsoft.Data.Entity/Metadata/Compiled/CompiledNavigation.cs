// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledNavigation : NoAnnotations
    {
        private readonly IModel _model;

        protected CompiledNavigation(IModel model)
        {
            _model = model;
        }

        protected abstract NavigationDefinition Definition { get; }

        public IEntityType EntityType
        {
            get { return _model.EntityTypes[Definition.EntityTypeIndex]; }
        }

        public IForeignKey ForeignKey
        {
            get { return _model.EntityTypes[Definition.ForeignKeyTypeIndex].ForeignKeys[Definition.ForeignKeyIndex]; }
        }

        protected struct NavigationDefinition
        {
            public short EntityTypeIndex;
            public short ForeignKeyTypeIndex;
            public short ForeignKeyIndex;

            public NavigationDefinition(short entityTypeIndex, short foreignKeyTypeIndex, short foreignKeyIndex)
            {
                EntityTypeIndex = entityTypeIndex;
                ForeignKeyTypeIndex = foreignKeyTypeIndex;
                ForeignKeyIndex = foreignKeyIndex;
            }
        }
    }
}
