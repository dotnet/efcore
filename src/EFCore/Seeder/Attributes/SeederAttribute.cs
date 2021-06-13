// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Seeder.Attributes
{
    /// <summary>
    /// Identifies the seeder method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class SeederAttribute : Attribute
    {
        /// <summary>
        /// Determines the model that should seed.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Determines seed priority
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Determines model should seed in Production enviroment
        /// </summary>
        public bool Production { get; }

        /// <summary>
        /// Force to seed even if model already has data
        /// </summary>
        public bool Force { get; }

        /// <summary>
        /// Creates a seed
        /// </summary>
        /// <param name="type">Determines the model that should seed.</param>
        /// <param name="priority">Determines seed priority</param>
        /// <param name="production">Determines model should seed in Production enviroment</param>
        /// <param name="force">Force to seed even if model already has data</param>
        public SeederAttribute(Type type, int priority, bool production = false, bool force = false)
        {
            Type = type;
            Priority = priority;
            Production = production;
            Force = force;
        }
    }
}
