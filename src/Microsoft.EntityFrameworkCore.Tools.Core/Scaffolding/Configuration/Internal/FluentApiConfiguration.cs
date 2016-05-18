// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Configuration.Internal
{
    public class FluentApiConfiguration : IFluentApiConfiguration
    {
        public FluentApiConfiguration(
            [NotNull] string methodName, [CanBeNull] params string[] methodArguments)
        {
            Check.NotEmpty(methodName, nameof(methodName));

            MethodName = methodName;
            MethodArguments = methodArguments;
        }

        public virtual string MethodName { get; }
        public virtual string[] MethodArguments { get; }

        public virtual bool HasAttributeEquivalent { get; set; }

        public virtual string MethodBody
            => MethodArguments == null
                ? MethodName + "()"
                : MethodName + "(" + string.Join(", ", MethodArguments) + ")";

        public virtual ICollection<string> FluentApiLines
            => new List<string> { MethodBody };
    }
}
