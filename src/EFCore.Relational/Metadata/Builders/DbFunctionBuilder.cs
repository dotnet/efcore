// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a <see cref="IMutableDbFunction" />.
    /// </summary>
    public class DbFunctionBuilder : IConventionDbFunctionBuilder
    {
        private readonly DbFunction _function;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public DbFunctionBuilder([NotNull] IMutableDbFunction function)
        {
            Check.NotNull(function, nameof(function));

            _function = (DbFunction)function;
        }

        /// <summary>
        ///     The function being configured.
        /// </summary>
        public virtual IMutableDbFunction Metadata => _function;

        /// <summary>
        ///     Sets the name of the database function.
        /// </summary>
        /// <param name="name"> The name of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilder HasName([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            _function.Name = name;

            return this;
        }

        /// <inheritdoc />
        IConventionDbFunctionBuilder IConventionDbFunctionBuilder.HasName(string name, bool fromDataAnnotation)
        {
            if (((IConventionDbFunctionBuilder)this).CanSetName(name, fromDataAnnotation))
            {
                ((IConventionDbFunction)_function).SetName(name, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <inheritdoc />
        bool IConventionDbFunctionBuilder.CanSetName(string name, bool fromDataAnnotation)
            => Overrides(fromDataAnnotation, _function.GetNameConfigurationSource())
                || _function.Name == name;

        /// <summary>
        ///     Sets the schema of the database function.
        /// </summary>
        /// <param name="schema"> The schema of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilder HasSchema([CanBeNull] string schema)
        {
            _function.Schema = schema;

            return this;
        }

        /// <inheritdoc />
        IConventionDbFunctionBuilder IConventionDbFunctionBuilder.HasSchema(string schema, bool fromDataAnnotation)
        {
            if (((IConventionDbFunctionBuilder)this).CanSetSchema(schema, fromDataAnnotation))
            {
                ((IConventionDbFunction)_function).SetSchema(schema, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <inheritdoc />
        bool IConventionDbFunctionBuilder.CanSetSchema(string schema, bool fromDataAnnotation)
            => Overrides(fromDataAnnotation, _function.GetSchemaConfigurationSource())
                || _function.Schema == schema;

        /// <summary>
        ///     Sets the store type of the database function.
        /// </summary>
        /// <param name="storeType"> The store type of the function in the database. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilder HasStoreType([CanBeNull] string storeType)
        {
            _function.StoreType = storeType;

            return this;
        }

        /// <inheritdoc />
        IConventionDbFunctionBuilder IConventionDbFunctionBuilder.HasStoreType(string storeType, bool fromDataAnnotation)
        {
            if (((IConventionDbFunctionBuilder)this).CanSetStoreType(storeType, fromDataAnnotation))
            {
                ((IConventionDbFunction)_function).SetStoreType(storeType, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <inheritdoc />
        bool IConventionDbFunctionBuilder.CanSetStoreType(string storeType, bool fromDataAnnotation)
            => Overrides(fromDataAnnotation, _function.GetStoreTypeConfigurationSource())
                || _function.StoreType == storeType;

        /// <inheritdoc />
        IConventionDbFunctionBuilder IConventionDbFunctionBuilder.HasTypeMapping(
            RelationalTypeMapping returnTypeMapping, bool fromDataAnnotation)
        {
            if (((IConventionDbFunctionBuilder)this).CanSetTypeMapping(returnTypeMapping, fromDataAnnotation))
            {
                ((IConventionDbFunction)_function).SetTypeMapping(returnTypeMapping, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <inheritdoc />
        bool IConventionDbFunctionBuilder.CanSetTypeMapping(RelationalTypeMapping returnTypeMapping, bool fromDataAnnotation)
            => Overrides(fromDataAnnotation, _function.GetTypeMappingConfigurationSource())
                || _function.TypeMapping == returnTypeMapping;

        /// <summary>
        ///     <para>
        ///         Sets a callback that will be invoked to perform custom translation of this
        ///         function. The callback takes a collection of expressions corresponding to
        ///         the parameters passed to the function call. The callback should return an
        ///         expression representing the desired translation.
        ///     </para>
        ///     <para>
        ///         See https://go.microsoft.com/fwlink/?linkid=852477 for more information.
        ///     </para>
        /// </summary>
        /// <param name="translation"> The translation to use. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual DbFunctionBuilder HasTranslation([NotNull] Func<IReadOnlyCollection<SqlExpression>, SqlExpression> translation)
        {
            Check.NotNull(translation, nameof(translation));

            _function.Translation = translation;

            return this;
        }

        /// <inheritdoc />
        IConventionDbFunction IConventionDbFunctionBuilder.Metadata => _function;

        /// <inheritdoc />
        IConventionDbFunctionBuilder IConventionDbFunctionBuilder.HasTranslation(
            Func<IReadOnlyCollection<SqlExpression>, SqlExpression> translation, bool fromDataAnnotation)
        {
            if (((IConventionDbFunctionBuilder)this).CanSetTranslation(translation, fromDataAnnotation))
            {
                ((IConventionDbFunction)_function).SetTranslation(translation, fromDataAnnotation);
                return this;
            }

            return null;
        }

        /// <summary>
        ///     Creates a <see cref="DbFunctionParameterBuilder" /> for a parameter with the given name.
        /// </summary>
        /// <param name="name"> The parameter name. </param>
        /// <returns> The builder to use for further parameter configuration. </returns>
        public virtual DbFunctionParameterBuilder HasParameter([NotNull] string name)
        {
            return new DbFunctionParameterBuilder((DbFunctionParameter)FindParameter(name));
        }

        private IDbFunctionParameter FindParameter(string name)
        {
            var parameter = Metadata.Parameters.SingleOrDefault(
                funcParam => string.Compare(funcParam.Name, name, StringComparison.OrdinalIgnoreCase) == 0);

            if (parameter == null)
            {
                throw new ArgumentException(
                    RelationalStrings.DbFunctionInvalidParameterName(name, Metadata.MethodInfo.DisplayName()));
            }

            return parameter;
        }

        /// <inheritdoc />
        bool IConventionDbFunctionBuilder.CanSetTranslation(
            Func<IReadOnlyCollection<SqlExpression>, SqlExpression> translation, bool fromDataAnnotation)
            => Overrides(fromDataAnnotation, _function.GetTranslationConfigurationSource())
                || _function.Translation == translation;

        private bool Overrides(bool fromDataAnnotation, ConfigurationSource? configurationSource)
            => (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                .Overrides(configurationSource);

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
