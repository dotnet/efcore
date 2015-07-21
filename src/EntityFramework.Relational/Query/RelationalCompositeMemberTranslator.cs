// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Query.Methods;

namespace Microsoft.Data.Entity.Query
{
    public abstract class RelationalCompositeMemberTranslator : IMemberTranslator
    {
        private readonly List<IMemberTranslator> _relationalTranslators = new List<IMemberTranslator>();

        public virtual Expression Translate(MemberExpression expression)
        {
            foreach (var translator in Translators)
            {
                var translatedMember = translator.Translate(expression);
                if (translatedMember != null)
                {
                    return translatedMember;
                }
            }

            return null;
        }

        protected virtual IReadOnlyList<IMemberTranslator> Translators => _relationalTranslators;
    }
}
