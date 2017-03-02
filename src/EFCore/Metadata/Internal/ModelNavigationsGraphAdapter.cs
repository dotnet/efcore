// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ModelNavigationsGraphAdapter : Graph<EntityType>
    {
        private readonly Model _model;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ModelNavigationsGraphAdapter([NotNull] Model model)
        {
            _model = model;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<EntityType> Vertices => _model.GetEntityTypes();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<EntityType> GetOutgoingNeighbours(EntityType from)
            => from.GetForeignKeys().Where(fk => fk.DependentToPrincipal != null).Select(fk => fk.PrincipalEntityType)
                .Union(from.GetReferencingForeignKeys().Where(fk => fk.PrincipalToDependent != null).Select(fk => fk.DeclaringEntityType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<EntityType> GetIncomingNeighbours(EntityType to)
            => to.GetForeignKeys().Where(fk => fk.PrincipalToDependent != null).Select(fk => fk.PrincipalEntityType)
                .Union(to.GetReferencingForeignKeys().Where(fk => fk.DependentToPrincipal != null).Select(fk => fk.DeclaringEntityType));
    }
}
