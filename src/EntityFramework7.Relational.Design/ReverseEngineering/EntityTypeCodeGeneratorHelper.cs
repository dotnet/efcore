// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class EntityTypeCodeGeneratorHelper
    {
        public EntityTypeCodeGeneratorHelper([NotNull] EntityTypeGeneratorModel generatorModel)
        {
            Check.NotNull(generatorModel, nameof(generatorModel));

            GeneratorModel = generatorModel;
        }

        public virtual EntityTypeGeneratorModel GeneratorModel { get; }

        public virtual IEnumerable<IProperty> OrderedEntityProperties
        {
            get
            {
                return GeneratorModel.Generator.ModelUtilities
                    .OrderedProperties(GeneratorModel.EntityType);
            }
        }
    }
}
