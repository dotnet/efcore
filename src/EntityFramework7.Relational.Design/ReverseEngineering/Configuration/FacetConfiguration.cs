// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class FacetConfiguration
    {
        public FacetConfiguration([NotNull] string methodBody)
        {
            Check.NotNull(methodBody, nameof(methodBody));

            MethodBody = methodBody;
        }

        public FacetConfiguration([NotNull] string @for, [NotNull] string methodBody)
        {
            Check.NotNull(@for, nameof(@for));
            Check.NotNull(methodBody, nameof(methodBody));

            For = @for;
            MethodBody = methodBody;
        }

        public virtual string For { get; }
        public virtual string MethodBody { get; }

        public override string ToString()
        {
            return (For == null ? MethodBody : For + "()." + MethodBody);
        }
    }
}
