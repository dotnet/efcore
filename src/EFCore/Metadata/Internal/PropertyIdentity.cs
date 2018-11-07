// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public readonly struct PropertyIdentity
    {
        private readonly object _nameOrProperty;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public PropertyIdentity([NotNull] string name)
            : this((object)name)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public PropertyIdentity([NotNull] MemberInfo property)
            : this((object)property)
        {
        }

        [DebuggerStepThrough]
        private PropertyIdentity([CanBeNull] object nameOrProperty)
        {
            _nameOrProperty = nameOrProperty;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool IsNone() => _nameOrProperty == null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly PropertyIdentity None = new PropertyIdentity((object)null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public static PropertyIdentity Create([CanBeNull] string name)
            => name == null ? None : new PropertyIdentity(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public static PropertyIdentity Create([CanBeNull] MemberInfo property)
            => property == null ? None : new PropertyIdentity(property);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyIdentity Create([CanBeNull] Navigation navigation)
            => navigation?.GetIdentifyingMemberInfo() == null
                ? Create(navigation?.Name)
                : Create(navigation.GetIdentifyingMemberInfo());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Name
        {
            [DebuggerStepThrough] get => Property?.GetSimpleMemberName() ?? (string)_nameOrProperty;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MemberInfo Property
        {
            [DebuggerStepThrough] get => _nameOrProperty as MemberInfo;
        }

        private string DebuggerDisplay()
            => Name ?? "NONE";
    }
}
