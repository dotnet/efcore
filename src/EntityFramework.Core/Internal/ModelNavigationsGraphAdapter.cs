// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Internal
{
    public class ModelNavigationsGraphAdapter : Graph<EntityType>
    {
        private readonly Model _model;

        public ModelNavigationsGraphAdapter([NotNull] Model model)
        {
            _model = model;
        }

        public override IEnumerable<EntityType> Vertices => _model.EntityTypes;

        public override IEnumerable<EntityType> GetOutgoingNeighbours(EntityType from)
            => @from.GetForeignKeys().Where(fk => fk.DependentToPrincipal != null).Select(fk => fk.PrincipalEntityType)
                .Union(_model.GetReferencingForeignKeys(@from).Where(fk => fk.PrincipalToDependent != null).Select(fk => fk.EntityType));

        public override IEnumerable<EntityType> GetIncomingNeighbours(EntityType to)
            => to.GetForeignKeys().Where(fk => fk.PrincipalToDependent != null).Select(fk => fk.PrincipalEntityType)
                .Union(_model.GetReferencingForeignKeys(to).Where(fk => fk.DependentToPrincipal != null).Select(fk => fk.EntityType));
    }
}
