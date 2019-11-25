// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class InterceptionTestBase
    {
        protected InterceptionTestBase(InterceptionFixtureBase fixture)
        {
            Fixture = fixture;
        }

        protected InterceptionFixtureBase Fixture { get; }

        protected class Singularity
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Type { get; set; }
        }

        protected class Brane
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Type { get; set; }
        }

        public class UniverseContext : PoolableDbContext
        {
            public UniverseContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Singularity>()
                    .HasData(
                        new Singularity { Id = 77, Type = "Black Hole" },
                        new Singularity { Id = 88, Type = "Bing Bang" });

                modelBuilder
                    .Entity<Brane>()
                    .HasData(
                        new Brane { Id = 77, Type = "Black Hole?" },
                        new Brane { Id = 88, Type = "Bing Bang?" });
            }
        }

        protected (DbContext, TInterceptor) CreateContext<TInterceptor>(bool inject = false)
            where TInterceptor : class, IInterceptor, new()
        {
            var interceptor = new TInterceptor();

            var context = inject ? CreateContext(null, interceptor) : CreateContext(interceptor);

            return (context, interceptor);
        }

        public UniverseContext CreateContext(IInterceptor appInterceptor, params IInterceptor[] injectedInterceptors)
            => new UniverseContext(
                Fixture.CreateOptions(
                    new[] { appInterceptor }, injectedInterceptors));

        public UniverseContext CreateContext(
            IEnumerable<IInterceptor> appInterceptors,
            IEnumerable<IInterceptor> injectedInterceptors = null)
            => new UniverseContext(Fixture.CreateOptions(appInterceptors, injectedInterceptors ?? Enumerable.Empty<IInterceptor>()));

        public interface ITestDiagnosticListener : IDisposable
        {
            void AssertEventsInOrder(params string[] eventNames);
        }

        public class NullDiagnosticListener : ITestDiagnosticListener
        {
            public void AssertEventsInOrder(params string[] eventNames)
            {
            }

            public void Dispose()
            {
            }
        }

        public class TestDiagnosticListener : ITestDiagnosticListener,
            IObserver<DiagnosticListener>,
            IObserver<KeyValuePair<string, object>>
        {
            private readonly DbContextId _contextId;
            private readonly IDisposable _subscription;
            private readonly List<string> _events = new List<string>();

            public TestDiagnosticListener(DbContextId contextId)
            {
                _contextId = contextId;
                _subscription = DiagnosticListener.AllListeners.Subscribe(this);
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void AssertEventsInOrder(params string[] eventNames)
            {
                Assert.True(_events.Count >= eventNames.Length);

                var lastIndex = -1;
                for (var i = 0; i < eventNames.Length; i++)
                {
                    var indexFound = _events.IndexOf(eventNames[i]);

                    if (indexFound < 0)
                    {
                        Assert.True(false, $"Event {eventNames[i]} not found.");
                    }

                    if (indexFound < lastIndex)
                    {
                        Assert.True(false, $"Event {eventNames[i]} found before {eventNames[i - 1]}.");
                    }

                    lastIndex = indexFound;
                }
            }

            public void OnNext(DiagnosticListener listener)
            {
                if (listener?.Name == DbLoggerCategory.Name)
                {
                    listener.Subscribe(this);
                }
            }

            public void OnNext(KeyValuePair<string, object> value)
            {
                var eventData = value.Value as DbContextEventData;
                if (eventData?.Context?.ContextId == _contextId)
                {
                    _events.Add(value.Key);
                }
            }

            public void Dispose()
            {
                _subscription.Dispose();
            }
        }

        public abstract class InterceptionFixtureBase : SharedStoreFixtureBase<UniverseContext>
        {
            protected abstract bool ShouldSubscribeToDiagnosticListener { get; }

            public virtual ITestDiagnosticListener SubscribeToDiagnosticListener(DbContextId contextId)
                => ShouldSubscribeToDiagnosticListener
                    ? (ITestDiagnosticListener)new TestDiagnosticListener(contextId)
                    : new NullDiagnosticListener();

            public virtual DbContextOptions CreateOptions(
                IEnumerable<IInterceptor> appInterceptors,
                IEnumerable<IInterceptor> injectedInterceptors)
                => AddOptions(
                        TestStore
                            .AddProviderOptions(
                                new DbContextOptionsBuilder()
                                    .AddInterceptors(appInterceptors)
                                    .UseInternalServiceProvider(
                                        InjectInterceptors(new ServiceCollection(), injectedInterceptors)
                                            .BuildServiceProvider())))
                    .EnableDetailedErrors()
                    .Options;

            protected virtual IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
            {
                foreach (var interceptor in injectedInterceptors)
                {
                    serviceCollection.AddSingleton(interceptor);
                }

                return serviceCollection;
            }
        }
    }
}
