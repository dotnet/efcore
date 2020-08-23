// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Specifies an index to be generated in the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IndexAttribute : Attribute
    {
        private bool? _isUnique;
        private string _name;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IndexAttribute" /> class.
        /// </summary>
        /// <param name="propertyNames"> The properties which constitute the index, in order (there must be at least one). </param>
        public IndexAttribute([CanBeNull] params string[] propertyNames)
        {
            Check.NotEmpty(propertyNames, nameof(propertyNames));
            Check.HasNoEmptyElements(propertyNames, nameof(propertyNames));

            PropertyNames = propertyNames.ToList();
        }

        /// <summary>
        ///     The properties which constitute the index, in order.
        /// </summary>
        public IReadOnlyList<string> PropertyNames { get; }

        /// <summary>
        ///     The name of the index.
        /// </summary>
        public string Name
        {
            get => _name;
            [param: NotNull] set => _name = Check.NotNull(value, nameof(value));
        }

        /// <summary>
        ///     Whether the index is unique.
        /// </summary>
        public bool IsUnique
        {
            get => _isUnique ?? false;
            set => _isUnique = value;
        }

        /// <summary>
        ///     Checks whether <see cref="IsUnique" /> has been explicitly set to a value.
        /// </summary>
        public bool IsUniqueHasValue
            => _isUnique.HasValue;
    }
}
