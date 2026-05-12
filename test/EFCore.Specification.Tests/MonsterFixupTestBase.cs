// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.TestModels;

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class MonsterFixupTestBase<TFixture> : IClassFixture<TFixture>, IDisposable
    where TFixture : MonsterFixupTestBase<TFixture>.MonsterFixupFixtureBase, new()
{
    protected MonsterFixupTestBase(TFixture fixture)
    {
        Fixture = fixture;
        TestStore = fixture.CreateTestStore();
        Options = fixture.CreateOptions(TestStore);
    }

    protected TFixture Fixture { get; }
    protected TestStore TestStore { get; }
    protected DbContextOptions Options { get; }

    [ConditionalFact]
    public virtual async Task Can_build_monster_model_and_seed_data_using_FKs()
    {
        await CreateAndSeedDatabase(async context => await context.SeedUsingFKs());

        SimpleVerification();
        FkVerification();
        NavigationVerification();
    }

    [ConditionalFact]
    public virtual async Task Can_build_monster_model_and_seed_data_using_all_navigations()
    {
        await CreateAndSeedDatabase(async context => await context.SeedUsingNavigations(dependentNavs: true, principalNavs: true));

        SimpleVerification();
        FkVerification();
        NavigationVerification();
    }

    [ConditionalFact]
    public async Task Can_build_monster_model_and_seed_data_using_dependent_navigations()
    {
        await CreateAndSeedDatabase(async context => await context.SeedUsingNavigations(dependentNavs: true, principalNavs: false));

        SimpleVerification();
        FkVerification();
        NavigationVerification();
    }

    [ConditionalFact]
    public async Task Can_build_monster_model_and_seed_data_using_principal_navigations()
    {
        await CreateAndSeedDatabase(async context => await context.SeedUsingNavigations(dependentNavs: false, principalNavs: true));

        SimpleVerification();
        FkVerification();
        NavigationVerification();
    }

    [ConditionalFact]
    public async Task Can_build_monster_model_and_seed_data_using_navigations_with_deferred_add()
    {
        await CreateAndSeedDatabase(async context => await context.SeedUsingNavigationsWithDeferredAdd());

        SimpleVerification();
        FkVerification();
        NavigationVerification();
    }

    [ConditionalFact]
    public async Task One_to_many_fixup_happens_when_FKs_change_test()
    {
        await CreateAndSeedDatabase(async context => await context.SeedUsingFKs());

        using (var context = CreateContext())
        {
            var login1 = context.Logins.Single(e => e.Username == "MrsKoalie73");
            var login2 = context.Logins.Single(e => e.Username == "MrsBossyPants");
            var login3 = context.Logins.Single(e => e.Username == "TheStripedMenace");

            var message1 = context.Messages.Single(e => e.Body.StartsWith("Fancy"));
            var message2 = context.Messages.Single(e => e.Body.StartsWith("Love"));
            var message3 = context.Messages.Single(e => e.Body.StartsWith("I'll"));

            AssertReceivedMessagesConsistent(login1, message2);
            AssertReceivedMessagesConsistent(login2, message1, message3);
            AssertReceivedMessagesConsistent(login3);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);

            // Simple change
            message2.ToUsername = login3.Username;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertReceivedMessagesConsistent(login1);
            AssertReceivedMessagesConsistent(login2, message1, message3);
            AssertReceivedMessagesConsistent(login3, message2);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);

            // Change back
            message2.ToUsername = login1.Username;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertReceivedMessagesConsistent(login1, message2);
            AssertReceivedMessagesConsistent(login2, message1, message3);
            AssertReceivedMessagesConsistent(login3);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);

            // Remove the relationship
            message2.ToUsername = null;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertReceivedMessagesConsistent(login1);
            AssertReceivedMessagesConsistent(login2, message1, message3);
            AssertReceivedMessagesConsistent(login3);
            AssertReceivedMessagesConsistent(null, message2);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);

            // Put the relationship back
            message2.ToUsername = login3.Username;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertReceivedMessagesConsistent(login1);
            AssertReceivedMessagesConsistent(login2, message1, message3);
            AssertReceivedMessagesConsistent(login3, message2);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);
        }
    }

    [ConditionalFact]
    public async Task One_to_many_fixup_happens_when_reference_changes()
    {
        await CreateAndSeedDatabase(async context => await context.SeedUsingFKs());

        using (var context = CreateContext())
        {
            var login1 = context.Logins.Single(e => e.Username == "MrsKoalie73");
            var login2 = context.Logins.Single(e => e.Username == "MrsBossyPants");
            var login3 = context.Logins.Single(e => e.Username == "TheStripedMenace");

            var message1 = context.Messages.Single(e => e.Body.StartsWith("Fancy"));
            var message2 = context.Messages.Single(e => e.Body.StartsWith("Love"));
            var message3 = context.Messages.Single(e => e.Body.StartsWith("I'll"));

            AssertReceivedMessagesConsistent(login1, message2);
            AssertReceivedMessagesConsistent(login2, message1, message3);
            AssertReceivedMessagesConsistent(login3);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);

            // Simple change
            message2.Recipient = login3;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertReceivedMessagesConsistent(login1);
            AssertReceivedMessagesConsistent(login2, message1, message3);
            AssertReceivedMessagesConsistent(login3, message2);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);

            // Change back
            message2.Recipient = login1;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertReceivedMessagesConsistent(login1, message2);
            AssertReceivedMessagesConsistent(login2, message1, message3);
            AssertReceivedMessagesConsistent(login3);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);

            // Remove the relationship
            message2.Recipient = null;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertReceivedMessagesConsistent(login1);
            AssertReceivedMessagesConsistent(login2, message1, message3);
            AssertReceivedMessagesConsistent(login3);
            AssertReceivedMessagesConsistent(null, message2);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);

            // Put the relationship back
            message2.Recipient = login3;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertReceivedMessagesConsistent(login1);
            AssertReceivedMessagesConsistent(login2, message1, message3);
            AssertReceivedMessagesConsistent(login3, message2);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);
        }
    }

    [ConditionalFact]
    public async Task One_to_many_fixup_happens_when_collection_changes()
    {
        await CreateAndSeedDatabase(async context => await context.SeedUsingFKs());

        using (var context = CreateContext())
        {
            var login1 = context.Logins.Single(e => e.Username == "MrsKoalie73");
            var login2 = context.Logins.Single(e => e.Username == "MrsBossyPants");
            var login3 = context.Logins.Single(e => e.Username == "TheStripedMenace");

            var message1 = context.Messages.Single(e => e.Body.StartsWith("Fancy"));
            var message2 = context.Messages.Single(e => e.Body.StartsWith("Love"));
            var message3 = context.Messages.Single(e => e.Body.StartsWith("I'll"));

            AssertReceivedMessagesConsistent(login1, message2);
            AssertReceivedMessagesConsistent(login2, message1, message3);
            AssertReceivedMessagesConsistent(login3);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);

            // Remove entities
            login2.ReceivedMessages.Remove(message3);
            login1.ReceivedMessages.Remove(message2);

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertReceivedMessagesConsistent(login1);
            AssertReceivedMessagesConsistent(login2, message1);
            AssertReceivedMessagesConsistent(login3);
            AssertReceivedMessagesConsistent(null, message2, message3);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);

            // Add entities
            login1.ReceivedMessages.Add(message3);
            login2.ReceivedMessages.Add(message2);

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertReceivedMessagesConsistent(login1, message3);
            AssertReceivedMessagesConsistent(login2, message1, message2);
            AssertReceivedMessagesConsistent(login3);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);

            // Remove and add at the same time
            login2.ReceivedMessages.Remove(message2);
            login1.ReceivedMessages.Remove(message3);
            login1.ReceivedMessages.Add(message2);
            login2.ReceivedMessages.Add(message3);

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertReceivedMessagesConsistent(login1, message2);
            AssertReceivedMessagesConsistent(login2, message1, message3);
            AssertReceivedMessagesConsistent(login3);

            AssertSentMessagesConsistent(login1, message1, message3);
            AssertSentMessagesConsistent(login2, message2);
            AssertSentMessagesConsistent(login3);
        }
    }

    [ConditionalFact]
    public async Task One_to_one_fixup_happens_when_FKs_change_test()
    {
        await CreateAndSeedDatabase(async context => await context.SeedUsingFKs());

        using (var context = CreateContext())
        {
            var customer0 = context.Customers.Single(e => e.Name == "Eeky Bear");
            var customer1 = context.Customers.Single(e => e.Name == "Sheila Koalie");
            var customer2 = context.Customers.Single(e => e.Name == "Sue Pandy");
            var customer3 = context.Customers.Single(e => e.Name == "Tarquin Tiger");

            AssertSpousesConsistent(customer0, null);
            AssertSpousesConsistent(customer1, null);
            AssertSpousesConsistent(customer2, customer0);
            AssertSpousesConsistent(customer3, null);
            AssertSpousesConsistent(null, customer1);
            AssertSpousesConsistent(null, customer2);
            AssertSpousesConsistent(null, customer3);

            // Add a new relationship
            customer1.HusbandId = customer3.CustomerId;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertSpousesConsistent(customer0, null);
            AssertSpousesConsistent(customer1, customer3);
            AssertSpousesConsistent(customer2, customer0);
            AssertSpousesConsistent(customer3, null);
            AssertSpousesConsistent(null, customer1);
            AssertSpousesConsistent(null, customer2);

            // Remove the relationship
            customer1.HusbandId = null;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertSpousesConsistent(customer0, null);
            AssertSpousesConsistent(customer1, null);
            AssertSpousesConsistent(customer2, customer0);
            AssertSpousesConsistent(customer3, null);
            AssertSpousesConsistent(null, customer1);
            AssertSpousesConsistent(null, customer2);
            AssertSpousesConsistent(null, customer3);

            // Change existing relationship
            customer2.HusbandId = customer3.CustomerId;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertSpousesConsistent(customer0, null);
            AssertSpousesConsistent(customer1, null);
            AssertSpousesConsistent(customer2, customer3);
            AssertSpousesConsistent(customer3, null);
            AssertSpousesConsistent(null, customer0);
            AssertSpousesConsistent(null, customer1);
            AssertSpousesConsistent(null, customer2);

            // Give Tarquin a husband and a wife
            customer3.HusbandId = customer2.CustomerId;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertSpousesConsistent(customer0, null);
            AssertSpousesConsistent(customer1, null);
            AssertSpousesConsistent(customer2, customer3);
            AssertSpousesConsistent(customer3, customer2);
            AssertSpousesConsistent(null, customer0);
            AssertSpousesConsistent(null, customer1);
        }
    }

    [ConditionalFact]
    public async Task One_to_one_fixup_happens_when_reference_change_test()
    {
        await CreateAndSeedDatabase(async context => await context.SeedUsingFKs());

        using (var context = CreateContext())
        {
            var customer0 = context.Customers.Single(e => e.Name == "Eeky Bear");
            var customer1 = context.Customers.Single(e => e.Name == "Sheila Koalie");
            var customer2 = context.Customers.Single(e => e.Name == "Sue Pandy");
            var customer3 = context.Customers.Single(e => e.Name == "Tarquin Tiger");

            AssertSpousesConsistent(customer0, null);
            AssertSpousesConsistent(customer1, null);
            AssertSpousesConsistent(customer2, customer0);
            AssertSpousesConsistent(customer3, null);
            AssertSpousesConsistent(null, customer1);
            AssertSpousesConsistent(null, customer2);
            AssertSpousesConsistent(null, customer3);

            // Set a dependent
            customer1.Husband = customer3;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertSpousesConsistent(customer0, null);
            AssertSpousesConsistent(customer1, customer3);
            AssertSpousesConsistent(customer2, customer0);
            AssertSpousesConsistent(customer3, null);
            AssertSpousesConsistent(null, customer1);
            AssertSpousesConsistent(null, customer2);

            // Remove a dependent
            customer2.Husband = null;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertSpousesConsistent(customer0, null);
            AssertSpousesConsistent(customer1, customer3);
            AssertSpousesConsistent(customer2, null);
            AssertSpousesConsistent(customer3, null);
            AssertSpousesConsistent(null, customer0);
            AssertSpousesConsistent(null, customer1);
            AssertSpousesConsistent(null, customer2);

            // Set a principal
            customer0.Wife = customer3;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertSpousesConsistent(customer0, null);
            AssertSpousesConsistent(customer1, customer3);
            AssertSpousesConsistent(customer2, null);
            AssertSpousesConsistent(customer3, customer0);
            AssertSpousesConsistent(null, customer1);
            AssertSpousesConsistent(null, customer2);

            // Remove a principal
            customer0.Wife = null;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertSpousesConsistent(customer0, null);
            AssertSpousesConsistent(customer1, customer3);
            AssertSpousesConsistent(customer2, null);
            AssertSpousesConsistent(customer3, null);
            AssertSpousesConsistent(null, customer0);
            AssertSpousesConsistent(null, customer1);
            AssertSpousesConsistent(null, customer2);
        }
    }

    [ConditionalFact]
    public async Task Composite_fixup_happens_when_FKs_change_test()
    {
        await CreateAndSeedDatabase(async context => await context.SeedUsingFKs());

        using (var context = CreateContext())
        {
            var product1 = context.Products.Single(e => e.Description.StartsWith("Mrs"));
            var product2 = context.Products.Single(e => e.Description.StartsWith("Chocolate"));
            var product3 = context.Products.Single(e => e.Description.StartsWith("Assorted"));

            var productReview1 = context.ProductReviews.Single(e => e.Review.StartsWith("Better"));
            var productReview2 = context.ProductReviews.Single(e => e.Review.StartsWith("Good"));
            var productReview3 = context.ProductReviews.Single(e => e.Review.StartsWith("Eeky"));

            // See issue#16428
            var sqlite = context.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";
            var productPhoto1 = sqlite
                ? context.ProductPhotos.ToList().Single(e => e.Photo[0] == 101)
                : context.ProductPhotos.Single(e => e.Photo[0] == 101);
            var productPhoto2 = sqlite
                ? context.ProductPhotos.ToList().Single(e => e.Photo[0] == 103)
                : context.ProductPhotos.Single(e => e.Photo[0] == 103);
            var productPhoto3 = sqlite
                ? context.ProductPhotos.ToList().Single(e => e.Photo[0] == 105)
                : context.ProductPhotos.Single(e => e.Photo[0] == 105);

            var productWebFeature1 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("Waffle"));
            var productWebFeature2 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("What"));

            Assert.NotNull(product2);
            AssertPhotosConsistent(productPhoto1, productWebFeature1);
            AssertPhotosConsistent(productPhoto2);
            AssertPhotosConsistent(productPhoto3);
            AssertPhotosConsistent(null, productWebFeature2);

            AssertReviewsConsistent(productReview1, productWebFeature1);
            AssertReviewsConsistent(productReview2);
            AssertReviewsConsistent(productReview3, productWebFeature2);

            // Change one part of the key
            productWebFeature1.ProductId = product3.ProductId;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertPhotosConsistent(productPhoto1);
            AssertPhotosConsistent(productPhoto2);
            AssertPhotosConsistent(productPhoto3);
            AssertPhotosConsistent(null, productWebFeature2);
            Assert.Equal(product3.ProductId, productWebFeature1.ProductId);
            Assert.Equal(productPhoto1.PhotoId, productWebFeature1.PhotoId);
            Assert.Null(productWebFeature1.Photo);

            AssertReviewsConsistent(productReview1);
            AssertReviewsConsistent(productReview2);
            AssertReviewsConsistent(productReview3, productWebFeature2);
            Assert.Equal(product3.ProductId, productWebFeature1.ProductId);
            Assert.Equal(productReview1.ReviewId, productWebFeature1.ReviewId);
            Assert.Null(productWebFeature1.Review);

            // Change the other part of the key
            productWebFeature1.ReviewId = productReview3.ReviewId;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertPhotosConsistent(productPhoto1);
            AssertPhotosConsistent(productPhoto2);
            AssertPhotosConsistent(productPhoto3);
            AssertPhotosConsistent(null, productWebFeature2);
            Assert.Equal(product3.ProductId, productWebFeature1.ProductId);
            Assert.Equal(productPhoto1.PhotoId, productWebFeature1.PhotoId);
            Assert.Null(productWebFeature1.Photo);

            AssertReviewsConsistent(productReview1);
            AssertReviewsConsistent(productReview2);
            AssertReviewsConsistent(productReview3, productWebFeature2);

            // Change both at the same time
            productWebFeature1.ReviewId = productReview1.ReviewId;
            productWebFeature1.ProductId = product1.ProductId;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertPhotosConsistent(productPhoto1, productWebFeature1);
            AssertPhotosConsistent(productPhoto2);
            AssertPhotosConsistent(productPhoto3);
            AssertPhotosConsistent(null, productWebFeature2);

            AssertReviewsConsistent(productReview1, productWebFeature1);
            AssertReviewsConsistent(productReview2);
            AssertReviewsConsistent(productReview3, productWebFeature2);
        }
    }

    [ConditionalFact]
    public async Task Fixup_with_binary_keys_happens_when_FKs_or_navigations_change_test()
    {
        await CreateAndSeedDatabase(async context => await context.SeedUsingFKs());

        using (var context = CreateContext())
        {
            var barcode1 = context.Barcodes.Single(e => e.Text == "Barcode 1 2 3 4");
            var barcode2 = context.Barcodes.Single(e => e.Text == "Barcode 2 2 3 4");
            var barcode3 = context.Barcodes.Single(e => e.Text == "Barcode 3 2 3 4");

            var incorrectScan1 = context.IncorrectScans.Single(e => e.Details.StartsWith("Treats"));
            var incorrectScan2 = context.IncorrectScans.Single(e => e.Details.StartsWith("Wot"));

            var barcodeDetails1 = context.BarcodeDetails.Single(e => e.RegisteredTo == "Eeky Bear");
            var barcodeDetails2 = context.BarcodeDetails.Single(e => e.RegisteredTo == "Trent");

            AssertBadScansConsistent(barcode1, incorrectScan2);
            AssertBadScansConsistent(barcode2, incorrectScan1);
            AssertBadScansConsistent(barcode3);

            AssertActualBarcodeConsistent(barcode1);
            AssertActualBarcodeConsistent(barcode2, incorrectScan2);
            AssertActualBarcodeConsistent(barcode3, incorrectScan1);

            AssertBarcodeDetailsConsistent(barcode1, barcodeDetails1);
            AssertBarcodeDetailsConsistent(barcode2, barcodeDetails2);
            AssertBarcodeDetailsConsistent(barcode3, null);

            // Binary FK change
            incorrectScan1.ExpectedCode = barcode3.Code.ToArray();

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertBadScansConsistent(barcode1, incorrectScan2);
            AssertBadScansConsistent(barcode2);
            AssertBadScansConsistent(barcode3, incorrectScan1);

            AssertActualBarcodeConsistent(barcode1);
            AssertActualBarcodeConsistent(barcode2, incorrectScan2);
            AssertActualBarcodeConsistent(barcode3, incorrectScan1);

            AssertBarcodeDetailsConsistent(barcode1, barcodeDetails1);
            AssertBarcodeDetailsConsistent(barcode2, barcodeDetails2);
            AssertBarcodeDetailsConsistent(barcode3, null);

            // Binary FK change
            incorrectScan2.ActualCode = barcode1.Code.ToArray();

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertBadScansConsistent(barcode1, incorrectScan2);
            AssertBadScansConsistent(barcode2);
            AssertBadScansConsistent(barcode3, incorrectScan1);

            AssertActualBarcodeConsistent(barcode1, incorrectScan2);
            AssertActualBarcodeConsistent(barcode2);
            AssertActualBarcodeConsistent(barcode3, incorrectScan1);

            AssertBarcodeDetailsConsistent(barcode1, barcodeDetails1);
            AssertBarcodeDetailsConsistent(barcode2, barcodeDetails2);
            AssertBarcodeDetailsConsistent(barcode3, null);

            // Change both back
            incorrectScan1.ExpectedCode = barcode2.Code.ToArray();
            incorrectScan2.ActualCode = barcode2.Code.ToArray();

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertBadScansConsistent(barcode1, incorrectScan2);
            AssertBadScansConsistent(barcode2, incorrectScan1);
            AssertBadScansConsistent(barcode3);

            AssertActualBarcodeConsistent(barcode1);
            AssertActualBarcodeConsistent(barcode2, incorrectScan2);
            AssertActualBarcodeConsistent(barcode3, incorrectScan1);

            AssertBarcodeDetailsConsistent(barcode1, barcodeDetails1);
            AssertBarcodeDetailsConsistent(barcode2, barcodeDetails2);
            AssertBarcodeDetailsConsistent(barcode3, null);

            // Change FK objects without changing values
            incorrectScan1.ExpectedCode = incorrectScan1.ExpectedCode.ToArray();
            incorrectScan2.ExpectedCode = incorrectScan2.ExpectedCode.ToArray();
            incorrectScan1.ActualCode = incorrectScan1.ActualCode.ToArray();
            incorrectScan2.ActualCode = incorrectScan2.ActualCode.ToArray();

            // Collection navigation changes
            barcode1.BadScans.Remove(incorrectScan2);
            barcode2.BadScans.Add(incorrectScan2);

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertBadScansConsistent(barcode1);
            AssertBadScansConsistent(barcode2, incorrectScan1, incorrectScan2);
            AssertBadScansConsistent(barcode3);

            AssertActualBarcodeConsistent(barcode1);
            AssertActualBarcodeConsistent(barcode2, incorrectScan2);
            AssertActualBarcodeConsistent(barcode3, incorrectScan1);

            AssertBarcodeDetailsConsistent(barcode1, barcodeDetails1);
            AssertBarcodeDetailsConsistent(barcode2, barcodeDetails2);
            AssertBarcodeDetailsConsistent(barcode3, null);

            // Reference navigation changes
            incorrectScan1.ExpectedBarcode = barcode1;
            incorrectScan2.ExpectedBarcode = barcode1;
            incorrectScan1.ActualBarcode = barcode3;
            incorrectScan2.ActualBarcode = barcode3;

            if (UseDetectChanges)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertBadScansConsistent(barcode1, incorrectScan1, incorrectScan2);
            AssertBadScansConsistent(barcode2);
            AssertBadScansConsistent(barcode3);

            AssertActualBarcodeConsistent(barcode1);
            AssertActualBarcodeConsistent(barcode2);
            AssertActualBarcodeConsistent(barcode3, incorrectScan1, incorrectScan2);

            AssertBarcodeDetailsConsistent(barcode1, barcodeDetails1);
            AssertBarcodeDetailsConsistent(barcode2, barcodeDetails2);
            AssertBarcodeDetailsConsistent(barcode3, null);
        }
    }

    protected void SimpleVerification()
    {
        using var context = CreateContext();
        Assert.Equal(
            new[] { "Eeky Bear", "Sheila Koalie", "Sue Pandy", "Tarquin Tiger" },
            context.Customers.Select(c => c.Name).OrderBy(n => n));

        Assert.Equal(
            new[] { "Assorted Dog Treats", "Chocolate Donuts", "Mrs Koalie's Famous Waffles" },
            context.Products.Select(c => c.Description).OrderBy(n => n));

        Assert.Equal(
            new[] { "Barcode 1 2 3 4", "Barcode 2 2 3 4", "Barcode 3 2 3 4" },
            context.Barcodes.Select(c => c.Text).OrderBy(n => n));

        Assert.Equal(
            new[] { "Barcode 1 2 3 4", "Barcode 2 2 3 4", "Barcode 3 2 3 4" },
            context.Barcodes.Select(c => c.Text).OrderBy(n => n));

        Assert.Equal(
            new[] { "Eeky Bear", "Trent" },
            context.BarcodeDetails.Select(c => c.RegisteredTo).OrderBy(n => n));

        Assert.Equal(
            new[] { "Treats not Donuts", "Wot no waffles?" },
            context.IncorrectScans.Select(c => c.Details).OrderBy(n => n));

        Assert.Equal(
            new[] { "Don't give coffee to Eeky!", "Really! Don't give coffee to Eeky!" },
            context.Complaints.Select(c => c.Details).OrderBy(n => n));

        Assert.Equal(
            new[] { "Destroyed all coffee in Redmond area." },
            context.Resolutions.Select(c => c.Details).OrderBy(n => n));

        Assert.Equal(
            new[] { "MrsBossyPants", "MrsKoalie73", "TheStripedMenace" },
            context.Logins.Select(c => c.Username).OrderBy(n => n));

        Assert.Equal(
            new[] { "Crumbs in the cupboard", "Donuts gone missing", "Pig prints on keyboard" },
            context.SuspiciousActivities.Select(c => c.Activity).OrderBy(n => n));

        Assert.Equal(
            new[] { "1234", "2234" },
            context.RsaTokens.Select(c => c.Serial).OrderBy(n => n));

        Assert.Equal(
            new[] { "MrsBossyPants", "MrsKoalie73" },
            context.SmartCards.Select(c => c.Username).OrderBy(n => n));

        Assert.Equal(
            new[] { "Rent-A-Mole" },
            context.PasswordResets.Select(c => c.TempPassword).OrderBy(n => n));

        Assert.Equal(
            new[] { "somePage1", "somePage2", "somePage3" },
            context.PageViews.Select(c => c.PageUrl).OrderBy(n => n));

        Assert.Equal(
            new[] { "MrsBossyPants", "MrsKoalie73" },
            context.LastLogins.Select(c => c.Username).OrderBy(n => n));

        Assert.Equal(
            new[] { "Fancy a cup of tea?", "I'll put the kettle on.", "Love one!" },
            context.Messages.Select(c => c.Body).OrderBy(n => n));

        Assert.Equal(
            new[] { "MrsBossyPants", "MrsKoalie73", "TheStripedMenace" },
            context.Orders.Select(c => c.Username).OrderBy(n => n));

        Assert.Equal(
            new[] { "And donuts!", "But no coffee. :-(", "Must have tea!" },
            context.OrderNotes.Select(c => c.Note).OrderBy(n => n));

        Assert.Equal(
            new[] { "Eeky Bear", "Eeky Bear", "Eeky Bear" },
            context.OrderQualityChecks.Select(c => c.CheckedBy).OrderBy(n => n));

        Assert.Equal(
            new[] { 1, 2, 3, 4, 5, 7 },
            context.OrderLines.Select(c => c.Quantity).OrderBy(n => n));

        Assert.Equal(
            new[] { "A Waffle Cart specialty!", "Eeky Bear's favorite!" },
            context.ProductDetails.Select(c => c.Details).OrderBy(n => n));

        Assert.Equal(
            new[] { "Better than Tarqies!", "Eeky says yes!", "Good with maple syrup." },
            context.ProductReviews.Select(c => c.Review).OrderBy(n => n));

        // See issue#16428
        if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            Assert.Equal(
                new[] { "101", "103", "105" },
                context.ProductPhotos.ToList().Select(c => c.Photo.First().ToString()).OrderBy(n => n));
        }
        else
        {
            Assert.Equal(
                new[] { "101", "103", "105" },
                context.ProductPhotos.Select(c => c.Photo.First().ToString()).OrderBy(n => n));
        }

        Assert.Equal(
            new[] { "Waffle Style", "What does the waffle say?" },
            context.ProductWebFeatures.Select(c => c.Heading).OrderBy(n => n));

        Assert.Equal(
            new[] { "Ants By Boris", "Trading As Trent" },
            context.Suppliers.Select(c => c.Name).OrderBy(n => n));

        Assert.Equal(
            new[] { "201", "202" },
            context.SupplierLogos.ToList().SelectMany(c => c.Logo).Select(l => l.ToString()).OrderBy(n => n));

        Assert.Equal(
            new[] { "Orange fur?", "Seems a bit dodgy.", "Very expensive!" },
            context.SupplierInformation.Select(c => c.Information).OrderBy(n => n));

        Assert.Equal(
            new[] { "Mrs Bossy Pants!", "Really likes tea." },
            context.CustomerInformation.Select(c => c.Information).OrderBy(n => n));

        Assert.Equal(
            new[] { "markash420", "unicorns420" },
            context.Computers.Select(c => c.Name).OrderBy(n => n));

        Assert.Equal(
            new[] { "It's a Dell!", "It's not a Dell!" },
            context.ComputerDetails.Select(c => c.Specifications).OrderBy(n => n));

        Assert.Equal(
            new[] { "Eeky Bear", "Splash Bear" },
            context.Drivers.Select(c => c.Name).OrderBy(n => n));

        Assert.Equal(
            new[] { "10", "11" },
            context.Licenses.Select(c => c.LicenseNumber).OrderBy(n => n));
    }

    protected void FkVerification()
    {
        using var context = CreateContext();
        var customer0 = context.Customers.Single(e => e.Name == "Eeky Bear");
        var customer1 = context.Customers.Single(e => e.Name == "Sheila Koalie");
        var customer2 = context.Customers.Single(e => e.Name == "Sue Pandy");
        var customer3 = context.Customers.Single(e => e.Name == "Tarquin Tiger");

        Assert.Null(customer0.HusbandId);
        Assert.Null(customer1.HusbandId);
        Assert.Equal(customer0.CustomerId, customer2.HusbandId);
        Assert.Null(customer3.HusbandId);

        var product1 = context.Products.Single(e => e.Description.StartsWith("Mrs"));
        var product2 = context.Products.Single(e => e.Description.StartsWith("Chocolate"));
        var product3 = context.Products.Single(e => e.Description.StartsWith("Assorted"));

        var barcode1 = context.Barcodes.Single(e => e.Text == "Barcode 1 2 3 4");
        var barcode2 = context.Barcodes.Single(e => e.Text == "Barcode 2 2 3 4");
        var barcode3 = context.Barcodes.Single(e => e.Text == "Barcode 3 2 3 4");

        Assert.Equal(product1.ProductId, barcode1.ProductId);
        Assert.Equal(product2.ProductId, barcode2.ProductId);
        Assert.Equal(product3.ProductId, barcode3.ProductId);

        var barcodeDetails1 = context.BarcodeDetails.Single(e => e.RegisteredTo == "Eeky Bear");
        var barcodeDetails2 = context.BarcodeDetails.Single(e => e.RegisteredTo == "Trent");

        Assert.Equal(barcode1.Code, barcodeDetails1.Code);
        Assert.Equal(barcode2.Code, barcodeDetails2.Code);

        var incorrectScan1 = context.IncorrectScans.Single(e => e.Details.StartsWith("Treats"));
        var incorrectScan2 = context.IncorrectScans.Single(e => e.Details.StartsWith("Wot"));

        Assert.Equal(barcode3.Code, incorrectScan1.ActualCode);
        Assert.Equal(barcode2.Code, incorrectScan1.ExpectedCode);
        Assert.Equal(barcode2.Code, incorrectScan2.ActualCode);
        Assert.Equal(barcode1.Code, incorrectScan2.ExpectedCode);

        var complaint1 = context.Complaints.Single(e => e.Details.StartsWith("Don't"));
        var complaint2 = context.Complaints.Single(e => e.Details.StartsWith("Really"));

        Assert.Equal(customer2.CustomerId, complaint1.CustomerId);
        Assert.Equal(customer2.CustomerId, complaint2.CustomerId);

        var resolution = context.Resolutions.Single(e => e.Details.StartsWith("Destroyed"));

        Assert.Equal(complaint2.AlternateId, resolution.ResolutionId);

        var login1 = context.Logins.Single(e => e.Username == "MrsKoalie73");
        var login2 = context.Logins.Single(e => e.Username == "MrsBossyPants");
        var login3 = context.Logins.Single(e => e.Username == "TheStripedMenace");

        Assert.Equal(customer1.CustomerId, login1.CustomerId);
        Assert.Equal(customer2.CustomerId, login2.CustomerId);
        Assert.Equal(customer3.CustomerId, login3.CustomerId);

        var suspiciousActivity1 = context.SuspiciousActivities.Single(e => e.Activity.StartsWith("Pig"));
        var suspiciousActivity2 = context.SuspiciousActivities.Single(e => e.Activity.StartsWith("Crumbs"));
        var suspiciousActivity3 = context.SuspiciousActivities.Single(e => e.Activity.StartsWith("Donuts"));

        Assert.Equal(login3.Username, suspiciousActivity1.Username);
        Assert.Equal(login3.Username, suspiciousActivity2.Username);
        Assert.Equal(login3.Username, suspiciousActivity3.Username);

        var rsaToken1 = context.RsaTokens.Single(e => e.Serial == "1234");
        var rsaToken2 = context.RsaTokens.Single(e => e.Serial == "2234");

        Assert.Equal(login1.Username, rsaToken1.Username);
        Assert.Equal(login2.Username, rsaToken2.Username);

        var smartCard1 = context.SmartCards.Single(e => e.Username == "MrsKoalie73");
        var smartCard2 = context.SmartCards.Single(e => e.Username == "MrsBossyPants");

        Assert.Equal(rsaToken1.Serial, smartCard1.CardSerial);
        Assert.Equal(rsaToken2.Serial, smartCard2.CardSerial);
        Assert.Equal(rsaToken1.Issued, smartCard1.Issued);
        Assert.Equal(rsaToken2.Issued, smartCard2.Issued);

        var reset1 = context.PasswordResets.Single(e => e.EmailedTo == "trent@example.com");

        Assert.Equal(login3.AlternateUsername, reset1.Username);

        var pageView1 = context.PageViews.Single(e => e.PageUrl == "somePage1");
        var pageView2 = context.PageViews.Single(e => e.PageUrl == "somePage1");
        var pageView3 = context.PageViews.Single(e => e.PageUrl == "somePage1");

        Assert.Equal(login1.Username, pageView1.Username);
        Assert.Equal(login1.Username, pageView2.Username);
        Assert.Equal(login1.Username, pageView3.Username);

        var lastLogin1 = context.LastLogins.Single(e => e.Username == "MrsKoalie73");
        var lastLogin2 = context.LastLogins.Single(e => e.Username == "MrsBossyPants");

        Assert.Equal(smartCard1.Username, lastLogin1.SmartcardUsername);
        Assert.Equal(smartCard2.Username, lastLogin2.SmartcardUsername);

        var message1 = context.Messages.Single(e => e.Body.StartsWith("Fancy"));
        var message2 = context.Messages.Single(e => e.Body.StartsWith("Love"));
        var message3 = context.Messages.Single(e => e.Body.StartsWith("I'll"));

        Assert.Equal(login1.Username, message1.FromUsername);
        Assert.Equal(login2.Username, message1.ToUsername);
        Assert.Equal(login2.Username, message2.FromUsername);
        Assert.Equal(login1.Username, message2.ToUsername);
        Assert.Equal(login1.Username, message3.FromUsername);
        Assert.Equal(login2.Username, message3.ToUsername);

        var order1 = context.Orders.Single(e => e.Username == "MrsKoalie73");
        var order2 = context.Orders.Single(e => e.Username == "MrsBossyPants");
        var order3 = context.Orders.Single(e => e.Username == "TheStripedMenace");

        Assert.Equal(customer1.CustomerId, order1.CustomerId);
        Assert.Equal(customer2.CustomerId, order2.CustomerId);
        Assert.Equal(customer3.CustomerId, order3.CustomerId);

        var orderLine1 = context.OrderLines.Single(e => e.Quantity == 7);
        var orderLine2 = context.OrderLines.Single(e => e.Quantity == 1);
        var orderLine3 = context.OrderLines.Single(e => e.Quantity == 2);
        var orderLine4 = context.OrderLines.Single(e => e.Quantity == 3);
        var orderLine5 = context.OrderLines.Single(e => e.Quantity == 4);
        var orderLine6 = context.OrderLines.Single(e => e.Quantity == 5);

        Assert.Equal(product1.ProductId, orderLine1.ProductId);
        Assert.Equal(product2.ProductId, orderLine2.ProductId);
        Assert.Equal(product3.ProductId, orderLine3.ProductId);
        Assert.Equal(product2.ProductId, orderLine4.ProductId);
        Assert.Equal(product1.ProductId, orderLine5.ProductId);
        Assert.Equal(product2.ProductId, orderLine6.ProductId);
        Assert.Equal(order1.AnOrderId, orderLine1.OrderId);
        Assert.Equal(order1.AnOrderId, orderLine2.OrderId);
        Assert.Equal(order2.AnOrderId, orderLine3.OrderId);
        Assert.Equal(order2.AnOrderId, orderLine4.OrderId);
        Assert.Equal(order2.AnOrderId, orderLine5.OrderId);
        Assert.Equal(order3.AnOrderId, orderLine6.OrderId);

        var productDetail1 = context.ProductDetails.Single(e => e.Details.StartsWith("A"));
        var productDetail2 = context.ProductDetails.Single(e => e.Details.StartsWith("Eeky"));

        Assert.Equal(product1.ProductId, productDetail1.ProductId);
        Assert.Equal(product2.ProductId, productDetail2.ProductId);

        var productReview1 = context.ProductReviews.Single(e => e.Review.StartsWith("Better"));
        var productReview2 = context.ProductReviews.Single(e => e.Review.StartsWith("Good"));
        var productReview3 = context.ProductReviews.Single(e => e.Review.StartsWith("Eeky"));

        Assert.Equal(product1.ProductId, productReview1.ProductId);
        Assert.Equal(product1.ProductId, productReview2.ProductId);
        Assert.Equal(product2.ProductId, productReview3.ProductId);

        // See issue#16428
        var sqlite = context.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";
        var productPhoto1 = sqlite
            ? context.ProductPhotos.ToList().Single(e => e.Photo[0] == 101)
            : context.ProductPhotos.Single(e => e.Photo[0] == 101);
        var productPhoto2 = sqlite
            ? context.ProductPhotos.ToList().Single(e => e.Photo[0] == 103)
            : context.ProductPhotos.Single(e => e.Photo[0] == 103);
        var productPhoto3 = sqlite
            ? context.ProductPhotos.ToList().Single(e => e.Photo[0] == 105)
            : context.ProductPhotos.Single(e => e.Photo[0] == 105);

        Assert.Equal(product1.ProductId, productPhoto1.ProductId);
        Assert.Equal(product1.ProductId, productPhoto2.ProductId);
        Assert.Equal(product3.ProductId, productPhoto3.ProductId);

        var productWebFeature1 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("Waffle"));
        var productWebFeature2 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("What"));

        Assert.Equal(product1.ProductId, productWebFeature1.ProductId);
        Assert.Equal(product2.ProductId, productWebFeature2.ProductId);
        Assert.Equal(productPhoto1.PhotoId, productWebFeature1.PhotoId);
        Assert.Null(productWebFeature2.PhotoId);
        Assert.Equal(productReview1.ReviewId, productWebFeature1.ReviewId);
        Assert.Equal(productReview3.ReviewId, productWebFeature2.ReviewId);

        var supplier1 = context.Suppliers.Single(e => e.Name.StartsWith("Trading"));
        var supplier2 = context.Suppliers.Single(e => e.Name.StartsWith("Ants"));

        var supplierLogo1 = sqlite
            ? context.SupplierLogos.ToList().Single(e => e.Logo[0] == 201)
            : context.SupplierLogos.Single(e => e.Logo[0] == 201);

        Assert.Equal(supplier1.SupplierId, supplierLogo1.SupplierId);

        var supplierInfo1 = context.SupplierInformation.Single(e => e.Information.StartsWith("Seems"));
        var supplierInfo2 = context.SupplierInformation.Single(e => e.Information.StartsWith("Orange"));
        var supplierInfo3 = context.SupplierInformation.Single(e => e.Information.StartsWith("Very"));

        Assert.Equal(supplier1.SupplierId, supplierInfo1.SupplierId);
        Assert.Equal(supplier1.SupplierId, supplierInfo2.SupplierId);
        Assert.Equal(supplier2.SupplierId, supplierInfo3.SupplierId);

        var customerInfo1 = context.CustomerInformation.Single(e => e.Information.StartsWith("Really"));
        var customerInfo2 = context.CustomerInformation.Single(e => e.Information.StartsWith("Mrs"));

        Assert.Equal(customer1.CustomerId, customerInfo1.CustomerInfoId);
        Assert.Equal(customer2.CustomerId, customerInfo2.CustomerInfoId);

        var computer1 = context.Computers.Single(e => e.Name == "markash420");
        var computer2 = context.Computers.Single(e => e.Name == "unicorns420");

        var computerDetail1 = context.ComputerDetails.Single(e => e.Specifications == "It's a Dell!");
        var computerDetail2 = context.ComputerDetails.Single(e => e.Specifications == "It's not a Dell!");

        Assert.Equal(computer1.ComputerId, computerDetail1.ComputerDetailId);
        Assert.Equal(computer2.ComputerId, computerDetail2.ComputerDetailId);

        var driver1 = context.Drivers.Single(e => e.Name == "Eeky Bear");
        var driver2 = context.Drivers.Single(e => e.Name == "Splash Bear");

        var license1 = context.Licenses.Single(e => e.LicenseNumber == "10");
        var license2 = context.Licenses.Single(e => e.LicenseNumber == "11");

        Assert.Equal(driver1.Name, license1.Name);
        Assert.Equal(driver2.Name, license2.Name);
    }

    protected void NavigationVerification()
    {
        using var context = CreateContext();
        var customer0 = context.Customers.Single(e => e.Name == "Eeky Bear");
        var customer1 = context.Customers.Single(e => e.Name == "Sheila Koalie");
        var customer2 = context.Customers.Single(e => e.Name == "Sue Pandy");
        var customer3 = context.Customers.Single(e => e.Name == "Tarquin Tiger");

        Assert.Null(customer0.Husband);
        Assert.Same(customer2, customer0.Wife);

        Assert.Null(customer1.Husband);
        Assert.Null(customer1.Wife);

        Assert.Same(customer0, customer2.Husband);
        Assert.Null(customer2.Wife);

        Assert.Null(customer3.Husband);
        Assert.Null(customer3.Wife);

        var product1 = context.Products.Single(e => e.Description.StartsWith("Mrs"));
        var product2 = context.Products.Single(e => e.Description.StartsWith("Chocolate"));
        var product3 = context.Products.Single(e => e.Description.StartsWith("Assorted"));

        var barcode1 = context.Barcodes.Single(e => e.Text == "Barcode 1 2 3 4");
        var barcode2 = context.Barcodes.Single(e => e.Text == "Barcode 2 2 3 4");
        var barcode3 = context.Barcodes.Single(e => e.Text == "Barcode 3 2 3 4");

        Assert.Same(barcode1, product1.Barcodes.Single());
        Assert.Same(product1, barcode1.Product);

        Assert.Same(barcode2, product2.Barcodes.Single());
        Assert.Same(product2, barcode2.Product);

        Assert.Same(barcode3, product3.Barcodes.Single());
        Assert.Same(product3, barcode3.Product);

        var barcodeDetails1 = context.BarcodeDetails.Single(e => e.RegisteredTo == "Eeky Bear");
        var barcodeDetails2 = context.BarcodeDetails.Single(e => e.RegisteredTo == "Trent");

        Assert.Same(barcodeDetails1, barcode1.Detail);
        Assert.Same(barcodeDetails2, barcode2.Detail);

        var incorrectScan1 = context.IncorrectScans.Single(e => e.Details.StartsWith("Treats"));
        var incorrectScan2 = context.IncorrectScans.Single(e => e.Details.StartsWith("Wot"));

        Assert.Same(barcode3, incorrectScan1.ActualBarcode);
        Assert.Same(barcode2, incorrectScan2.ActualBarcode);

        Assert.Same(barcode2, incorrectScan1.ExpectedBarcode);
        Assert.Same(incorrectScan1, barcode2.BadScans.Single());

        Assert.Same(barcode1, incorrectScan2.ExpectedBarcode);
        Assert.Same(incorrectScan2, barcode1.BadScans.Single());

        Assert.True(barcode3.BadScans == null || barcode3.BadScans.Count == 0);

        var complaint1 = context.Complaints.Single(e => e.Details.StartsWith("Don't"));
        var complaint2 = context.Complaints.Single(e => e.Details.StartsWith("Really"));

        Assert.Same(customer2, complaint1.Customer);
        Assert.Same(customer2, complaint2.Customer);

        var resolution = context.Resolutions.Single(e => e.Details.StartsWith("Destroyed"));

        Assert.Same(complaint2, resolution.Complaint);
        Assert.Same(resolution, complaint2.Resolution);

        Assert.Null(complaint1.Resolution);

        var login1 = context.Logins.Single(e => e.Username == "MrsKoalie73");
        var login2 = context.Logins.Single(e => e.Username == "MrsBossyPants");
        var login3 = context.Logins.Single(e => e.Username == "TheStripedMenace");

        Assert.Same(customer1, login1.Customer);
        Assert.Same(login1, customer1.Logins.Single());

        Assert.Same(customer2, login2.Customer);
        Assert.Same(login2, customer2.Logins.Single());

        Assert.Same(customer3, login3.Customer);
        Assert.Same(login3, customer3.Logins.Single());

        Assert.True(customer0.Logins == null || customer0.Logins.Count == 0);

        var rsaToken1 = context.RsaTokens.Single(e => e.Serial == "1234");
        var rsaToken2 = context.RsaTokens.Single(e => e.Serial == "2234");

        Assert.Same(login1, rsaToken1.Login);
        Assert.Same(login2, rsaToken2.Login);

        var smartCard1 = context.SmartCards.Single(e => e.Username == "MrsKoalie73");
        var smartCard2 = context.SmartCards.Single(e => e.Username == "MrsBossyPants");

        Assert.Same(login1, smartCard1.Login);
        Assert.Same(login2, smartCard2.Login);

        var reset1 = context.PasswordResets.Single(e => e.EmailedTo == "trent@example.com");

        Assert.Same(login3, reset1.Login);

        var pageView1 = context.PageViews.Single(e => e.PageUrl == "somePage1");
        var pageView2 = context.PageViews.Single(e => e.PageUrl == "somePage1");
        var pageView3 = context.PageViews.Single(e => e.PageUrl == "somePage1");

        Assert.Same(login1, pageView1.Login);
        Assert.Same(login1, pageView2.Login);
        Assert.Same(login1, pageView3.Login);

        var lastLogin1 = context.LastLogins.Single(e => e.Username == "MrsKoalie73");
        var lastLogin2 = context.LastLogins.Single(e => e.Username == "MrsBossyPants");

        Assert.Same(login1, lastLogin1.Login);
        Assert.Same(login2, lastLogin2.Login);

        var message1 = context.Messages.Single(e => e.Body.StartsWith("Fancy"));
        var message2 = context.Messages.Single(e => e.Body.StartsWith("Love"));
        var message3 = context.Messages.Single(e => e.Body.StartsWith("I'll"));

        Assert.Same(login1, message1.Sender);
        Assert.Same(login1, message3.Sender);
        Assert.Equal(
            new[] { "Fanc", "I'll" },
            login1.SentMessages.Select(m => m.Body.Substring(0, 4)).OrderBy(m => m).ToArray());

        Assert.Same(login2, message2.Sender);
        Assert.Same(message2, login2.SentMessages.Single());

        Assert.Same(login2, message1.Recipient);
        Assert.Same(login2, message3.Recipient);
        Assert.Equal(
            new[] { "Fanc", "I'll" },
            login2.ReceivedMessages.Select(m => m.Body.Substring(0, 4)).OrderBy(m => m).ToArray());

        Assert.Same(login1, message2.Recipient);
        Assert.Same(message2, login1.ReceivedMessages.Single());

        var order1 = context.Orders.Single(e => e.Username == "MrsKoalie73");
        var order2 = context.Orders.Single(e => e.Username == "MrsBossyPants");
        var order3 = context.Orders.Single(e => e.Username == "TheStripedMenace");

        Assert.Same(customer1, order1.Customer);
        Assert.Same(order1, customer1.Orders.Single());

        Assert.Same(customer2, order2.Customer);
        Assert.Same(order2, customer2.Orders.Single());

        Assert.Same(customer3, order3.Customer);
        Assert.Same(order3, customer3.Orders.Single());

        var orderLine1 = context.OrderLines.Single(e => e.Quantity == 7);
        var orderLine2 = context.OrderLines.Single(e => e.Quantity == 1);
        var orderLine3 = context.OrderLines.Single(e => e.Quantity == 2);
        var orderLine4 = context.OrderLines.Single(e => e.Quantity == 3);
        var orderLine5 = context.OrderLines.Single(e => e.Quantity == 4);
        var orderLine6 = context.OrderLines.Single(e => e.Quantity == 5);

        Assert.Same(product1, orderLine1.Product);
        Assert.Same(product2, orderLine2.Product);
        Assert.Same(product3, orderLine3.Product);
        Assert.Same(product2, orderLine4.Product);
        Assert.Same(product1, orderLine5.Product);
        Assert.Same(product2, orderLine6.Product);

        Assert.Same(order1, orderLine1.Order);
        Assert.Same(order1, orderLine2.Order);
        Assert.Same(order2, orderLine3.Order);
        Assert.Same(order2, orderLine4.Order);
        Assert.Same(order2, orderLine5.Order);
        Assert.Same(order3, orderLine6.Order);

        Assert.Equal(
            [orderLine2, orderLine1],
            order1.OrderLines.OrderBy(e => e.Quantity).ToArray());

        Assert.Equal(
            [orderLine3, orderLine4, orderLine5],
            order2.OrderLines.OrderBy(e => e.Quantity).ToArray());

        Assert.Same(orderLine6, order3.OrderLines.Single());

        var productDetail1 = context.ProductDetails.Single(e => e.Details.StartsWith("A"));
        var productDetail2 = context.ProductDetails.Single(e => e.Details.StartsWith("Eeky"));

        Assert.Same(product1, productDetail1.Product);
        Assert.Same(productDetail1, product1.Detail);

        Assert.Same(product2, productDetail2.Product);
        Assert.Same(productDetail2, product2.Detail);

        var productReview1 = context.ProductReviews.Single(e => e.Review.StartsWith("Better"));
        var productReview2 = context.ProductReviews.Single(e => e.Review.StartsWith("Good"));
        var productReview3 = context.ProductReviews.Single(e => e.Review.StartsWith("Eeky"));

        Assert.Same(product1, productReview1.Product);
        Assert.Same(product1, productReview2.Product);
        Assert.Equal(
            [productReview1, productReview2],
            product1.Reviews.OrderBy(r => r.Review).ToArray());

        Assert.Same(product2, productReview3.Product);
        Assert.Same(productReview3, product2.Reviews.Single());

        Assert.True(product3.Reviews == null || product3.Reviews.Count == 0);

        // See issue#16428
        var sqlite = context.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";
        var productPhoto1 = sqlite
            ? context.ProductPhotos.ToList().Single(e => e.Photo[0] == 101)
            : context.ProductPhotos.Single(e => e.Photo[0] == 101);
        var productPhoto2 = sqlite
            ? context.ProductPhotos.ToList().Single(e => e.Photo[0] == 103)
            : context.ProductPhotos.Single(e => e.Photo[0] == 103);
        var productPhoto3 = sqlite
            ? context.ProductPhotos.ToList().Single(e => e.Photo[0] == 105)
            : context.ProductPhotos.Single(e => e.Photo[0] == 105);

        Assert.Equal(
            [productPhoto1, productPhoto2],
            product1.Photos.OrderBy(r => r.Photo.First()).ToArray());

        Assert.Same(productPhoto3, product3.Photos.Single());
        Assert.True(product2.Photos == null || product2.Photos.Count == 0);

        var productWebFeature1 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("Waffle"));
        var productWebFeature2 = context.ProductWebFeatures.Single(e => e.Heading.StartsWith("What"));

        Assert.Same(productPhoto1, productWebFeature1.Photo);
        Assert.Same(productWebFeature1, productPhoto1.Features.Single());

        Assert.Same(productReview1, productWebFeature1.Review);
        Assert.Same(productWebFeature1, productReview1.Features.Single());

        Assert.Null(productWebFeature2.Photo);
        Assert.True(productPhoto2.Features == null || productPhoto2.Features.Count == 0);

        Assert.Same(productReview3, productWebFeature2.Review);
        Assert.Same(productWebFeature2, productReview3.Features.Single());

        Assert.True(productPhoto3.Features == null || productPhoto3.Features.Count == 0);
        Assert.True(productReview2.Features == null || productReview2.Features.Count == 0);

        var supplier1 = context.Suppliers.Single(e => e.Name.StartsWith("Trading"));
        var supplier2 = context.Suppliers.Single(e => e.Name.StartsWith("Ants"));

        var supplierLogo1 = sqlite
            ? context.SupplierLogos.ToList().Single(e => e.Logo[0] == 201)
            : context.SupplierLogos.Single(e => e.Logo[0] == 201);

        Assert.Same(supplierLogo1, supplier1.Logo);

        var supplierInfo1 = context.SupplierInformation.Single(e => e.Information.StartsWith("Seems"));
        var supplierInfo2 = context.SupplierInformation.Single(e => e.Information.StartsWith("Orange"));
        var supplierInfo3 = context.SupplierInformation.Single(e => e.Information.StartsWith("Very"));

        Assert.Same(supplier1, supplierInfo1.Supplier);
        Assert.Same(supplier1, supplierInfo2.Supplier);
        Assert.Same(supplier2, supplierInfo3.Supplier);

        var customerInfo1 = context.CustomerInformation.Single(e => e.Information.StartsWith("Really"));
        var customerInfo2 = context.CustomerInformation.Single(e => e.Information.StartsWith("Mrs"));

        Assert.Same(customerInfo1, customer1.Info);
        Assert.Same(customerInfo2, customer2.Info);

        var computer1 = context.Computers.Single(e => e.Name == "markash420");
        var computer2 = context.Computers.Single(e => e.Name == "unicorns420");

        var computerDetail1 = context.ComputerDetails.Single(e => e.Specifications == "It's a Dell!");
        var computerDetail2 = context.ComputerDetails.Single(e => e.Specifications == "It's not a Dell!");

        Assert.Same(computer1, computerDetail1.Computer);
        Assert.Same(computerDetail1, computer1.ComputerDetail);

        Assert.Same(computer2, computerDetail2.Computer);
        Assert.Same(computerDetail2, computer2.ComputerDetail);

        var driver1 = context.Drivers.Single(e => e.Name == "Eeky Bear");
        var driver2 = context.Drivers.Single(e => e.Name == "Splash Bear");

        var license1 = context.Licenses.Single(e => e.LicenseNumber == "10");
        var license2 = context.Licenses.Single(e => e.LicenseNumber == "11");

        Assert.Same(driver1, license1.Driver);
        Assert.Same(license1, driver1.License);

        Assert.Same(driver2, license2.Driver);
        Assert.Same(license2, driver2.License);
    }

    protected bool UseDetectChanges
        => Fixture.UseDetectChanges;

    protected Task CreateAndSeedDatabase(Func<MonsterContext, Task> seed)
        => TestStore.InitializeAsync(Fixture.ServiceProvider, CreateContext, c => seed((MonsterContext)c));

    protected MonsterContext CreateContext()
        => Fixture.CreateContext(Options);

    public virtual void Dispose()
        => TestStore.Dispose();

    public abstract class MonsterFixupFixtureBase : ServiceProviderFixtureBase
    {
        public abstract string StoreName { get; }
        public abstract bool UseDetectChanges { get; }

        public TestStore CreateTestStore()
            => TestStoreFactory.Create(StoreName);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder);

        public abstract MonsterContext CreateContext(DbContextOptions options);

        protected virtual void OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(
            ModelBuilder builder)
            where TMessage : class, IMessage
            where TProduct : class, IProduct
            where TProductPhoto : class, IProductPhoto
            where TProductReview : class, IProductReview
            where TComputerDetail : class, IComputerDetail
            where TDimensions : class, IDimensions
        {
        }
    }

    public abstract class MonsterFixupSnapshotFixtureBase : MonsterFixupFixtureBase
    {
        public override string StoreName
            => "MonsterSnapshot";

        public override bool UseDetectChanges
            => true;

        public override MonsterContext CreateContext(DbContextOptions options)
            => new SnapshotMonsterContext(options);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);
            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
            OnModelCreating<SnapshotMonsterContext.Message,
                SnapshotMonsterContext.Product,
                SnapshotMonsterContext.ProductPhoto,
                SnapshotMonsterContext.ProductReview,
                SnapshotMonsterContext.ComputerDetail,
                SnapshotMonsterContext.Dimensions>(modelBuilder);
        }
    }

    public abstract class MonsterFixupChangedOnlyFixtureBase : MonsterFixupFixtureBase
    {
        public override string StoreName
            => "MonsterChangedOnly";

        public override bool UseDetectChanges
            => false;

        public override MonsterContext CreateContext(DbContextOptions options)
            => new ChangedOnlyMonsterContext(options);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);
            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
            OnModelCreating<ChangedOnlyMonsterContext.Message,
                ChangedOnlyMonsterContext.Product,
                ChangedOnlyMonsterContext.ProductPhoto,
                ChangedOnlyMonsterContext.ProductReview,
                ChangedOnlyMonsterContext.ComputerDetail,
                ChangedOnlyMonsterContext.Dimensions>(modelBuilder);
        }
    }

    public abstract class MonsterFixupChangedChangingFixtureBase : MonsterFixupFixtureBase
    {
        public override string StoreName
            => "MonsterFullNotify";

        public override bool UseDetectChanges
            => false;

        public override MonsterContext CreateContext(DbContextOptions options)
            => new ChangedChangingMonsterContext(options);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);
            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            OnModelCreating<ChangedChangingMonsterContext.Message,
                ChangedChangingMonsterContext.Product,
                ChangedChangingMonsterContext.ProductPhoto,
                ChangedChangingMonsterContext.ProductReview,
                ChangedChangingMonsterContext.ComputerDetail,
                ChangedChangingMonsterContext.Dimensions>(modelBuilder);
        }
    }

    private static void AssertBadScansConsistent(IBarcode expectedPrincipal, params IIncorrectScan[] expectedDependents)
        => AssertConsistent(
            expectedPrincipal,
            expectedDependents,
            e => e.BadScans.NullChecked().OrderBy(m => m.Details),
            e => e.ExpectedBarcode,
            e => e.Code,
            e => e.ExpectedCode);

    private static void AssertActualBarcodeConsistent(IBarcode expectedPrincipal, params IIncorrectScan[] expectedDependents)
        => AssertConsistent(
            expectedPrincipal,
            expectedDependents,
            null,
            e => e.ActualBarcode,
            e => e.Code,
            e => e.ActualCode);

    private static void AssertPhotosConsistent(IProductPhoto expectedPrincipal, params IProductWebFeature[] expectedDependents)
        => AssertConsistent(
            expectedPrincipal,
            expectedDependents,
            e => e.Features.NullChecked().OrderBy(f => f.Heading),
            e => e.Photo,
            e => Tuple.Create(e.PhotoId, e.ProductId),
            e => e.ProductId == null || e.PhotoId == null ? null : Tuple.Create(e.PhotoId.Value, e.ProductId.Value));

    private static void AssertReviewsConsistent(IProductReview expectedPrincipal, params IProductWebFeature[] expectedDependents)
        => AssertConsistent(
            expectedPrincipal,
            expectedDependents,
            e => e.Features.NullChecked().OrderBy(f => f.Heading),
            e => e.Review,
            e => Tuple.Create(e.ReviewId, e.ProductId),
            e => e.ProductId == null ? null : Tuple.Create(e.ReviewId, e.ProductId.Value));

    private static void AssertReceivedMessagesConsistent(ILogin expectedPrincipal, params IMessage[] expectedDependents)
        => AssertConsistent(
            expectedPrincipal,
            expectedDependents,
            e => e.ReceivedMessages.NullChecked().OrderBy(m => m.Body),
            e => e.Recipient,
            e => e.Username,
            e => e.ToUsername);

    private static void AssertSentMessagesConsistent(ILogin expectedPrincipal, params IMessage[] expectedDependents)
        => AssertConsistent(
            expectedPrincipal,
            expectedDependents,
            e => e.SentMessages.NullChecked().OrderBy(m => m.Body),
            e => e.Sender,
            e => e.Username,
            e => e.FromUsername);

    private static void AssertConsistent<TPrincipal, TDependent>(
        TPrincipal expectedPrincipal,
        TDependent[] expectedDependents,
        Func<TPrincipal, IEnumerable<TDependent>> getDependents,
        Func<TDependent, TPrincipal> getPrincipal,
        Func<TPrincipal, object> getPrincipalKey,
        Func<TDependent, object> getForeignKey)
        where TPrincipal : class
        where TDependent : class
    {
        if (expectedPrincipal == null)
        {
            foreach (var dependent in expectedDependents)
            {
                Assert.Null(getPrincipal(dependent));
                Assert.Null(getForeignKey(dependent));
            }
        }
        else
        {
            var dependents = getDependents?.Invoke(expectedPrincipal).ToArray();
            var principalKey = getPrincipalKey(expectedPrincipal);

            if (getDependents != null)
            {
                Assert.Equal(expectedDependents.Length, dependents.Length);
            }

            for (var i = 0; i < expectedDependents.Length; i++)
            {
                if (getDependents != null)
                {
                    Assert.Same(expectedDependents[i], dependents[i]);
                }

                if (getPrincipal != null)
                {
                    Assert.Same(expectedPrincipal, getPrincipal(expectedDependents[i]));
                }

                Assert.Equal(principalKey, getForeignKey(expectedDependents[i]));
            }
        }
    }

    private static void AssertSpousesConsistent(ICustomer wife, ICustomer husband)
        => AssertConsistent(
            husband,
            wife,
            e => e.Wife,
            e => e.Husband,
            e => e.CustomerId,
            e => e.HusbandId);

    private static void AssertBarcodeDetailsConsistent(IBarcode principal, IBarcodeDetail dependent)
        => AssertConsistent(
            principal,
            dependent,
            e => e.Detail,
            null,
            e => e.Code,
            e => e.Code);

    private static void AssertConsistent<TPrincipal, TDependent>(
        TPrincipal expectedPrincipal,
        TDependent expectedDependent,
        Func<TPrincipal, TDependent> getDependent,
        Func<TDependent, TPrincipal> getPrincipal,
        Func<TPrincipal, object> getPrincipalKey,
        Func<TDependent, object> getForeignKey)
        where TPrincipal : class
        where TDependent : class
    {
        if (expectedDependent != null
            && getPrincipal != null)
        {
            Assert.Same(expectedPrincipal, getPrincipal(expectedDependent));
        }

        if (expectedPrincipal != null
            && getDependent != null)
        {
            Assert.Same(expectedDependent, getDependent(expectedPrincipal));
        }

        if (expectedDependent != null)
        {
            Assert.True(
                StructuralComparisons.StructuralEqualityComparer.Equals(
                    expectedPrincipal == null ? null : getPrincipalKey(expectedPrincipal),
                    getForeignKey(expectedDependent)));
        }
    }
}
