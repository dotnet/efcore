// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal.Configuration
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

        public virtual string For { get;[param: NotNull] private set; }

        public virtual string MethodBody
        {
            get
            {
                return MethodArguments == null
                    ? MethodName + "()"
                    : MethodName + "(" + string.Join(", ", MethodArguments) + ")";
            }
        }

        public virtual string FluentApi{
            get
            {
                return For == null
                    ? MethodBody
                    : For + "()." + MethodBody;
            }
        }
    }
}
