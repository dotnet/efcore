// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public struct TypeIdentity
    {
        private readonly object _nameOrType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public TypeIdentity([NotNull] string name)
            : this((object)name)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public TypeIdentity([NotNull] Type type)
            : this((object)type)
        {
        }

        [DebuggerStepThrough]
        private TypeIdentity(object nameOrType)
        {
            _nameOrType = nameOrType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Name
        {
            [DebuggerStepThrough] get { return Type?.DisplayName() ?? (string)_nameOrType; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Type Type
        {
            [DebuggerStepThrough] get { return _nameOrType as Type; }
        }

        private string DebuggerDisplay() => Name;
    }
}
