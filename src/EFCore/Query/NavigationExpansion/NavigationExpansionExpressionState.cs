// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public class NavigationExpansionExpressionState
    {
        public NavigationExpansionExpressionState(
            ParameterExpression currentParameter,
            List<SourceMapping> sourceMappings,
            LambdaExpression pendingSelector,
            bool applyPendingSelector,
            List<(MethodInfo method, LambdaExpression keySelector)> pendingOrderings,
            NavigationBindingExpression pendingIncludeChain,
            MethodInfo pendingCardinalityReducingOperator,
            List<string> pendingTags,
            List<List<string>> customRootMappings,
            INavigation materializeCollectionNavigation)
        {
            CurrentParameter = currentParameter;
            SourceMappings = sourceMappings;
            PendingSelector = pendingSelector;
            ApplyPendingSelector = applyPendingSelector;
            PendingOrderings = pendingOrderings;
            PendingIncludeChain = pendingIncludeChain;
            PendingCardinalityReducingOperator = pendingCardinalityReducingOperator;
            PendingTags = pendingTags;
            CustomRootMappings = customRootMappings;
            MaterializeCollectionNavigation = materializeCollectionNavigation;
        }

        public virtual ParameterExpression CurrentParameter { get; set; }
        public virtual List<SourceMapping> SourceMappings { get; set; }
        public virtual LambdaExpression PendingSelector { get; set; }
        public virtual bool ApplyPendingSelector { get; set; }
        public virtual List<(MethodInfo method, LambdaExpression keySelector)> PendingOrderings { get; set; }
        public virtual NavigationBindingExpression PendingIncludeChain { get; set; }
        public virtual MethodInfo PendingCardinalityReducingOperator { get; set; }
        public virtual List<string> PendingTags { get; set; }
        public virtual List<List<string>> CustomRootMappings { get; set; }
        public virtual INavigation MaterializeCollectionNavigation { get; set; }
    }
}
