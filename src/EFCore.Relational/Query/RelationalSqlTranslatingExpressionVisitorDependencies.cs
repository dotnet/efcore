// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="RelationalSqlTranslatingExpressionVisitorFactory" />
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
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed record RelationalSqlTranslatingExpressionVisitorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="RelationalSqlTranslatingExpressionVisitorFactory" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public RelationalSqlTranslatingExpressionVisitorDependencies(
            ISqlExpressionFactory sqlExpressionFactory,
            IModel model,
            IRelationalTypeMappingSource typeMappingSource,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            Check.NotNull(sqlExpressionFactory, nameof(sqlExpressionFactory));
            Check.NotNull(model, nameof(model));
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(memberTranslatorProvider, nameof(memberTranslatorProvider));
            Check.NotNull(methodCallTranslatorProvider, nameof(methodCallTranslatorProvider));

            SqlExpressionFactory = sqlExpressionFactory;
            Model = model;
#pragma warning disable CS0618 // Type or member is obsolete
            TypeMappingSource = typeMappingSource;
#pragma warning restore CS0618 // Type or member is obsolete
            MemberTranslatorProvider = memberTranslatorProvider;
            MethodCallTranslatorProvider = methodCallTranslatorProvider;
        }

        /// <summary>
        ///     The expression factory.
        /// </summary>
        public ISqlExpressionFactory SqlExpressionFactory { get; init; }

        /// <summary>
        ///     The expression factory.
        /// </summary>
        public IModel Model { get; init; }

        /// <summary>
        ///     The relational type mapping souce.
        /// </summary>
        public IRelationalTypeMappingSource TypeMappingSource { get; init; }

        /// <summary>
        ///     The member translation provider.
        /// </summary>
        public IMemberTranslatorProvider MemberTranslatorProvider { get; init; }

        /// <summary>
        ///     The method-call translation provider.
        /// </summary>
        public IMethodCallTranslatorProvider MethodCallTranslatorProvider { get; init; }
    }
}
