// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     A base composite member translator that dispatches to multiple specialized
    ///     member translators.
    /// </summary>
    public abstract class RelationalCompositeMemberTranslator : IMemberTranslator
    {
        private readonly List<IMemberTranslator> _plugins = new List<IMemberTranslator>();
        private readonly List<IMemberTranslator> _translators = new List<IMemberTranslator>();

        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected RelationalCompositeMemberTranslator([NotNull] RelationalCompositeMemberTranslatorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            _plugins.AddRange(dependencies.Plugins.SelectMany(p => p.Translators));
        }

        /// <summary>
        ///     Translates the given member expression.
        /// </summary>
        /// <param name="memberExpression"> The member expression. </param>
        /// <returns>
        ///     A SQL expression representing the translated MemberExpression.
        /// </returns>
        public virtual Expression Translate(MemberExpression memberExpression)
            => Enumerable.Concat(_plugins, _translators)
                .Select(translator => translator.Translate(memberExpression))
                .FirstOrDefault(translatedMember => translatedMember != null);

        /// <summary>
        ///     Adds additional translators to the dispatch list.
        /// </summary>
        /// <param name="translators"> The translators. </param>
        protected virtual void AddTranslators([NotNull] IEnumerable<IMemberTranslator> translators)
            => _translators.AddRange(translators);
    }
}
