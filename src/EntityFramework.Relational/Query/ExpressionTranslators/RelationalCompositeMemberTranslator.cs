// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators
{
    public abstract class RelationalCompositeMemberTranslator : IMemberTranslator
    {
        private readonly List<IMemberTranslator> _translators = new List<IMemberTranslator>();

        public virtual Expression Translate(MemberExpression expression)
        {
            foreach (var translator in _translators)
            {
                var translatedMember = translator.Translate(expression);
                if (translatedMember != null)
                {
                    return translatedMember;
                }
            }

            return null;
        }

        protected virtual void AddTranslators([NotNull] IEnumerable<IMemberTranslator> translators)
        {
            _translators.AddRange(translators);
        }
    }
}
