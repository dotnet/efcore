// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class FluentApiConfiguration : IFluentApiConfiguration
    {
        private readonly string _methodName;
        private readonly string[] _methodArguments;

        public FluentApiConfiguration(
            [NotNull] string methodName, [CanBeNull] params string[] methodArguments)
        {
            Check.NotEmpty(methodName, nameof(methodName));

            _methodName = methodName;
            _methodArguments = methodArguments;
        }

        public virtual bool HasAttributeEquivalent { get; set; }

        public virtual string For { get;[param: NotNull] private set; }

        public virtual string MethodBody
        {
            get
            {
                return _methodArguments == null
                    ? _methodName + "()"
                    : _methodName + "(" + string.Join(", ", _methodArguments) + ")";
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
