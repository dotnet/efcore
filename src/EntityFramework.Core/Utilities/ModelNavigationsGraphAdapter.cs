// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Utilities
{
    public class ModelNavigationsGraphAdapter : Graph<EntityType>
    {
        private readonly Model _model;

        public ModelNavigationsGraphAdapter([NotNull] Model model)
        {
            Check.NotNull(model, "model");

            _model = model;
        }

        public override IEnumerable<EntityType> Vertices
        {
            get { return _model.EntityTypes; }
        }

        public override IEnumerable<EntityType> GetOutgoingNeighbours(EntityType from)
        {
            return from.ForeignKeys.Where(fk => fk.GetNavigationToPrincipal() != null).Select(fk => fk.ReferencedEntityType)
                .Union(_model.GetReferencingForeignKeys(from).Where(fk => fk.GetNavigationToDependent() != null).Select(fk => fk.EntityType));
        }

        public override IEnumerable<EntityType> GetIncomingNeighbours(EntityType to)
        {
            return to.ForeignKeys.Where(fk => fk.GetNavigationToDependent() != null).Select(fk => fk.ReferencedEntityType)
                .Union(_model.GetReferencingForeignKeys(to).Where(fk => fk.GetNavigationToPrincipal() != null).Select(fk => fk.EntityType));
        }
    }
}
