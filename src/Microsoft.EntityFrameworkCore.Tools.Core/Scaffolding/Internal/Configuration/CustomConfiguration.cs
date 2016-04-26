// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal.Configuration
{
    public class CustomConfiguration
    {
        public CustomConfiguration([NotNull] string connectionString,
            [CanBeNull] string contextClassName, [NotNull] string @namespace,
            bool useFluentApiOnly)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotEmpty(@namespace, nameof(@namespace));

            ConnectionString = connectionString;
            ContextClassName = contextClassName;
            Namespace = @namespace;
            UseFluentApiOnly = useFluentApiOnly;
        }

        public virtual string ConnectionString { get; [param: NotNull] set; }
        public virtual string ContextClassName { get; [param: CanBeNull] set; }
        public virtual string Namespace { get; [param: CanBeNull] set; }
        public virtual bool UseFluentApiOnly { get; set; }
    }
}
