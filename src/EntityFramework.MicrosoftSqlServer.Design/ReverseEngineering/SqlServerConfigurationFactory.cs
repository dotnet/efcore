// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.Configuration;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
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
