// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     A base composite member translator that dispatches to multiple specialized
    ///     member translators.
    /// </summary>
    public abstract class RelationalCompositeMemberTranslator : IMemberTranslator
    {
        private readonly List<IMemberTranslator> _translators = new List<IMemberTranslator>();

        /// <summary>
        ///     Translates the given member expression.
        /// </summary>
        /// <param name="memberExpression"> The member expression. </param>
        /// <returns>
        ///     A SQL expression representing the translated MemberExpression.
        /// </returns>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var translator in _translators)
            {
                var translatedMember = translator.Translate(memberExpression);
                if (translatedMember != null)
                {
                    return translatedMember;
                }
            }

            return null;
        }

        /// <summary>
        ///     Adds additional translators to the dispatch list.
        /// </summary>
        /// <param name="translators"> The translators. </param>
        protected virtual void AddTranslators([NotNull] IEnumerable<IMemberTranslator> translators)
            => _translators.AddRange(translators);
    }
}
