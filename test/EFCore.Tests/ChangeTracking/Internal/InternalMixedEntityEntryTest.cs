// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class InternalMixedEntityEntryTest : InternalEntityEntryTestBase<
    InternalMixedEntityEntryTest.SomeEntity,
    InternalMixedEntityEntryTest.SomeSimpleEntityBase,
    InternalMixedEntityEntryTest.SomeDependentEntity,
    InternalMixedEntityEntryTest.SomeMoreDependentEntity,
    InternalMixedEntityEntryTest.Root,
    InternalMixedEntityEntryTest.FirstDependent,
    InternalMixedEntityEntryTest.SecondDependent,
    InternalMixedEntityEntryTest.CompositeRoot,
    InternalMixedEntityEntryTest.CompositeFirstDependent,
    InternalMixedEntityEntryTest.SomeCompositeEntityBase,
    InternalMixedEntityEntryTest.CompositeSecondDependent,
    InternalMixedEntityEntryTest.KMixedContext,
    InternalMixedEntityEntryTest.KMixedSnapContext>
{
    public class SomeCompositeEntityBase;

    public class SomeDependentEntity : SomeCompositeEntityBase;

    public class SomeMoreDependentEntity : SomeSimpleEntityBase;

    public class Root : IRoot
    {
        public FirstDependent First { get; set; }

        IFirstDependent IRoot.First
        {
            get => First;
            set => First = (FirstDependent)value;
        }
    }

    public class FirstDependent : IFirstDependent
    {
        public Root Root { get; set; }

        IRoot IFirstDependent.Root
        {
            get => Root;
            set => Root = (Root)value;
        }

        public SecondDependent Second { get; set; }

        ISecondDependent IFirstDependent.Second
        {
            get => Second;
            set => Second = (SecondDependent)value;
        }
    }

    public class SecondDependent : ISecondDependent
    {
        public FirstDependent First { get; set; }

        IFirstDependent ISecondDependent.First
        {
            get => First;
            set => First = (FirstDependent)value;
        }
    }

    public class CompositeRoot : ICompositeRoot
    {
        public ICompositeFirstDependent First { get; set; }
    }

    public class CompositeFirstDependent : ICompositeFirstDependent
    {
        public CompositeRoot Root { get; set; }

        ICompositeRoot ICompositeFirstDependent.Root
        {
            get => Root;
            set => Root = (CompositeRoot)value;
        }

        public CompositeSecondDependent Second { get; set; }

        ICompositeSecondDependent ICompositeFirstDependent.Second
        {
            get => Second;
            set => Second = (CompositeSecondDependent)value;
        }
    }

    public class CompositeSecondDependent : ICompositeSecondDependent
    {
        public CompositeFirstDependent First { get; set; }

        ICompositeFirstDependent ICompositeSecondDependent.First
        {
            get => First;
            set => First = (CompositeFirstDependent)value;
        }
    }

    public class SomeSimpleEntityBase;

    public class SomeEntity : SomeSimpleEntityBase;

    public class KMixedContext : KContext;

    public class KMixedSnapContext : KContext;
}
