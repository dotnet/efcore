// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class ModelNavigationsGraphAdapter : Graph<EntityType>
    {
        private readonly Model _model;

        public ModelNavigationsGraphAdapter([NotNull] Model model)
        {
            _model = model;
        }

        public override IEnumerable<EntityType> Vertices => _model.GetEntityTypes();

        public override IEnumerable<EntityType> GetOutgoingNeighbours(EntityType from)
            => @from.GetForeignKeys().Where(fk => fk.DependentToPrincipal != null).Select(fk => fk.PrincipalEntityType)
                .Union(@from.FindReferencingForeignKeys().Where(fk => fk.PrincipalToDependent != null).Select(fk => fk.DeclaringEntityType));

        public override IEnumerable<EntityType> GetIncomingNeighbours(EntityType to)
            => to.GetForeignKeys().Where(fk => fk.PrincipalToDependent != null).Select(fk => fk.PrincipalEntityType)
                .Union(to.FindReferencingForeignKeys().Where(fk => fk.DependentToPrincipal != null).Select(fk => fk.DeclaringEntityType));
    }
}
