// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class IncludeQueryResultAsserter
    {
        private readonly MethodInfo _assertElementMethodInfo;
        private readonly MethodInfo _assertCollectionMethodInfo;
        private readonly Dictionary<Type, object> _entitySorters;
        private readonly Dictionary<Type, object> _entityAsserters;

        private List<string> _path;
        private Stack<string> _fullPath;

        public IncludeQueryResultAsserter(
            Dictionary<Type, object> entitySorters,
            Dictionary<Type, object> entityAsserters)
        {
            _entitySorters = entitySorters ?? new Dictionary<Type, object>();
            _entityAsserters = entityAsserters ?? new Dictionary<Type, object>();

            _assertElementMethodInfo = typeof(IncludeQueryResultAsserter).GetTypeInfo().GetDeclaredMethod(nameof(AssertElement));
            _assertCollectionMethodInfo = typeof(IncludeQueryResultAsserter).GetTypeInfo().GetDeclaredMethod(nameof(AssertCollection));
        }

        public virtual void AssertResult(object expected, object actual, IEnumerable<IExpectedInclude> expectedIncludes)
        {
            _path = new List<string>();
            _fullPath = new Stack<string>();
            _fullPath.Push("root");

            AssertObject(expected, actual, expectedIncludes);
        }

        protected virtual void AssertObject(object expected, object actual, IEnumerable<IExpectedInclude> expectedIncludes)
        {
            if (expected == null
                && actual == null)
            {
                return;
            }

            Assert.Equal(expected == null, actual == null);

            var expectedType = expected.GetType();
            if (expectedType.GetTypeInfo().IsGenericType
                && expectedType.GetTypeInfo().ImplementedInterfaces.Any(
                    i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                var typeArgument = expectedType.GenericTypeArguments[0];
                var assertCollectionMethodInfo = _assertCollectionMethodInfo.MakeGenericMethod(typeArgument);
                assertCollectionMethodInfo.Invoke(this, new[] { expected, actual, expectedIncludes });
            }
            else
            {
                var assertElementMethodInfo = _assertElementMethodInfo.MakeGenericMethod(expectedType);
                assertElementMethodInfo.Invoke(this, new[] { expected, actual, expectedIncludes });
            }
        }

        protected virtual void AssertElement<TElement>(TElement expected, TElement actual, IEnumerable<IExpectedInclude> expectedIncludes)
        {
            if (expected == null
                && actual == null)
            {
                return;
            }

            Assert.Equal(expected == null, actual == null);

            var expectedType = expected.GetType();

            Assert.Equal(expectedType, actual.GetType());

            if (_entityAsserters.TryGetValue(expectedType, out var asserter))
            {
                ((Action<dynamic, dynamic>)asserter)(expected, actual);
                //asserter(expected, actual);
                ProcessIncludes(expected, actual, expectedIncludes);

                return;
            }

            var expectedTypeInfo = expectedType.GetTypeInfo();
            if (expectedTypeInfo.IsGenericType
                && expectedTypeInfo.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var keyPropertyInfo = expectedTypeInfo.GetDeclaredProperty("Key");
                var expectedKeyProperty = keyPropertyInfo.GetValue(expected);
                var actualKeyProperty = keyPropertyInfo.GetValue(actual);

                AssertObject(expectedKeyProperty, actualKeyProperty, expectedIncludes);

                var valuePropertyInfo = expectedTypeInfo.GetDeclaredProperty("Value");
                var expectedValueProperty = valuePropertyInfo.GetValue(expected);
                var actualValueProperty = valuePropertyInfo.GetValue(actual);

                AssertObject(expectedValueProperty, actualValueProperty, expectedIncludes);

                return;
            }

            Assert.Equal(expected, actual);
        }

        protected virtual void AssertCollection<TElement>(
            IEnumerable<TElement> expected, IEnumerable<TElement> actual, IEnumerable<IExpectedInclude> expectedIncludes)
        {
            if (expected == null
                && actual == null)
            {
                return;
            }

            Assert.Equal(expected == null, actual == null);

            var expectedList = expected.ToList();
            var actualList = actual.ToList();

            if (_entitySorters.TryGetValue(typeof(TElement), out var sorter))
            {
                // TODO: fix/cleanup
                var actualSorter = (Func<dynamic, object>)sorter;
                expectedList = ((IEnumerable<object>)expectedList).OrderBy(actualSorter).Cast<TElement>().ToList();
                actualList = ((IEnumerable<object>)actualList).OrderBy(actualSorter).Cast<TElement>().ToList();
            }

            Assert.Equal(expectedList.Count, actualList.Count);

            for (var i = 0; i < expectedList.Count; i++)
            {
                _fullPath.Push("[" + i + "]");

                var elementType = expectedList[i]?.GetType() ?? typeof(TElement);
                var assertElementMethodInfo = _assertElementMethodInfo.MakeGenericMethod(elementType);
                assertElementMethodInfo.Invoke(this, new object[] { expectedList[i], actualList[i], expectedIncludes });

                _fullPath.Pop();
            }
        }

        protected void ProcessIncludes<TEntity>(TEntity expected, TEntity actual, IEnumerable<IExpectedInclude> expectedIncludes)
        {
            var currentPath = string.Join(".", _path);
            foreach (var expectedInclude in expectedIncludes.OfType<ExpectedInclude<TEntity>>().Where(i => i.NavigationPath == currentPath))
            {
                var expectedIncludedNavigation = expectedInclude.Include(expected);
                var actualIncludedNavigation = expectedInclude.Include(actual);

                _path.Add(expectedInclude.IncludedName);
                _fullPath.Push("." + expectedInclude.IncludedName);

                AssertObject(expectedIncludedNavigation, actualIncludedNavigation, expectedIncludes);

                _path.RemoveAt(_path.Count - 1);
                _fullPath.Pop();
            }
        }

        // for debugging purposes
        protected string FullPath => string.Join(string.Empty, _fullPath.Reverse());
    }
}
