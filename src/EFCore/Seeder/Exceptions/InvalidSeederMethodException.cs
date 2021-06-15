// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Seeder.Exceptions
{
    /// <summary>
    /// The exception that throws when SeederAttribute used in invalid method.
    /// </summary>
    public class InvalidSeederMethodException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message.</summary>
        /// <param name="seederMethod">The method that has SeederAttribute</param>
        public InvalidSeederMethodException(MemberInfo seederMethod)
            : base($"{seederMethod.Name} is not a valid seeder. The Seeder should be a public non-static method of a public non-static class.")
        {
        }
    }
}
