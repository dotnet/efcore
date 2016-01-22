// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    public abstract class RelationalCompositeMemberTranslator : IMemberTranslator
    {
        private readonly List<IMemberTranslator> _translators = new List<IMemberTranslator>();

        public virtual Expression Translate(MemberExpression memberExpression)
        {
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

        protected virtual void AddTranslators([NotNull] IEnumerable<IMemberTranslator> translators)
        {
            _translators.AddRange(translators);
        }
    }
}
