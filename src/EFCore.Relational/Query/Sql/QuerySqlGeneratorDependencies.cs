// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="QuerySqlGeneratorFactoryBase" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>.
    ///         This means a single instance of each service is used by many <see cref="DbContext"/> instances.
    ///         The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public sealed class QuerySqlGeneratorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="QuerySqlGeneratorFactoryBase" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        /// </summary>
        /// <param name="sqlGenerationHelper"> The SQL generation helper. </param>
        /// <param name="parameterNameGeneratorFactory"> The parameter name generator factory. </param>
        /// <param name="typeMappingSource"> The type mapper. </param>
        public QuerySqlGeneratorDependencies(
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IRelationalTypeMappingSource typeMappingSource)
        {
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));

            SqlGenerationHelper = sqlGenerationHelper;
            ParameterNameGeneratorFactory = parameterNameGeneratorFactory;
            TypeMappingSource = typeMappingSource;
        }

        /// <summary>
        ///     Gets the SQL generation helper.
        /// </summary>
        public ISqlGenerationHelper SqlGenerationHelper { get; }

        /// <summary>
        ///     Gets the parameter name generator factory.
        /// </summary>
        public IParameterNameGeneratorFactory ParameterNameGeneratorFactory { get; }

        /// <summary>
        ///     The type mapping source.
        /// </summary>
        public IRelationalTypeMappingSource TypeMappingSource { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="sqlGenerationHelper"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QuerySqlGeneratorDependencies With([NotNull] ISqlGenerationHelper sqlGenerationHelper)
            => new QuerySqlGeneratorDependencies(
                sqlGenerationHelper,
                ParameterNameGeneratorFactory,
                TypeMappingSource);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="parameterNameGeneratorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QuerySqlGeneratorDependencies With([NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory)
            => new QuerySqlGeneratorDependencies(
                SqlGenerationHelper,
                parameterNameGeneratorFactory,
                TypeMappingSource);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="typeMappingSource"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QuerySqlGeneratorDependencies With([NotNull] IRelationalTypeMappingSource typeMappingSource)
            => new QuerySqlGeneratorDependencies(
                SqlGenerationHelper,
                ParameterNameGeneratorFactory,
                typeMappingSource);
    }
}
