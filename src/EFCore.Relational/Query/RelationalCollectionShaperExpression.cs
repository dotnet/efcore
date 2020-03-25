// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalCollectionShaperExpression : Expression, IPrintableExpression
    {
        [Obsolete("Use ctor which takes value comaprers.")]
        public RelationalCollectionShaperExpression(
            int collectionId,
            [NotNull] Expression parentIdentifier,
            [NotNull] Expression outerIdentifier,
            [NotNull] Expression selfIdentifier,
            [NotNull] Expression innerShaper,
            [CanBeNull] INavigation navigation,
            [NotNull] Type elementType)
            : this(collectionId, parentIdentifier, outerIdentifier, selfIdentifier,
                  null, null, null, innerShaper, navigation, elementType)
        {
        }


        public RelationalCollectionShaperExpression(
            int collectionId,
            [NotNull] Expression parentIdentifier,
            [NotNull] Expression outerIdentifier,
            [NotNull] Expression selfIdentifier,
            [CanBeNull] IReadOnlyList<ValueComparer> parentIdentifierValueComparers,
            [CanBeNull] IReadOnlyList<ValueComparer> outerIdentifierValueComparers,
            [CanBeNull] IReadOnlyList<ValueComparer> selfIdentifierValueComparers,
            [NotNull] Expression innerShaper,
            [CanBeNull] INavigation navigation,
            [NotNull] Type elementType)
        {
            Check.NotNull(parentIdentifier, nameof(parentIdentifier));
            Check.NotNull(outerIdentifier, nameof(outerIdentifier));
            Check.NotNull(selfIdentifier, nameof(selfIdentifier));
            Check.NotNull(innerShaper, nameof(innerShaper));
            Check.NotNull(elementType, nameof(elementType));

            CollectionId = collectionId;
            ParentIdentifier = parentIdentifier;
            OuterIdentifier = outerIdentifier;
            SelfIdentifier = selfIdentifier;
            ParentIdentifierValueComparers = parentIdentifierValueComparers;
            OuterIdentifierValueComparers = outerIdentifierValueComparers;
            SelfIdentifierValueComparers = selfIdentifierValueComparers;
            InnerShaper = innerShaper;
            Navigation = navigation;
            ElementType = elementType;
        }

        public virtual int CollectionId { get; }
        public virtual Expression ParentIdentifier { get; }
        public virtual Expression OuterIdentifier { get; }
        public virtual Expression SelfIdentifier { get; }
        public virtual IReadOnlyList<ValueComparer> ParentIdentifierValueComparers { get; }
        public virtual IReadOnlyList<ValueComparer> OuterIdentifierValueComparers { get; }
        public virtual IReadOnlyList<ValueComparer> SelfIdentifierValueComparers { get; }

        public virtual Expression InnerShaper { get; }
        public virtual INavigation Navigation { get; }
        public virtual Type ElementType { get; }

        public override Type Type => Navigation?.ClrType ?? typeof(List<>).MakeGenericType(ElementType);
        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var parentIdentifier = visitor.Visit(ParentIdentifier);
            var outerIdentifier = visitor.Visit(OuterIdentifier);
            var selfIdentifier = visitor.Visit(SelfIdentifier);
            var innerShaper = visitor.Visit(InnerShaper);

            return Update(parentIdentifier, outerIdentifier, selfIdentifier, innerShaper);
        }

        public virtual RelationalCollectionShaperExpression Update(
            [NotNull] Expression parentIdentifier,
            [NotNull] Expression outerIdentifier,
            [NotNull] Expression selfIdentifier,
            [NotNull] Expression innerShaper)
        {
            Check.NotNull(parentIdentifier, nameof(parentIdentifier));
            Check.NotNull(outerIdentifier, nameof(outerIdentifier));
            Check.NotNull(selfIdentifier, nameof(selfIdentifier));
            Check.NotNull(innerShaper, nameof(innerShaper));

            return parentIdentifier != ParentIdentifier
                || outerIdentifier != OuterIdentifier
                || selfIdentifier != SelfIdentifier
                || innerShaper != InnerShaper
                    ? new RelationalCollectionShaperExpression(
                        CollectionId, parentIdentifier, outerIdentifier, selfIdentifier,
                        ParentIdentifierValueComparers, OuterIdentifierValueComparers, SelfIdentifierValueComparers,
                        innerShaper, Navigation, ElementType)
                    : this;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine("RelationalCollectionShaper:");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.AppendLine($"CollectionId: {CollectionId}");
                expressionPrinter.Append("ParentIdentifier:");
                expressionPrinter.Visit(ParentIdentifier);
                expressionPrinter.AppendLine();
                expressionPrinter.Append("OuterIdentifier:");
                expressionPrinter.Visit(OuterIdentifier);
                expressionPrinter.AppendLine();
                expressionPrinter.Append("SelfIdentifier:");
                expressionPrinter.Visit(SelfIdentifier);
                expressionPrinter.AppendLine();
                expressionPrinter.Append("InnerShaper:");
                expressionPrinter.Visit(InnerShaper);
                expressionPrinter.AppendLine();
                expressionPrinter.AppendLine($"Navigation: {Navigation?.Name}");
            }
        }
    }
}
