// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ModelBuilder : BasicModelBuilder
    {
        // TODO: Get the default convention list from DI
        // TODO: Configure property facets, foreign keys & navigation properties
        private readonly IList<IModelConvention> _conventions;

        public ModelBuilder([NotNull] Model model)
            : base(model)
        {
            _conventions = new List<IModelConvention>
                {
                    new PropertiesConvention(),
                    new KeyConvention()
                };
        }

        protected ModelBuilder([NotNull] Model model, [NotNull] IList<IModelConvention> conventions)
            : base(model)
        {
            Check.NotNull(conventions, "conventions");

            _conventions = conventions;
        }

        public virtual IList<IModelConvention> Conventions
        {
            get { return _conventions; }
        }

        protected override void OnEntityTypeAdded([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            foreach (var convention in Conventions)
            {
                convention.Apply(entityType);
            }
        }
    }
}
