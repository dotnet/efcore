// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Relational.Design.FunctionalTests.Compilation
{
    public class CompiledAssemblyResult
    {
        public virtual bool Success { get; set; }

        public virtual Assembly Assembly { get; set; }

        public virtual IEnumerable<string> ErrorMessages { get; set; }

        public static CompiledAssemblyResult FromAssembly(Assembly assembly)
        {
            return new CompiledAssemblyResult
            {
                Assembly = assembly,
                Success = true
            };
        }

        public static CompiledAssemblyResult FromErrorMessages(IEnumerable<string> errorMessages)
        {
            return new CompiledAssemblyResult
            {
                ErrorMessages = errorMessages,
                Success = false
            };
        }
    }
}
