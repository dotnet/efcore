// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Internal
{
    public class ModelForeignKeyUndirectedGraphAdapter : Graph<EntityType>
    {
        private readonly Model _model;

        public ModelForeignKeyUndirectedGraphAdapter([NotNull] Model model)
        {
            _model = model;
        }

        public override IEnumerable<EntityType> Vertices => _model.EntityTypes;

        public override IEnumerable<EntityType> GetOutgoingNeighbours(EntityType from)
            => @from.GetForeignKeys().Select(fk => fk.PrincipalEntityType)
                .Union(_model.GetReferencingForeignKeys(@from).Select(fk => fk.EntityType));

        public override IEnumerable<EntityType> GetIncomingNeighbours(EntityType to)
            => GetOutgoingNeighbours(to);
    }
}
