// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
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
        /// <param name="convertToStoreExpression"> An expression to convert objects when writing data to the store. </param>
        /// <param name="convertFromStoreExpression"> An expression to convert objects when reading data from the store. </param>
        public ValueConverter(
            [NotNull] Expression<Func<TModel, TStore>> convertToStoreExpression,
            [NotNull] Expression<Func<TStore, TModel>> convertFromStoreExpression)
            : base(
                SanitizeConverter(Check.NotNull(convertToStoreExpression, nameof(convertToStoreExpression))),
                SanitizeConverter(Check.NotNull(convertFromStoreExpression, nameof(convertFromStoreExpression))),
                convertToStoreExpression,
                convertFromStoreExpression)
        {
        }

        private static Func<object, object> SanitizeConverter<TIn, TOut>(Expression<Func<TIn, TOut>> convertExpression)
        {
            var compiled = convertExpression.Compile();

            return typeof(TIn).IsNullableType()
                ? (Func<object, object>)(v => compiled(SanitizeNullable<TIn>(v)))
                : (v => v == null ? (object)null : compiled(SanitizeNonNullable<TIn>(v)));
        }

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
        ///     Gets the expression to convert objects when writing data to the store,
        ///     exactly as supplied and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public new virtual Expression<Func<TModel, TStore>> ConvertToStoreExpression
            => (Expression<Func<TModel, TStore>>)base.ConvertToStoreExpression;

        /// <summary>
        ///     Gets the expression to convert objects when reading data from the store,
        ///     exactly as supplied and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public new virtual Expression<Func<TStore, TModel>> ConvertFromStoreExpression
            => (Expression<Func<TStore, TModel>>)base.ConvertFromStoreExpression;

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
