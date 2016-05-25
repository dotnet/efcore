// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public struct PropertyIdentity
    {
        private readonly object _nameOrProperty;

        public PropertyIdentity([NotNull] string name)
            : this((object)name)
        {
        }

        public PropertyIdentity([NotNull] PropertyInfo property)
            : this((object)property)
        {
        }

        private PropertyIdentity([CanBeNull] object nameOrProperty)
        {
            _nameOrProperty = nameOrProperty;
        }

        public bool IsNone() => _nameOrProperty == null;

        public static readonly PropertyIdentity None = new PropertyIdentity((object)null);

        public static PropertyIdentity Create([CanBeNull] string name)
            => name == null ? None : new PropertyIdentity(name);

        public static PropertyIdentity Create([CanBeNull] PropertyInfo property)
            => property == null ? None : new PropertyIdentity(property);

        public static PropertyIdentity Create([CanBeNull] Navigation navigation)
            => navigation?.PropertyInfo == null ? Create(navigation?.Name) : Create(navigation.PropertyInfo);

        public string Name => Property?.Name ?? (string)_nameOrProperty;

        public PropertyInfo Property => _nameOrProperty as PropertyInfo;

        private string DebuggerDisplay()
            => Name ?? "NONE";
    }
}
