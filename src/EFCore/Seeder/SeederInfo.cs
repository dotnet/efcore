// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Seeder.Attributes;

namespace Microsoft.EntityFrameworkCore.Seeder
{
    /// <summary>
    /// Holds seeders information
    /// </summary>
    public class SeederInfo
    {
        internal Type Type { get; }
        internal MethodInfo MethodInfo { get; }
        internal SeederAttribute SeederAttribute { get; }

        /// <summary>
        /// Creates a seeder information object
        /// </summary>
        /// <param name="type">The model that should seeds</param>
        /// <param name="methodInfo">The method that seeds the model</param>
        /// <param name="seederAttribute">The attribute that defines in seeder method.</param>
        public SeederInfo(Type type, MethodInfo methodInfo, SeederAttribute seederAttribute)
        {
            Type = type;
            MethodInfo = methodInfo;
            SeederAttribute = seederAttribute;
        }

        /// <summary>
        /// Determine a seeder is async or not
        /// </summary>
        /// <returns>true if seeder is an async method. false is seeder isn't an async method.</returns>
        internal bool IsAsync()
        {
            return MethodInfo.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
        }
    }
}
