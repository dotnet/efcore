// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Provides translations for LINQ <see cref="MemberExpression" /> expressions by dispatching to multiple specialized member
    ///         translators.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class RelationalMemberTranslatorProvider : IMemberTranslatorProvider
    {
        private readonly List<IMemberTranslator> _plugins = new();
        private readonly List<IMemberTranslator> _translators = new();

        /// <summary>
        ///     Creates a new instance of the <see cref="RelationalMemberTranslatorProvider" /> class.
        /// </summary>
        /// <param name="relationalDependencies"> Parameter object containing dependencies for this class. </param>
        public RelationalMemberTranslatorProvider(RelationalMemberTranslatorProviderDependencies relationalDependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
            
            _plugins.AddRange(relationalDependencies.Plugins.SelectMany(p => p.Translators));
            _translators
                .AddRange(
                    new[] { new NullableMemberTranslator(relationalDependencies.SqlExpressionFactory) });
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual RelationalMemberTranslatorProviderDependencies RelationalDependencies { get; }

        /// <inheritdoc />
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));
            Check.NotNull(logger, nameof(logger));

            return _plugins.Concat(_translators)
                .Select(t => t.Translate(instance, member, returnType, logger)).FirstOrDefault(t => t != null);
        }

        /// <summary>
        ///     Adds additional translators which will take priority over existing registered translators.
        /// </summary>
        /// <param name="translators"> Translators to add. </param>
        protected virtual void AddTranslators(IEnumerable<IMemberTranslator> translators)
        {
            Check.NotNull(translators, nameof(translators));

            _translators.InsertRange(0, translators);
        }
    }
}
