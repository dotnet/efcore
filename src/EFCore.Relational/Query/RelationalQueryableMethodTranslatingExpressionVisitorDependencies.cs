// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="RelationalQueryableMethodTranslatingExpressionVisitor" />
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
    public sealed record RelationalQueryableMethodTranslatingExpressionVisitorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="RelationalQueryableMethodTranslatingExpressionVisitor" />.
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
        public RelationalQueryableMethodTranslatingExpressionVisitorDependencies(
            IRelationalSqlTranslatingExpressionVisitorFactory relationalSqlTranslatingExpressionVisitorFactory,
            ISqlExpressionFactory sqlExpressionFactory,
            IRelationalSharedTypeEntityExpansionHelper relationalSharedTypeEntityExpansionHelper)
        {
            Check.NotNull(relationalSqlTranslatingExpressionVisitorFactory, nameof(relationalSqlTranslatingExpressionVisitorFactory));
            Check.NotNull(sqlExpressionFactory, nameof(sqlExpressionFactory));
            Check.NotNull(relationalSharedTypeEntityExpansionHelper, nameof(relationalSharedTypeEntityExpansionHelper));

            RelationalSqlTranslatingExpressionVisitorFactory = relationalSqlTranslatingExpressionVisitorFactory;
            SqlExpressionFactory = sqlExpressionFactory;
            RelationalSharedTypeEntityExpansionHelper = relationalSharedTypeEntityExpansionHelper;
        }

        /// <summary>
        ///     The SQL-translating expression visitor factory.
        /// </summary>
        public IRelationalSqlTranslatingExpressionVisitorFactory RelationalSqlTranslatingExpressionVisitorFactory { get; init; }

        /// <summary>
        ///     The SQL expression factory.
        /// </summary>
        public ISqlExpressionFactory SqlExpressionFactory { get; init; }

        /// <summary>
        ///     Shared type entity expansion helper.
        /// </summary>
        [EntityFrameworkInternal]
        public IRelationalSharedTypeEntityExpansionHelper RelationalSharedTypeEntityExpansionHelper { get; init; }
    }
}
