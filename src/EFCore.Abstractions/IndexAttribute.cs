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
        private static readonly bool DefaultIsUnique = false;
        private bool? _isUnique;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexAttribute" /> class.
        /// </summary>
        /// <param name="propertyNames"> The properties which constitute the index, in order (there must be at least one). </param>
        public IndexAttribute(params string[] propertyNames)
        {
            Check.NotEmpty(propertyNames, nameof(propertyNames));
            Check.HasNoEmptyElements(propertyNames, nameof(propertyNames));
            PropertyNames = propertyNames.ToList();
        }

        /// <summary>
        ///     The properties which constitute the index, in order.
        /// </summary>
        public List<string> PropertyNames { get; }

        /// <summary>
        ///     The name of the index.
        /// </summary>
        public string Name { get; [param: NotNull] set; }


        /// <summary>
        ///     Whether the index is unique.
        /// </summary>
        public bool IsUnique
        {
            get => _isUnique ?? DefaultIsUnique;
            set => _isUnique = value;
        }

        /// <summary>
        ///     Use this method if you want to know the uniqueness of
        ///     the index or <see langword="null"/> if it was not specified.
        /// </summary>
        public bool? GetIsUnique() => _isUnique;
    }
}
