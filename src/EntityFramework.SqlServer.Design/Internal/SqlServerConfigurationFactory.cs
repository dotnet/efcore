// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Scaffolding.Internal.Configuration;
using Microsoft.Data.Entity.Scaffolding.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    public class SqlServerConfigurationFactory : ConfigurationFactory
    {
        public SqlServerConfigurationFactory(
            [NotNull] IRelationalAnnotationProvider extensionsProvider,
            [NotNull] CSharpUtilities cSharpUtilities,
            [NotNull] ModelUtilities modelUtilities)
            : base(extensionsProvider, cSharpUtilities, modelUtilities)
        {
        }

        public override ModelConfiguration CreateModelConfiguration(
            [NotNull] IModel model, [NotNull] CustomConfiguration customConfiguration)
        {
            return new SqlServerModelConfiguration(
                this, model, customConfiguration, ExtensionsProvider, CSharpUtilities, ModelUtilities);
        }
    }
}
