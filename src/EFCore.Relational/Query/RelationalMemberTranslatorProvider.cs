// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalMemberTranslatorProvider : IMemberTranslatorProvider
    {
        private readonly List<IMemberTranslator> _plugins = new List<IMemberTranslator>();
        private readonly List<IMemberTranslator> _translators = new List<IMemberTranslator>();

        public RelationalMemberTranslatorProvider(RelationalMemberTranslatorProviderDependencies dependencies)
        {
            _plugins.AddRange(dependencies.Plugins.SelectMany(p => p.Translators));
            _translators
                .AddRange(
                    new[] { new NullableMemberTranslator(dependencies.SqlExpressionFactory) });
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            return _plugins.Concat(_translators)
                .Select(t => t.Translate(instance, member, returnType)).FirstOrDefault(t => t != null);
        }

        protected virtual void AddTranslators(IEnumerable<IMemberTranslator> translators)
            => _translators.InsertRange(0, translators);
    }
}
