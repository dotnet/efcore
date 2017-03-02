// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.FunkyDataModel;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class FunkyDataQueryTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : FunkyDataQueryFixtureBase<TTestStore>, new()
    {
        protected FunkyDataContext CreateContext() => Fixture.CreateContext(TestStore);

        protected FunkyDataQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        [ConditionalFact]
        public virtual void String_contains_on_argument_with_wildcard_constant()
        {
            using (var ctx = CreateContext())
            {
                var result1 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains("%B")).Select(c => c.FirstName).ToList();
                var expected1 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.Contains("%B"));
                Assert.True(expected1.Count() == result1.Count);

                var result2 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains("a_")).Select(c => c.FirstName).ToList();
                var expected2 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.Contains("a_"));
                Assert.True(expected2.Count() == result2.Count);

                var result3 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains(null)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result3.Count);

                var result4 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains("")).Select(c => c.FirstName).ToList();
                Assert.True(ctx.FunkyCustomers.Count() == result4.Count);

                var result5 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains("_Ba_")).Select(c => c.FirstName).ToList();
                var expected5 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.Contains("_Ba_"));
                Assert.True(expected5.Count() == result5.Count);

                var result6 = ctx.FunkyCustomers.Where(c => !c.FirstName.Contains("%B%a%r")).Select(c => c.FirstName).ToList();
                var expected6 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && !c.Contains("%B%a%r"));
                Assert.True(expected6.Count() == result6.Count);

                var result7 = ctx.FunkyCustomers.Where(c => !c.FirstName.Contains("")).Select(c => c.FirstName).ToList();
                Assert.True(0 == result7.Count);

                var result8 = ctx.FunkyCustomers.Where(c => !c.FirstName.Contains(null)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result8.Count);
            }
        }

        [ConditionalFact]
        public virtual void String_contains_on_argument_with_wildcard_parameter()
        {
            using (var ctx = CreateContext())
            {
                var prm1 = "%B";
                var result1 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains(prm1)).Select(c => c.FirstName).ToList();
                var expected1 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.Contains(prm1));
                Assert.True(expected1.Count() == result1.Count);

                var prm2 = "a_";
                var result2 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains(prm2)).Select(c => c.FirstName).ToList();
                var expected2 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.Contains(prm2));
                Assert.True(expected2.Count() == result2.Count);

                var prm3 = (string)null;
                var result3 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains(prm3)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result3.Count);

                var prm4 = "";
                var result4 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains(prm4)).Select(c => c.FirstName).ToList();
                Assert.True(ctx.FunkyCustomers.Count() == result4.Count);

                var prm5 = "_Ba_";
                var result5 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains(prm5)).Select(c => c.FirstName).ToList();
                var expected5 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.Contains(prm5));
                Assert.True(expected5.Count() == result5.Count);

                var prm6 = "%B%a%r";
                var result6 = ctx.FunkyCustomers.Where(c => !c.FirstName.Contains(prm6)).Select(c => c.FirstName).ToList();
                var expected6 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && !c.Contains(prm6));
                Assert.True(expected6.Count() == result6.Count);

                var prm7 = "";
                var result7 = ctx.FunkyCustomers.Where(c => !c.FirstName.Contains(prm7)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result7.Count);

                var prm8 = (string)null;
                var result8 = ctx.FunkyCustomers.Where(c => !c.FirstName.Contains(prm8)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result8.Count);
            }
        }

        [ConditionalFact]
        public virtual void String_contains_on_argument_with_wildcard_column()
        {
            using (var ctx = CreateContext())
            {
                var result = ctx.FunkyCustomers.Select(c => c.FirstName)
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.fn.Contains(r.ln))
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                var expected = ctx.FunkyCustomers.Select(c => c.FirstName).ToList()
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName).ToList(), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln == "" || (r.fn != null && r.ln != null && r.fn.Contains(r.ln)))
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                Assert.Equal(result.Count, expected.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected[i].fn == result[i].fn);
                    Assert.True(expected[i].ln == result[i].ln);
                }
            }
        }

        [ConditionalFact]
        public virtual void String_contains_on_argument_with_wildcard_column_negated()
        {
            using (var ctx = CreateContext())
            {
                var result = ctx.FunkyCustomers.Select(c => c.FirstName)
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => !r.fn.Contains(r.ln))
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                var expected = ctx.FunkyCustomers.Select(c => c.FirstName).ToList()
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName).ToList(), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln != "" && r.fn != null && r.ln != null && !r.fn.Contains(r.ln))
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                Assert.Equal(result.Count, expected.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected[i].fn == result[i].fn);
                    Assert.True(expected[i].ln == result[i].ln);
                }
            }
        }

        [ConditionalFact]
        public virtual void String_starts_with_on_argument_with_wildcard_constant()
        {
            using (var ctx = CreateContext())
            {
                var result1 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith("%B")).Select(c => c.FirstName).ToList();
                var expected1 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith("%B"));
                Assert.True(expected1.Count() == result1.Count);

                var result2 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith("a_")).Select(c => c.FirstName).ToList();
                var expected2 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith("a_"));
                Assert.True(expected2.Count() == result2.Count);

                var result3 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(null)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result3.Count);

                var result4 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith("")).Select(c => c.FirstName).ToList();
                Assert.True(ctx.FunkyCustomers.Count() == result4.Count);

                var result5 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith("_Ba_")).Select(c => c.FirstName).ToList();
                var expected5 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith("_Ba_"));
                Assert.True(expected5.Count() == result5.Count);

                var result6 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith("%B%a%r")).Select(c => c.FirstName).ToList();
                var expected6 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && !c.StartsWith("%B%a%r"));
                Assert.True(expected6.Count() == result6.Count);

                var result7 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith("")).Select(c => c.FirstName).ToList();
                Assert.True(0 == result7.Count);

                var result8 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith(null)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result8.Count);
            }
        }

        [ConditionalFact]
        public virtual void String_starts_with_on_argument_with_wildcard_parameter()
        {
            using (var ctx = CreateContext())
            {
                var prm1 = "%B";
                var result1 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(prm1)).Select(c => c.FirstName).ToList();
                var expected1 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith(prm1));
                Assert.True(expected1.Count() == result1.Count);

                var prm2 = "a_";
                var result2 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(prm2)).Select(c => c.FirstName).ToList();
                var expected2 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith(prm2));
                Assert.True(expected2.Count() == result2.Count);

                var prm3 = (string)null;
                var result3 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(prm3)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result3.Count);

                var prm4 = "";
                var result4 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(prm4)).Select(c => c.FirstName).ToList();
                Assert.True(ctx.FunkyCustomers.Count() == result4.Count);

                var prm5 = "_Ba_";
                var result5 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(prm5)).Select(c => c.FirstName).ToList();
                var expected5 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith(prm5));
                Assert.True(expected5.Count() == result5.Count);

                var prm6 = "%B%a%r";
                var result6 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith(prm6)).Select(c => c.FirstName).ToList();
                var expected6 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && !c.StartsWith(prm6));
                Assert.True(expected6.Count() == result6.Count);

                var prm7 = "";
                var result7 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith(prm7)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result7.Count);

                var prm8 = (string)null;
                var result8 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith(prm8)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result8.Count);
            }
        }

        [ConditionalFact]
        public virtual void String_starts_with_on_argument_with_wildcard_column()
        {
            using (var ctx = CreateContext())
            {
                var result = ctx.FunkyCustomers.Select(c => c.FirstName)
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.fn.StartsWith(r.ln))
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                var expected = ctx.FunkyCustomers.Select(c => c.FirstName).ToList()
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName).ToList(), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln == "" || (r.fn != null && r.ln != null && r.fn.StartsWith(r.ln)))
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                Assert.Equal(result.Count, expected.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected[i].fn == result[i].fn);
                    Assert.True(expected[i].ln == result[i].ln);
                }
            }
        }

        [ConditionalFact]
        public virtual void String_starts_with_on_argument_with_wildcard_column_negated()
        {
            using (var ctx = CreateContext())
            {
                var result = ctx.FunkyCustomers.Select(c => c.FirstName)
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => !r.fn.StartsWith(r.ln))
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                var expected = ctx.FunkyCustomers.Select(c => c.FirstName).ToList()
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName).ToList(), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln != "" && r.fn != null && r.ln != null && !r.fn.StartsWith(r.ln))
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                Assert.Equal(result.Count, expected.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected[i].fn == result[i].fn);
                    Assert.True(expected[i].ln == result[i].ln);
                }
            }
        }

        [ConditionalFact]
        public virtual void String_ends_with_on_argument_with_wildcard_constant()
        {
            using (var ctx = CreateContext())
            {
                var result1 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith("%B")).Select(c => c.FirstName).ToList();
                var expected1 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith("%B"));
                Assert.True(expected1.Count() == result1.Count);

                var result2 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith("_r")).Select(c => c.FirstName).ToList();
                var expected2 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith("_r"));
                Assert.True(expected2.Count() == result2.Count);

                var result3 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(null)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result3.Count);

                var result4 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith("")).Select(c => c.FirstName).ToList();
                Assert.True(ctx.FunkyCustomers.Count() == result4.Count);

                var result5 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith("a__r_")).Select(c => c.FirstName).ToList();
                var expected5 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith("a__r_"));
                Assert.True(expected5.Count() == result5.Count);

                var result6 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith("%B%a%r")).Select(c => c.FirstName).ToList();
                var expected6 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && !c.StartsWith("%B%a%r"));
                Assert.True(expected6.Count() == result6.Count);

                var result7 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith("")).Select(c => c.FirstName).ToList();
                Assert.True(0 == result7.Count);

                var result8 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith(null)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result8.Count);
            }
        }

        [ConditionalFact]
        public virtual void String_ends_with_on_argument_with_wildcard_parameter()
        {
            using (var ctx = CreateContext())
            {
                var prm1 = "%B";
                var result1 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(prm1)).Select(c => c.FirstName).ToList();
                var expected1 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith(prm1));
                Assert.True(expected1.Count() == result1.Count);

                var prm2 = "_r";
                var result2 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(prm2)).Select(c => c.FirstName).ToList();
                var expected2 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith(prm2));
                Assert.True(expected2.Count() == result2.Count);

                var prm3 = (string)null;
                var result3 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(prm3)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result3.Count);

                var prm4 = "";
                var result4 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(prm4)).Select(c => c.FirstName).ToList();
                Assert.True(ctx.FunkyCustomers.Count() == result4.Count);

                var prm5 = "a__r_";
                var result5 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(prm5)).Select(c => c.FirstName).ToList();
                var expected5 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith(prm5));
                Assert.True(expected5.Count() == result5.Count);

                var prm6 = "%B%a%r";
                var result6 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith(prm6)).Select(c => c.FirstName).ToList();
                var expected6 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && !c.StartsWith(prm6));
                Assert.True(expected6.Count() == result6.Count);

                var prm7 = "";
                var result7 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith(prm7)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result7.Count);

                var prm8 = (string)null;
                var result8 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith(prm8)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result8.Count);
            }
        }

        [ConditionalFact]
        public virtual void String_ends_with_on_argument_with_wildcard_column()
        {
            using (var ctx = CreateContext())
            {
                var result = ctx.FunkyCustomers.Select(c => c.FirstName)
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.fn.EndsWith(r.ln))
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                var expected = ctx.FunkyCustomers.Select(c => c.FirstName).ToList()
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName).ToList(), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln == "" || (r.fn != null && r.ln != null && r.fn.EndsWith(r.ln)))
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                Assert.Equal(result.Count, expected.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected[i].fn == result[i].fn);
                    Assert.True(expected[i].ln == result[i].ln);
                }
            }
        }

        [ConditionalFact]
        public virtual void String_ends_with_on_argument_with_wildcard_column_negated()
        {
            using (var ctx = CreateContext())
            {
                var result = ctx.FunkyCustomers.Select(c => c.FirstName)
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => !r.fn.EndsWith(r.ln))
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                var expected = ctx.FunkyCustomers.Select(c => c.FirstName).ToList()
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName).ToList(), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln != "" && r.fn != null && r.ln != null && !r.fn.EndsWith(r.ln))
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                Assert.Equal(result.Count, expected.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected[i].fn == result[i].fn);
                    Assert.True(expected[i].ln == result[i].ln);
                }
            }
        }

        [ConditionalFact]
        public virtual void String_ends_with_inside_conditional()
        {
            using (var ctx = CreateContext())
            {
                var result = ctx.FunkyCustomers.Select(c => c.FirstName)
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.fn.EndsWith(r.ln) ? true : false)
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                var expected = ctx.FunkyCustomers.Select(c => c.FirstName).ToList()
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName).ToList(), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln == "" || (r.fn != null && r.ln != null && r.fn.EndsWith(r.ln)) ? true : false)
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                Assert.Equal(result.Count, expected.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected[i].fn == result[i].fn);
                    Assert.True(expected[i].ln == result[i].ln);
                }
            }
        }

        [ConditionalFact]
        public virtual void String_ends_with_inside_conditional_negated()
        {
            using (var ctx = CreateContext())
            {
                var result = ctx.FunkyCustomers.Select(c => c.FirstName)
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => !r.fn.EndsWith(r.ln) ? true : false)
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                var expected = ctx.FunkyCustomers.Select(c => c.FirstName).ToList()
                    .SelectMany(c => ctx.FunkyCustomers.Select(c2 => c2.LastName).ToList(), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln != "" && r.fn != null && r.ln != null && !r.fn.EndsWith(r.ln) ? true : false)
                    .ToList().OrderBy(r => r.fn).ThenBy(r => r.ln).ToList();

                Assert.Equal(result.Count, expected.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected[i].fn == result[i].fn);
                    Assert.True(expected[i].ln == result[i].ln);
                }
            }
        }

        [ConditionalFact]
        public virtual void String_ends_with_equals_nullable_column()
        {
            using (var ctx = CreateContext())
            {
                var expected = ctx.FunkyCustomers.ToList()
                    .SelectMany(c => ctx.FunkyCustomers.ToList(), (c1, c2) => new { c1, c2 })
                    .Where(r => (r.c2.LastName != null && r.c1.FirstName != null && r.c1.NullableBool.HasValue && r.c1.FirstName.EndsWith(r.c2.LastName) == r.c1.NullableBool.Value) || (r.c2.LastName == null && r.c1.NullableBool == false))
                    .ToList().Select(r => new { r.c1.FirstName, r.c2.LastName, r.c1.NullableBool }).OrderBy(r => r.FirstName).ThenBy(r => r.LastName).ToList();

                ClearLog();

                var result = ctx.FunkyCustomers
                    .SelectMany(c => ctx.FunkyCustomers, (c1, c2) => new { c1, c2 })
                    .Where(r => r.c1.FirstName.EndsWith(r.c2.LastName) == r.c1.NullableBool.Value)
                    .ToList().Select(r => new { r.c1.FirstName, r.c2.LastName, r.c1.NullableBool }).OrderBy(r => r.FirstName).ThenBy(r => r.LastName).ToList();

                Assert.Equal(result.Count, expected.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected[i].FirstName == result[i].FirstName);
                    Assert.True(expected[i].LastName == result[i].LastName);
                }
            }
        }

        [ConditionalFact]
        public virtual void String_ends_with_not_equals_nullable_column()
        {
            using (var ctx = CreateContext())
            {
                var expected = ctx.FunkyCustomers.ToList()
                    .SelectMany(c => ctx.FunkyCustomers.ToList(), (c1, c2) => new { c1, c2 })
                    .Where(r =>
                        (r.c2.LastName != null && r.c1.FirstName != null && r.c1.NullableBool.HasValue && r.c1.FirstName.EndsWith(r.c2.LastName) != r.c1.NullableBool.Value)
                        || r.c1.NullableBool == null
                        || (r.c2.LastName == null && r.c1.NullableBool == true))
                    .ToList().Select(r => new { r.c1.FirstName, r.c2.LastName, r.c1.NullableBool }).OrderBy(r => r.FirstName).ThenBy(r => r.LastName).ToList();

                ClearLog();

                var result = ctx.FunkyCustomers
                    .SelectMany(c => ctx.FunkyCustomers, (c1, c2) => new { c1, c2 })
                    .Where(r => r.c1.FirstName.EndsWith(r.c2.LastName) != r.c1.NullableBool.Value)
                    .ToList().Select(r => new { r.c1.FirstName, r.c2.LastName, r.c1.NullableBool }).OrderBy(r => r.FirstName).ThenBy(r => r.LastName).ToList();

                Assert.Equal(result.Count, expected.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected[i].FirstName == result[i].FirstName);
                    Assert.True(expected[i].LastName == result[i].LastName);
                }
            }
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        protected virtual void ClearLog()
        {
        }

        public void Dispose() => TestStore.Dispose();
    }
}
