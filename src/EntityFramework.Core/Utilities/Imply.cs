// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable 0169 

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Storage;
using Remotion.Linq.Clauses;

// ReSharper disable AssignNullToNotNullAttribute

// ReSharper disable PossibleNullReferenceException

// ReSharper disable InconsistentNaming

namespace Microsoft.Data.Entity.Utilities
{
    internal partial class ImplyTypes
    {
        public ImplyGeneric<ValueBuffer> ValueBufferProp;
        public ImplyGeneric<object> Object;
        public ImplyGeneric<string> String;
    }

    /* 
    This code exists only to trick the ILC compliation to include metadata about combinations of entity types and our internal types.
    This is the jumping off point for a reasoning about what generic types may exist at runtime.
    https://github.com/aspnet/EntityFramework/issues/3477
    */
    [UsedImplicitly]
    internal class ImpliedEntityType<TEntity>
        where TEntity : class
    {
        public ImplyGeneric<TEntity> EntityProp;
        public ImplyJoin<TEntity, TEntity> Join;
    }

    internal partial class ImplyGeneric<T>
    {
        public SimpleKeyValueFactory<T> KeyFactoryType;
        public IAsyncEnumerable<T> AsyncEnum;

        public ClrPropertyGetter<object, T> GetterProp;
        public ClrPropertySetter<object, T> SetterProp;

        public ImplyJoin<T, T> Join;
        public Func<T> Func;

        public void ImplyMethods()
        {
            ((IDatabase)null).CompileQuery<T>(null);
            ((IDatabase)null).CompileQuery<IOrderedEnumerable<T>>(null);
            ((IDatabase)null).CompileQuery<IEnumerable<T>>(null);

            ((IDatabase)null).CompileAsyncQuery<T>(null);
            ((IDatabase)null).CompileAsyncQuery<IOrderedEnumerable<T>>(null);
            ((IDatabase)null).CompileAsyncQuery<IEnumerable<T>>(null);

            EF.Property<T>(null, null);

            LinqOperatorProvider._InterceptExceptions<T>(null, null, null);
            LinqOperatorProvider._ToEnumerable<T>(null);
            LinqOperatorProvider._ToOrdered<T>(null);
            LinqOperatorProvider._ToSequence((T)new object());
            LinqOperatorProvider._ToQueryable<T>(null);
            LinqOperatorProvider._TrackEntities<T, object>(null, null, null, null);
            LinqOperatorProvider._Where<T>(null, null);

            AsyncLinqOperatorProvider._InterceptExceptions<T>(null, null, null);
            AsyncLinqOperatorProvider._ToEnumerable<T>(null);
            AsyncLinqOperatorProvider._ToOrdered<T>(null);
            AsyncLinqOperatorProvider._ToSequence((T)new object());
            AsyncLinqOperatorProvider._ToQueryable<T>(null);
            AsyncLinqOperatorProvider._TrackEntities<T, object>(null, null, null, null);
            AsyncLinqOperatorProvider._Where<T>(null, null);
        }
    }

    internal partial class ImplyGeneric<T1, T2>
    {
        public Func<T1, T2> Func1;
        public Func<T2, T1> Func2;
        public Func<T1, IEnumerable<T2>> Func3;
        public Func<T2, IEnumerable<T1>> Func4;

        public Func<T2, T1, T2> JoinFuncUsedInQueries;
        public Func<T1, T2, T1> JoinFuncUsedInQueries2;

        public ImplyJoin<T1, T2> Join;
        public ImplyGeneric<EntityQueryModelVisitor.TransparentIdentifier<T1, T2>> TransparentIdentifier1;
        public ImplyGeneric<EntityQueryModelVisitor.TransparentIdentifier<T2, T1>> TransparentIdentifier2;

        public void ImplyMethods()
        {
            ((IDatabase)null).CompileQuery<IGrouping<T1, T2>>(null);
            ((IDatabase)null).CompileQuery<IGrouping<T2, T1>>(null);
            ((IDatabase)null).CompileAsyncQuery<IAsyncGrouping<T1, T2>>(null);
            ((IDatabase)null).CompileAsyncQuery<IAsyncGrouping<T2, T1>>(null);

            LinqOperatorProvider._OrderBy<T1, T2>(null, null, OrderingDirection.Asc);
            LinqOperatorProvider._OrderBy<T2, T1>(null, null, OrderingDirection.Asc);
            LinqOperatorProvider._Select<T1, T2>(null, null);
            LinqOperatorProvider._Select<T2, T1>(null, null);
            LinqOperatorProvider._ThenBy<T1, T2>(null, null, OrderingDirection.Asc);
            LinqOperatorProvider._ThenBy<T2, T1>(null, null, OrderingDirection.Asc);
            LinqOperatorProvider._TrackGroupedEntities<T1, T2, object>(null, null, null, null);
            LinqOperatorProvider._TrackGroupedEntities<T2, T1, object>(null, null, null, null);
        }
    }

    internal partial class ImplyGeneric<T1, T2, T3, T4>
    {
        public void ImplyMethods()
        {
            LinqOperatorProvider._Join<T1, T2, T3, T4>(null, null, null, null, null);
            LinqOperatorProvider._GroupJoin<T1, T2, T3, T4>(null, null, null, null, null);

            AsyncLinqOperatorProvider._Join<T1, T2, T3, T4>(null, null, null, null, null);
            AsyncLinqOperatorProvider._GroupJoin<T1, T2, T3, T4>(null, null, null, null, null);
        }
    }
}
