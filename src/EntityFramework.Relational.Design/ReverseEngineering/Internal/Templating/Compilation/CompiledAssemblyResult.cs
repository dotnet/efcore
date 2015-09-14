// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal.Templating.Compilation
{
    public class CompiledAssemblyResult
    {
        public virtual bool Success { get; [param: NotNull] set; }

        public virtual Assembly Assembly { get; [param: NotNull] set; }

        public virtual IEnumerable<string> ErrorMessages { get; [param: NotNull] set; }

        public static CompiledAssemblyResult FromAssembly([NotNull] Assembly assembly)
        {
            Check.NotNull(assembly, nameof(assembly));

            return new CompiledAssemblyResult
            {
                Assembly = assembly,
                Success = true
            };
        }

        public static CompiledAssemblyResult FromErrorMessages([NotNull] IEnumerable<string> errorMessages)
        {
            Check.NotNull(errorMessages, nameof(errorMessages));

            return new CompiledAssemblyResult
            {
                ErrorMessages = errorMessages,
                Success = false
            };
        }
    }
}
