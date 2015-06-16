// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.Configuration
{
    public class SqlServerNavigationPropertyInitializer
    {
        public SqlServerNavigationPropertyInitializer([NotNull] string navPropName, [NotNull] string principalEntityTypeName)
        {
            Check.NotEmpty(navPropName, nameof(navPropName));
            Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));

            NavigationPropertyName = navPropName;
            PrincipalEntityTypeName = principalEntityTypeName;
        }

        public virtual string NavigationPropertyName { get; [param: NotNull] private set; }
        public virtual string PrincipalEntityTypeName { get; [param: NotNull] private set; }
    }
}
