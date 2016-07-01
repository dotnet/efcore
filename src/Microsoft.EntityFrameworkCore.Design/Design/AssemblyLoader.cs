// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    public class AssemblyLoader
    {
        private readonly Func<AssemblyName, Assembly> _loadFunc;

        public AssemblyLoader()
            : this(Assembly.Load)
        {
        }

        public AssemblyLoader([NotNull] Func<AssemblyName, Assembly> loadFunc)
        {
            Check.NotNull(loadFunc, nameof(loadFunc));

            _loadFunc = loadFunc;
        }

        public virtual Assembly Load([NotNull] string assemblyName)
            => _loadFunc(new AssemblyName(assemblyName));
    }
}
