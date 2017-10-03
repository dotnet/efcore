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
    public abstract class TypeConverter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeConverter" /> class.
        /// </summary>
        /// <param name="convertToStore">
        ///     The function to convert objects when writing data to the store,
        ///     setup to handle nulls, boxing, and non-exact matches of simple types.
        /// </param>
        /// <param name="convertFromStore">
        ///     The function to convert objects when reading data from the store,
        ///     setup to handle nulls, boxing, and non-exact matches of simple types.
        /// </param>
        /// <param name="rawConvertToStore">
        ///     The function to convert objects when writing data to the store,
        ///     exactly as supplied, which may be a generic delegate and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </param>
        /// <param name="rawConvertFromStore">
        ///     The function to convert objects when reading data from the store,
        ///     exactly as supplied, which may be a generic delegate and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </param>
        protected TypeConverter(
            [NotNull] Func<object, object> convertToStore,
            [NotNull] Func<object, object> convertFromStore,
            [NotNull] Delegate rawConvertToStore,
            [NotNull] Delegate rawConvertFromStore)

        {
            Check.NotNull(convertToStore, nameof(convertToStore));
            Check.NotNull(convertFromStore, nameof(convertFromStore));
            Check.NotNull(rawConvertToStore, nameof(rawConvertToStore));
            Check.NotNull(rawConvertFromStore, nameof(rawConvertFromStore));

            ConvertToStore = convertToStore;
            ConvertFromStore = convertFromStore;
            RawConvertToStore = rawConvertToStore;
            RawConvertFromStore = rawConvertFromStore;
        }

        /// <summary>
        ///     Gets the function to convert objects when writing data to the store,
        ///     setup to handle nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public virtual Func<object, object> ConvertToStore { get; }

        /// <summary>
        ///     Gets the function to convert objects when reading data from the store,
        ///     setup to handle nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public virtual Func<object, object> ConvertFromStore { get; }

        /// <summary>
        ///     Gets the function to convert objects when writing data to the store,
        ///     exactly as supplied, which may be a generic delegate and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public virtual Delegate RawConvertToStore { get; }

        /// <summary>
        ///     Gets the function to convert objects when reading data from the store,
        ///     exactly as supplied, which may be a generic delegate and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public virtual Delegate RawConvertFromStore { get; }

        /// <summary>
        ///     The CLR type used in the EF model.
        /// </summary>
        public abstract Type ModelType { get; }

        /// <summary>
        ///     The CLR type used when reading and writing from the store.
        /// </summary>
        public abstract Type StoreType { get; }
    }
}
