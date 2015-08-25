// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    // TODO decouple GeneratorModels and their helpers so that this factory is unnecessary
    public abstract class CodeGeneratorHelperFactory
    {
        protected readonly ModelUtilities ModelUtilities;

        protected CodeGeneratorHelperFactory([NotNull] ModelUtilities modelUtilities)
        {
            Check.NotNull(modelUtilities, nameof(modelUtilities));

            ModelUtilities = modelUtilities;
        }

        public abstract DbContextCodeGeneratorHelper DbContextHelper([NotNull] DbContextGeneratorModel generatorModel);

        public virtual EntityTypeCodeGeneratorHelper EntityTypeHelper([NotNull] EntityTypeGeneratorModel generatorModel)
            => new EntityTypeCodeGeneratorHelper(
                Check.NotNull(generatorModel, nameof(generatorModel)),
                ModelUtilities);
    }
}
