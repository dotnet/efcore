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
    public class ValueConverter<TModel, TStore> : ValueConverter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueConverter{TModel,TStore}" /> class.
        /// </summary>
        /// <param name="convertToStore"> The function to convert objects when writing data to the store. </param>
        /// <param name="convertFromStore"> The function to convert objects when reading data from the store. </param>
        public ValueConverter(
            [NotNull] Func<TModel, TStore> convertToStore,
            [NotNull] Func<TStore, TModel> convertFromStore)
            : base(
                SanitizeConverter(Check.NotNull(convertToStore, nameof(convertToStore))),
                SanitizeConverter(Check.NotNull(convertFromStore, nameof(convertFromStore))),
                convertToStore,
                convertFromStore)
        {
        }

        private static Func<object, object> SanitizeConverter<TIn, TOut>(Func<TIn, TOut> convertToStore)
            => typeof(TIn).IsNullableType()
                ? (Func<object, object>)(v => convertToStore(SanitizeNullable<TIn>(v)))
                : (v => v == null ? (object)null : convertToStore(SanitizeNonNullable<TIn>(v)));

        private static T SanitizeNullable<T>(object value)
        {
            var unwrappedType = typeof(T).UnwrapNullableType();

            return value == null
                ? (T)value
                : (T)(unwrappedType != value.GetType()
                    ? Convert.ChangeType(value, unwrappedType)
                    : value);
        }

        private static T SanitizeNonNullable<T>(object value)
            => (T)(typeof(T) != value.GetType()
                ? Convert.ChangeType(value, typeof(T))
                : value);

        /// <summary>
        ///     Gets the function to convert objects when writing data to the store,
        ///     exactly as supplied, which may be a generic delegate and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public new virtual Func<TModel, TStore> RawConvertToStore
            => (Func<TModel, TStore>)base.RawConvertToStore;

        /// <summary>
        ///     Gets the function to convert objects when reading data from the store,
        ///     exactly as supplied, which may be a generic delegate and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public new virtual Func<TStore, TModel> RawConvertFromStore
            => (Func<TStore, TModel>)base.RawConvertFromStore;

        /// <summary>
        ///     The CLR type used in the EF model.
        /// </summary>
        public override Type ModelType => typeof(TModel);

        /// <summary>
        ///     The CLR type used when reading and writing from the store.
        /// </summary>
        public override Type StoreType => typeof(TStore);
    }
}
