// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Defines conversions from an object of one type in a model to an object of the same or
    ///     different type in the store.
    /// </summary>
    public class TypeConverter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeConverter" /> class.
        /// </summary>
        /// <param name="convertToStore"> The function to convert objects when writing data to the store. </param>
        /// <param name="convertFromStore"> The function to convert objects when reading data from the store. </param>
        // TODO.TM Generic, non-boxing delegates
        public TypeConverter(
            [NotNull] Func<object, object> convertToStore,
            [NotNull] Func<object, object> convertFromStore)
        {
            Check.NotNull(convertToStore, nameof(convertToStore));
            Check.NotNull(convertFromStore, nameof(convertFromStore));

            ConvertToStore = convertToStore;
            ConvertFromStore = convertFromStore;
        }

        /// <summary>
        ///     Gets the function to convert objects when writing data to the store.
        /// </summary>
        public virtual Func<object, object> ConvertToStore { get; }

        /// <summary>
        ///     Gets the function to convert objects when reading data from the store.
        /// </summary>
        public virtual Func<object, object> ConvertFromStore { get; }
    }
}
