// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class ParameterNameGenerator
    {
        private int _count;

        public virtual string GenerateNext()
            => $"{ParameterPrefix}p{_count++}";

        public virtual string Generate([NotNull] string name)
            => $"{ParameterPrefix}{Check.NotEmpty(name, nameof(name))}";

        protected virtual string ParameterPrefix => "@";
    }
}
