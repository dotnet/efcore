// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.MusicStore;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class MusicStoreTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : MusicStoreTestBase<TFixture>.MusicStoreFixtureBase, new()
    {
        protected MusicStoreTestBase(TFixture fixture)
        {
            Fixture = fixture;
            fixture.ListLoggerFactory.Clear();
        }

        [ConditionalFact]
        public void Music_store_project_to_mapped_entity()
        {
            using (var context = CreateContext())
            {
                using (Fixture.BeginTransaction(context))
                {
                    var albums = CreateTestAlbums(
                        10,
                        new Artist
                        {
                            ArtistId = 1, Name = "Kung Fu Kenny"
                        }, new Genre
                        {
                            GenreId = 1, Name = "Rap"
                        });

                    context.Albums.AddRange(albums);
                    context.SaveChanges();

                    var q = from album in context.Albums
                            join genre in context.Genres on album.GenreId equals genre.GenreId
                            join artist in context.Artists on album.ArtistId equals artist.ArtistId
                            select new Album
                            {
                                ArtistId = album.ArtistId,
                                AlbumArtUrl = album.AlbumArtUrl,
                                AlbumId = album.AlbumId,
                                GenreId = album.GenreId,
                                Price = album.Price,
                                Title = album.Title,
                                Artist = new Artist
                                {
                                    ArtistId = album.ArtistId, Name = artist.Name
                                },
                                Genre = new Genre
                                {
                                    GenreId = album.GenreId, Name = genre.Name
                                }
                            };

                    var foundAlbums = q.ToList();

                    Assert.Equal(10, foundAlbums.Count);
                }
            }
        }

        [ConditionalFact]
        public async Task RemoveFromCart_removes_items_from_cart()
        {
            const string cartId = "CartId_A";
            const int cartItemId = 3;
            const int numberOfItem = 5;
            const int unitPrice = 10;

            using (var context = CreateContext())
            {
                using (Fixture.BeginTransaction(context))
                {
                    var cartItems = CreateTestCartItems(cartId, unitPrice, numberOfItem);
                    context.AddRange(cartItems.Select(n => n.Album).Distinct());
                    context.AddRange(cartItems);
                    context.SaveChanges();

                    var controller = new ShoppingCartController(context, cartId);

                    var viewModel = await controller.RemoveFromCart(cartItemId);

                    Assert.Equal(numberOfItem - 1, viewModel.CartCount);
                    Assert.Equal((numberOfItem - 1) * 10, viewModel.CartTotal);
                    Assert.Equal("Greatest Hits has been removed from your shopping cart.", viewModel.Message);

                    var cart = ShoppingCart.GetCart(context, cartId);
                    Assert.DoesNotContain((await cart.GetCartItems()), c => c.CartItemId == cartItemId);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(null)]
        [InlineData("CartId_A")]
        public async Task Cart_is_empty_when_no_items_have_been_added(string cartId)
        {
            using (var context = CreateContext())
            {
                using (Fixture.BeginTransaction(context))
                {
                    var controller = new ShoppingCartController(context, cartId);
                    var viewModel = await controller.Index();

                    Assert.Empty(viewModel.CartItems);
                    Assert.Equal(0, viewModel.CartTotal);
                }
            }
        }

        [Fact]
        public async Task Cart_has_items_once_they_have_been_added()
        {
            const string cartId = "CartId_A";

            using (var context = CreateContext())
            {
                using (Fixture.BeginTransaction(context))
                {
                    var cartItems = CreateTestCartItems(
                        cartId,
                        itemPrice: 10,
                        numberOfItem: 5);

                    context.AddRange(cartItems.Select(n => n.Album).Distinct());
                    context.AddRange(cartItems);
                    context.SaveChanges();

                    var controller = new ShoppingCartController(context, cartId);
                    var viewModel = await controller.Index();

                    Assert.Equal(5, viewModel.CartItems.Count);
                    Assert.Equal(5 * 10, viewModel.CartTotal);
                }
            }
        }

        [Fact]
        public async Task Can_add_items_to_cart()
        {
            const string cartId = "CartId_A";
            const int albumId = 3;


            using (var context = CreateContext())
            {
                using (Fixture.BeginTransaction(context))
                {
                    var albums = CreateTestAlbums(
                        10,
                        new Artist
                        {
                            ArtistId = 1, Name = "Kung Fu Kenny"
                        }, new Genre
                        {
                            GenreId = 1, Name = "Rap"
                        });

                    context.AddRange(albums);
                    context.SaveChanges();

                    var controller = new ShoppingCartController(context, cartId);
                    await controller.AddToCart(albumId);

                    var cart = ShoppingCart.GetCart(context, cartId);
                    Assert.Single(await cart.GetCartItems());
                    Assert.Equal(albumId, (await cart.GetCartItems()).Single().AlbumId);
                }
            }
        }

        private static CartItem[] CreateTestCartItems(string cartId, decimal itemPrice, int numberOfItem)
        {
            var albums = CreateTestAlbums(
                itemPrice, new Artist
                {
                    ArtistId = 1, Name = "Kung Fu Kenny"
                }, new Genre
                {
                    GenreId = 1, Name = "Rap"
                });

            var cartItems = Enumerable.Range(1, numberOfItem).Select(
                n =>
                    new CartItem()
                    {
                        Count = 1, CartId = cartId, AlbumId = n % albums.Length, Album = albums[n % albums.Length],
                    }).ToArray();

            return cartItems;
        }

        private static Album[] CreateTestAlbums(decimal itemPrice, Artist artist, Genre genre)
        {
            return Enumerable.Range(1, 10).Select(
                n =>
                    new Album
                    {
                        Title = "Greatest Hits",
                        AlbumId = n,
                        Price = itemPrice,
                        Artist = artist,
                        Genre = genre
                    }).ToArray();
        }

        protected class ShoppingCartController
        {
            private readonly MusicStoreContext _context;
            private readonly string _cartId;

            public ShoppingCartController(MusicStoreContext context, string cartId)
            {
                _context = context;
                _cartId = cartId;
            }

            public virtual async Task<ShoppingCartRemoveViewModel> RemoveFromCart(int cartItemId)
            {
                var cart = ShoppingCart.GetCart(_context, _cartId);

                var cartItem = await _context.CartItems
                    .Where(item => item.CartItemId == cartItemId)
                    .Include(c => c.Album)
                    .SingleOrDefaultAsync();

                string message;
                int itemCount;
                if (cartItem != null)
                {
                    itemCount = cart.RemoveFromCart(cartItemId);

                    await _context.SaveChangesAsync();

                    var removed = (itemCount > 0) ? " 1 copy of " : string.Empty;
                    message = removed + cartItem.Album.Title + " has been removed from your shopping cart.";
                }
                else
                {
                    itemCount = 0;
                    message = "Could not find this item, nothing has been removed from your shopping cart.";
                }

                var viewModel = new ShoppingCartRemoveViewModel
                {
                    Message = message,
                    CartTotal = await cart.GetTotal(),
                    CartCount = await cart.GetCount(),
                    ItemCount = itemCount,
                    DeleteId = cartItemId
                };

                return viewModel;
            }

            public virtual async Task<ShoppingCartViewModel> Index()
            {
                var cart = ShoppingCart.GetCart(_context, _cartId);

                var viewModel = new ShoppingCartViewModel
                {
                    CartItems = await cart.GetCartItems(), CartTotal = await cart.GetTotal()
                };

                return viewModel;
            }

            public async Task AddToCart(int id)
            {
                var addedAlbum = await _context.Albums.SingleAsync(album => album.AlbumId == id);

                var cart = ShoppingCart.GetCart(_context, _cartId);

                await cart.AddToCart(addedAlbum);

                await _context.SaveChangesAsync();
            }
        }

        protected TFixture Fixture { get; }

        protected MusicStoreContext CreateContext() => Fixture.CreateContext();

        public abstract class MusicStoreFixtureBase : SharedStoreFixtureBase<MusicStoreContext>
        {
            public virtual IDisposable BeginTransaction(DbContext context) => context.Database.BeginTransaction();

            protected override string StoreName { get; } = "MusicStore";

            protected override bool UsePooling => false;
        }
    }
}
