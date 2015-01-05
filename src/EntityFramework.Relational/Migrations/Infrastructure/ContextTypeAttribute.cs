// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ContextTypeAttribute : Attribute
    {
        private readonly Type _contextType;

        public ContextTypeAttribute([NotNull] Type contextType)
        {
            Check.NotNull(contextType, "contextType");

            _contextType = contextType;
        }

        public virtual Type ContextType
        {
            get { return _contextType; }
        }
    }
}
