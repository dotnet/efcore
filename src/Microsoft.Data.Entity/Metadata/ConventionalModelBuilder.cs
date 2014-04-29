// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ConventionalModelBuilder : ModelBuilder
    {
        // TODO: Get the default convention list from DI
        // TODO: Configure property facets, foreign keys & navigation properties
        private readonly IList<IModelConvention> _conventions = new List<IModelConvention>
            {
                new PropertiesConvention(),
                new KeyConvention()
            };

        public ConventionalModelBuilder([NotNull] Model model)
            : base(model)
        {
        }

        public virtual IList<IModelConvention> Conventions
        {
            get { return _conventions; }
        }

        protected override void OnEntityTypeAdded([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            foreach (var convention in _conventions)
            {
                convention.Apply(entityType);
            }
        }
    }
}
