// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query
{
#if DEBUG
    public class ExpressionPrinter
    {
        public static string Print([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var visitor = new ExpressionPrintingVisitor();
            var nodePrinter = new TreeNodePrinter();
            var node = visitor.BuildTreeNode(expression);

            return nodePrinter.Print(node);
        }

        public class TreeNode
        {
            private readonly List<TreeNode> _children = new List<TreeNode>();

            public TreeNode()
            {
                Text = new StringBuilder();
            }

            public TreeNode([CanBeNull] string text, [NotNull] params TreeNode[] children)
            {
                Text = string.IsNullOrEmpty(text) ? new StringBuilder() : new StringBuilder(text);

                if (children != null)
                {
                    _children.AddRange(children);
                }
            }

            public TreeNode([CanBeNull] string text, [CanBeNull] List<TreeNode> children)
                : this(text)
            {
                if (children != null)
                {
                    _children.AddRange(children);
                }
            }

            public virtual StringBuilder Text { get; }

            public virtual IList<TreeNode> Children => _children;

            public virtual int Position { get; set; }
        }

        internal class TreeNodePrinter
        {
            private readonly List<TreeNode> _scopes = new List<TreeNode>();
            private const char Horizontals = '_';
            private const char Verticals = '|';

            public virtual string Print(TreeNode node)
            {
                var text = new StringBuilder();
                PrintNode(text, node);

                return text.ToString();
            }

            public virtual void PrintNode(StringBuilder text, TreeNode node)
            {
                IndentLine(text);
                text.Append(node.Text);
                PrintChildren(text, node);
            }

            public virtual void PrintChildren(StringBuilder text, TreeNode node)
            {
                _scopes.Add(node);
                node.Position = 0;
                foreach (var childNode in node.Children)
                {
                    text.AppendLine();
                    node.Position++;
                    PrintNode(text, childNode);
                }

                _scopes.RemoveAt(_scopes.Count - 1);
            }

            public void IndentLine(StringBuilder text)
            {
                var idx = 0;
                for (var scopeIdx = 0; scopeIdx < _scopes.Count; scopeIdx++)
                {
                    var parentScope = _scopes[scopeIdx];
                    if (parentScope.Position == parentScope.Children.Count
                        && scopeIdx != _scopes.Count - 1)
                    {
                        text.Append(' ');
                    }
                    else
                    {
                        text.Append(Verticals);
                    }

                    idx++;
                    if (_scopes.Count == idx)
                    {
                        text.Append(Horizontals);
                    }
                    else
                    {
                        text.Append(' ');
                    }
                }
            }
        }

        internal class ExpressionPrintingVisitor : ExpressionVisitor
        {
            private TreeNode _rootNode;

            public TreeNode BuildTreeNode(Expression expression)
            {
                Visit(expression);

                return _rootNode;
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                Visit(node.Operand);

                _rootNode = new TreeNode(ExpressionType.Convert.ToString(), _rootNode);

                return node;
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                Visit(node.Left);
                var left = _rootNode;

                Visit(node.Right);
                var right = _rootNode;

                _rootNode = new TreeNode(node.Method.Name, left, right);

                return node;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                _rootNode = new TreeNode(node.ToString());

                return node;
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                Visit(node.Body);

                _rootNode = new TreeNode("Lambda", _rootNode);

                // TODO: handle parameters
                return node;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                Visit(node.Expression);
                _rootNode = new TreeNode("." + node.Member.Name, _rootNode);

                return node;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var argumentNodes = new List<TreeNode>();

                foreach (var argument in node.Arguments)
                {
                    Visit(argument);
                    argumentNodes.Add(_rootNode);
                }

                var parameters = string.Join(", ", node.Method.GetParameters().Select(p => PrintType(p.ParameterType)));
                var methodSignature = PrintType(node.Method.ReturnType) + " " + node.Method.Name + "(" + parameters + ")";

                _rootNode = new TreeNode("MethodCall: " + methodSignature, argumentNodes);

                return node;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                _rootNode = new TreeNode("Parameter(Name='" + node.Name + "' Type='" + node.Type + "')");

                return node;
            }

            protected override Expression VisitExtension(Expression node)
            {
                var querySourceExpression = node as QuerySourceReferenceExpression;

                if (querySourceExpression != null)
                {
                    _rootNode = new TreeNode("QuerySource(" + querySourceExpression.ReferencedQuerySource + ")");
                }

                return node;
            }

            private string PrintType(Type type)
            {
                return type.IsConstructedGenericType
                    ? type.Name.Substring(0, type.Name.IndexOf('`')) + "<" + string.Join(", ", type.GenericTypeArguments.Select(PrintType)) + ">"
                    : type.Name;
            }
        }
    }
#endif
}
