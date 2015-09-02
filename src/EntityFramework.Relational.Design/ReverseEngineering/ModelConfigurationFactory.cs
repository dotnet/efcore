// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class ModelConfigurationFactory
    {
        public ModelConfigurationFactory(
            [NotNull] IRelationalMetadataExtensionProvider extensionsProvider,
            [NotNull] CSharpUtilities cSharpUtilities,
            [NotNull] ModelUtilities modelUtilities)
        {
            Check.NotNull(extensionsProvider, nameof(extensionsProvider));
            Check.NotNull(cSharpUtilities, nameof(cSharpUtilities));
            Check.NotNull(modelUtilities, nameof(modelUtilities));

            ExtensionsProvider = extensionsProvider;
            CSharpUtilities = cSharpUtilities;
            ModelUtilities = modelUtilities;
        }

        protected virtual IRelationalMetadataExtensionProvider ExtensionsProvider { get;[param: NotNull] private set; }
        protected virtual CSharpUtilities CSharpUtilities { get;[param: NotNull] private set; }
        protected virtual ModelUtilities ModelUtilities { get;[param: NotNull] private set; }

        public abstract ModelConfiguration CreateModelConfiguration(
            [NotNull] IModel model, [NotNull] CustomConfiguration customConfiguration);
    }
}
