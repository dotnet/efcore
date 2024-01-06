// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class NonLoadingNavigationsInMemoryTest(NonLoadingNavigationsInMemoryTest.NonLoadingNavigationsInMemoryFixture fixture) : LoadTestBase<NonLoadingNavigationsInMemoryTest.NonLoadingNavigationsInMemoryFixture>(fixture)
{
    protected override bool LazyLoadingEnabled
        => false;

    public class NonLoadingNavigationsInMemoryFixture : LoadFixtureBase
    {
        protected override string StoreName
            => "NonLoadingNavigations";

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Parent>(
                b =>
                {
                    b.Navigation(e => e.Children).EnableLazyLoading(false);
                    b.Navigation(e => e.ChildrenAk).EnableLazyLoading(false);
                    b.Navigation(e => e.ChildrenCompositeKey).EnableLazyLoading(false);
                    b.Navigation(e => e.ChildrenShadowFk).EnableLazyLoading(false);
                    b.Navigation(e => e.Single).EnableLazyLoading(false);
                    b.Navigation(e => e.SingleAk).EnableLazyLoading(false);
                    b.Navigation(e => e.SingleCompositeKey).EnableLazyLoading(false);
                    b.Navigation(e => e.SingleShadowFk).EnableLazyLoading(false);
                    b.Navigation(e => e.SinglePkToPk).EnableLazyLoading(false);
                    b.Navigation(e => e.RequiredSingle).EnableLazyLoading(false);
                });

            modelBuilder.Entity<Child>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<ChildAk>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<ChildShadowFk>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<ChildCompositeKey>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<Single>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<SingleAk>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<SingleCompositeKey>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<SingleShadowFk>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<SinglePkToPk>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<RequiredSingle>().Navigation(e => e.Parent).EnableLazyLoading(false);

            modelBuilder.Entity<ParentFullLoaderByConstructor>(
                b =>
                {
                    b.Navigation(e => e.Children).EnableLazyLoading(false);
                    b.Navigation(e => e.Single).EnableLazyLoading(false);
                });

            modelBuilder.Entity<ChildFullLoaderByConstructor>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<SingleFullLoaderByConstructor>().Navigation(e => e.Parent).EnableLazyLoading(false);

            modelBuilder.Entity<ParentDelegateLoaderByConstructor>(
                b =>
                {
                    b.Navigation(e => e.Children).EnableLazyLoading(false);
                    b.Navigation(e => e.Single).EnableLazyLoading(false);
                });

            modelBuilder.Entity<ChildDelegateLoaderByConstructor>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<SingleDelegateLoaderByConstructor>().Navigation(e => e.Parent).EnableLazyLoading(false);

            modelBuilder.Entity<ParentDelegateLoaderByProperty>(
                b =>
                {
                    b.Navigation(e => e.Children).EnableLazyLoading(false);
                    b.Navigation(e => e.Single).EnableLazyLoading(false);
                });

            modelBuilder.Entity<ChildDelegateLoaderByProperty>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<SingleDelegateLoaderByProperty>().Navigation(e => e.Parent).EnableLazyLoading(false);

            modelBuilder.Entity<ParentDelegateLoaderWithStateByProperty>(
                b =>
                {
                    b.Navigation(e => e.Children).EnableLazyLoading(false);
                    b.Navigation(e => e.Single).EnableLazyLoading(false);
                });

            modelBuilder.Entity<ChildDelegateLoaderWithStateByProperty>().Navigation(e => e.Parent).EnableLazyLoading(false);
            modelBuilder.Entity<SingleDelegateLoaderWithStateByProperty>().Navigation(e => e.Parent).EnableLazyLoading(false);

            modelBuilder.Entity<Product>().Navigation(e => e.Deposit).EnableLazyLoading(false);
            modelBuilder.Entity<OptionalChildView>().HasNoKey().Navigation(e => e.Root).EnableLazyLoading(false);
            modelBuilder.Entity<RequiredChildView>().HasNoKey().Navigation(e => e.Root).EnableLazyLoading(false);
        }
    }
}
