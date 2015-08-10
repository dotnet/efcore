// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class OptionsBuilderConfiguration
    {
        public OptionsBuilderConfiguration([NotNull] string methodBody)
        {
            Check.NotNull(methodBody, nameof(methodBody));

            MethodBody = methodBody;
        }

        public virtual string MethodBody { get; }
    }
}
