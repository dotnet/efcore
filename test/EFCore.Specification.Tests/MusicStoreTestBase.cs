// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.MusicStore;
using Microsoft.Extensions.Primitives;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class MusicStoreTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : MusicStoreTestBase<TFixture>.MusicStoreFixtureBase, new()
{
    protected MusicStoreTestBase(TFixture fixture)
    {
        Fixture = fixture;
        fixture.ListLoggerFactory.Clear();
    }

    [ConditionalFact]
    public virtual async Task Browse_ReturnsViewWithGenre()
    {
        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    const string genreName = "Genre 1";
                    CreateTestGenres(numberOfGenres: 3, numberOfAlbums: 3, context: context);

                    var controller = new StoreController(context);

                    var result = await controller.Browse(genreName);

                    Assert.Equal(genreName, result.Name);
                    Assert.NotNull(result.Albums);
                    Assert.Equal(3, result.Albums.Count);
                }
            });
    }

    [ConditionalFact]
    public virtual async Task Index_CreatesViewWithGenres()
    {
        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    CreateTestGenres(numberOfGenres: 10, numberOfAlbums: 1, context: context);

                    var controller = new StoreController(context);

                    var result = await controller.Index();

                    Assert.Equal(10, result.Count);
                }
            });
    }

    [ConditionalFact]
    public virtual async Task Details_ReturnsAlbumDetail()
    {
        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var genres = CreateTestGenres(numberOfGenres: 3, numberOfAlbums: 3, context: context);
                    var albumId = genres.First().Albums[2].AlbumId;

                    var controller = new StoreController(context);

                    var result = await controller.Details(albumId);

                    Assert.NotNull(result.Genre);
                    var genre = genres.SingleOrDefault(g => g.GenreId == result.GenreId);
                    Assert.NotNull(genre);
                    Assert.NotNull(genre.Albums.SingleOrDefault(a => a.AlbumId == albumId));
                    Assert.NotNull(result.Artist);
                    Assert.NotEqual(0, result.ArtistId);
                }
            });
    }

    private static Genre[] CreateTestGenres(int numberOfGenres, int numberOfAlbums, DbContext context)
    {
        var artist = new Artist { Name = "Artist1" };

        var genres = Enumerable.Range(1, numberOfGenres).Select(
            g =>
                new Genre
                {
                    Name = "Genre " + g,
                    Albums = Enumerable.Range(1, numberOfAlbums).Select(
                        n =>
                            new Album { Artist = artist, Title = "Greatest Hits" }).ToList()
                }).ToList();

        context.AddRange(genres);
        context.SaveChanges();

        return genres.ToArray();
    }

    [ConditionalFact]
    public virtual async Task Index_GetsSixTopAlbums()
    {
        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var controller = new HomeController();

                    var albums = TestAlbumDataProvider.GetAlbums();

                    foreach (var album in albums)
                    {
                        await context.AddAsync(album);
                    }

                    await context.SaveChangesAsync();

                    var result = await controller.Index(context);

                    Assert.Equal(6, result.Count);
                }
            });
    }

    private static class TestAlbumDataProvider
    {
        public static Album[] GetAlbums()
        {
            var genres = Enumerable.Range(1, 10).Select(
                n =>
                    new Genre { Name = "Genre Name " + n }).ToArray();

            var artists = Enumerable.Range(1, 10).Select(
                n =>
                    new Artist { Name = "Artist Name " + n }).ToArray();

            var albums = Enumerable.Range(1, 10).Select(
                n =>
                    new Album
                    {
                        Artist = artists[n - 1],
                        Genre = genres[n - 1],
                        Title = "Greatest Hits"
                    }).ToArray();

            return albums;
        }
    }

    [ConditionalFact]
    public virtual async Task GenreMenuComponent_Returns_NineGenres()
    {
        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var genreMenuComponent = new GenreMenuComponent(context);

                    var genres = Enumerable.Range(1, 10).Select(
                        n => new Genre { Name = $"G{n}" });

                    context.AddRange(genres);
                    context.SaveChanges();

                    var result = await genreMenuComponent.InvokeAsync();

                    Assert.Equal(9, result.Count);
                }
            });
    }

    [ConditionalFact]
    public virtual async Task AddressAndPayment_RedirectToCompleteWhenSuccessful()
    {
        const string cartId = "CartId_A";

        var order = CreateOrder();

        var formCollection = new Dictionary<string, StringValues> { { "PromoCode", new[] { "FREE" } } };

        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var cartItems = CreateTestCartItems(cartId, itemPrice: 10, numberOfItems: 1);
                    context.AddRange(cartItems.Select(n => n.Album).Distinct());
                    context.AddRange(cartItems);
                    context.SaveChanges();

                    var controller = new CheckoutController(formCollection);

                    var result = await controller.AddressAndPayment(context, cartId, order);

                    Assert.Equal(order.OrderId, result);
                }
            });
    }

    [ConditionalFact]
    public virtual async Task AddressAndPayment_ReturnsOrderIfInvalidPromoCode()
    {
        const string cartId = "CartId_A";

        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var controller = new CheckoutController();
                    var order = CreateOrder();

                    var result = await controller.AddressAndPayment(context, cartId, order);

                    Assert.Null(result);
                }
            });
    }

    protected Order CreateOrder(string userName = "RainbowDash")
        => new()
        {
            Username = userName,
            FirstName = "Macavity",
            LastName = "Clark",
            Address = "11 Meadow Drive",
            City = "Healing",
            State = "IA",
            PostalCode = "DN37 7RU",
            Country = "USK",
            Phone = "555 887876",
            Email = "mc@sample.com"
        };

    [ConditionalFact]
    public virtual async Task Complete_ReturnsOrderIdIfValid()
    {
        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var controller = new CheckoutController();

                    var order = (await context.AddAsync(CreateOrder())).Entity;
                    await context.SaveChangesAsync();

                    var result = await controller.Complete(context, order.OrderId);

                    Assert.Equal(order.OrderId, result);
                }
            });
    }

    [ConditionalFact]
    public virtual async Task Complete_ReturnsErrorIfInvalidOrder()
    {
        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var controller = new CheckoutController();

                    var result = await controller.Complete(context, -3333);

                    Assert.Equal("Error", result);
                }
            });
    }

    [ConditionalFact]
    public virtual async Task CartSummaryComponent_returns_items()
    {
        const string cartId = "CartId_A";
        const string albumTitle = "Good goat, M.A.A.D Village";
        const int itemCount = 10;

        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var album = new Album
                    {
                        Title = albumTitle,
                        Artist = new Artist { Name = "Kung Fu Kenny" },
                        Genre = new Genre { Name = "Rap" }
                    };

                    var cartItems = Enumerable.Range(1, itemCount).Select(
                        n =>
                            new CartItem
                            {
                                Album = album,
                                Count = 1,
                                CartId = cartId
                            }).ToArray();

                    context.AddRange(cartItems);
                    context.SaveChanges();

                    var result = await new CartSummaryComponent(context, cartId).InvokeAsync();

                    Assert.Equal(itemCount, result.CartCount);
                    Assert.Equal(albumTitle, result.CartSummary);
                }
            });
    }

    [ConditionalFact]
    public virtual async void Music_store_project_to_mapped_entity()
    {
        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var albums = CreateTestAlbums(
                        10,
                        new Artist { Name = "Kung Fu Kenny" }, new Genre { Name = "Rap" });

                    context.Albums.AddRange(albums);
                    await context.SaveChangesAsync();

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
                                Artist = new Artist { ArtistId = album.ArtistId, Name = artist.Name },
                                Genre = new Genre { GenreId = album.GenreId, Name = genre.Name }
                            };

                    var foundAlbums = q.ToList();

                    Assert.Equal(10, foundAlbums.Count);
                }
            });
    }

    [ConditionalFact]
    public virtual async Task RemoveFromCart_removes_items_from_cart()
    {
        const string cartId = "CartId_A";
        const int numberOfItems = 5;
        const int unitPrice = 10;

        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var cartItems = CreateTestCartItems(cartId, unitPrice, numberOfItems);
                    context.AddRange(cartItems.Select(n => n.Album).Distinct());
                    context.AddRange(cartItems);
                    context.SaveChanges();

                    var controller = new ShoppingCartController(context, cartId);

                    var cartItemId = cartItems[2].CartItemId;
                    var viewModel = await controller.RemoveFromCart(cartItemId);

                    Assert.Equal(numberOfItems - 1, viewModel.CartCount);
                    Assert.Equal((numberOfItems - 1) * 10, viewModel.CartTotal);
                    Assert.Equal("Greatest Hits has been removed from your shopping cart.", viewModel.Message);

                    var cart = ShoppingCart.GetCart(context, cartId);
                    Assert.DoesNotContain((await cart.GetCartItems()), c => c.CartItemId == cartItemId);
                }
            });
    }

    [ConditionalTheory]
    [InlineData(null)]
    [InlineData("CartId_A")]
    public virtual async Task Cart_is_empty_when_no_items_have_been_added(string cartId)
    {
        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var controller = new ShoppingCartController(context, cartId);
                    var viewModel = await controller.Index();

                    Assert.Empty(viewModel.CartItems);
                    Assert.Equal(0, viewModel.CartTotal);
                }
            });
    }

    [ConditionalFact]
    public virtual async Task Cart_has_items_once_they_have_been_added()
    {
        const string cartId = "CartId_A";

        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var cartItems = CreateTestCartItems(
                        cartId,
                        itemPrice: 10,
                        numberOfItems: 5);

                    context.AddRange(cartItems.Select(n => n.Album).Distinct());
                    context.AddRange(cartItems);
                    context.SaveChanges();

                    var controller = new ShoppingCartController(context, cartId);
                    var viewModel = await controller.Index();

                    Assert.Equal(5, viewModel.CartItems.Count);
                    Assert.Equal(5 * 10, viewModel.CartTotal);
                }
            });
    }

    [ConditionalFact]
    public virtual async Task Can_add_items_to_cart()
    {
        const string cartId = "CartId_A";

        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                using (Fixture.BeginTransaction(context))
                {
                    var albums = CreateTestAlbums(
                        10,
                        new Artist { Name = "Kung Fu Kenny" }, new Genre { Name = "Rap" });

                    context.AddRange(albums);
                    context.SaveChanges();

                    var controller = new ShoppingCartController(context, cartId);
                    var albumId = albums[2].AlbumId;
                    await controller.AddToCart(albumId);

                    var cart = ShoppingCart.GetCart(context, cartId);
                    Assert.Single(await cart.GetCartItems());
                    Assert.Equal(albumId, (await cart.GetCartItems()).Single().AlbumId);
                }
            });
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Custom_projection_FirstOrDefault_works(bool async)
    {
        using var context = CreateContext();
        var shoppingCartId = "CartId_A";
        var id = 1;
        var query = context.CartItems
            .Select(
                ci => new CartItem
                {
                    CartId = ci.CartId,
                    CartItemId = ci.CartItemId,
                    Count = ci.Count,
                    Album = new Album { Title = ci.Album.Title }
                });

        var cartItem = async
            ? await query.FirstOrDefaultAsync(ci => ci.CartId == shoppingCartId && ci.CartItemId == id)
            : query.FirstOrDefault(ci => ci.CartId == shoppingCartId && ci.CartItemId == id);

        Assert.Null(cartItem);
    }

    private static CartItem[] CreateTestCartItems(string cartId, decimal itemPrice, int numberOfItems)
    {
        var albums = CreateTestAlbums(
            itemPrice,
            new Artist { Name = "Kung Fu Kenny" }, new Genre { Name = "Rap" });

        var cartItems = Enumerable.Range(1, numberOfItems).Select(
            n => new CartItem
            {
                Count = 1,
                CartId = cartId,
                Album = albums[n % albums.Length]
            }).ToArray();

        return cartItems;
    }

    private static Album[] CreateTestAlbums(decimal itemPrice, Artist artist, Genre genre)
        => Enumerable.Range(1, 10).Select(
            n =>
                new Album
                {
                    Title = "Greatest Hits",
                    Price = itemPrice,
                    Artist = artist,
                    Genre = genre
                }).ToArray();

    protected class CartSummaryComponent(MusicStoreContext context, string cartId)
    {
        private readonly MusicStoreContext _context = context;
        private readonly string _cartId = cartId;

        public async Task<CartSummaryViewBag> InvokeAsync()
        {
            var cartItems = await ShoppingCart.GetCart(_context, _cartId).GetCartItems();

            var viewBag = new CartSummaryViewBag
            {
                CartCount = cartItems.Sum(c => c.Count),
                CartSummary = string.Join("\n", cartItems.Select(c => c.Album.Title).Distinct())
            };

            return viewBag;
        }
    }

    protected class CartSummaryViewBag
    {
        public int CartCount { get; set; }
        public string CartSummary { get; set; }
    }

    protected class ShoppingCartController(MusicStoreContext context, string cartId)
    {
        private readonly MusicStoreContext _context = context;
        private readonly string _cartId = cartId;

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

            var viewModel = new ShoppingCartViewModel { CartItems = await cart.GetCartItems(), CartTotal = await cart.GetTotal() };

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

    public class CheckoutController(Dictionary<string, StringValues> formCollection = null)
    {
        private readonly Dictionary<string, StringValues> _formCollection = formCollection ?? new Dictionary<string, StringValues>();
        private const string PromoCode = "FREE";

        public async Task<object> AddressAndPayment(MusicStoreContext context, string cartId, Order order)
        {
            try
            {
                if (!string.Equals(
                        _formCollection["PromoCode"].FirstOrDefault(),
                        PromoCode,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                order.Username = "RainbowDash";
                order.OrderDate = DateTime.Now;
                context.Orders.Add(order);

                var cart = ShoppingCart.GetCart(context, cartId);
                await cart.CreateOrder(order);
                await context.SaveChangesAsync();

                return order.OrderId;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<object> Complete(MusicStoreContext context, int id)
        {
            var userName = "RainbowDash";

            var isValid = await context.Orders.AnyAsync(
                o => o.OrderId == id && o.Username == userName);

            if (isValid)
            {
                return id;
            }

            return "Error";
        }
    }

    public class GenreMenuComponent(MusicStoreContext context)
    {
        private readonly MusicStoreContext _context = context;

        public async Task<List<string>> InvokeAsync()
        {
            var genres = await _context.Genres.OrderBy(e => e.GenreId).Select(g => g.Name).Take(9).ToListAsync();

            return genres;
        }
    }

    public class HomeController
    {
        public async Task<List<Album>> Index(MusicStoreContext context)
        {
            var albums = await GetTopSellingAlbumsAsync(context, 6);

            return albums;
        }

        private Task<List<Album>> GetTopSellingAlbumsAsync(MusicStoreContext dbContext, int count)
            => dbContext.Albums
                .OrderByDescending(a => a.OrderDetails.Count)
                .Take(count)
                .ToListAsync();
    }

    public class StoreController(MusicStoreContext context)
    {
        private readonly MusicStoreContext _context = context;

        public async Task<List<Genre>> Index()
        {
            var genres = await _context.Genres.ToListAsync();

            return genres;
        }

        public async Task<Genre> Browse(string genre)
        {
            var genreModel = await _context.Genres
                .Include(g => g.Albums)
                .Where(g => g.Name == genre)
                .FirstOrDefaultAsync();

            return genreModel;
        }

        public async Task<Album> Details(int id)
        {
            var album = await _context.Albums
                .Where(a => a.AlbumId == id)
                .Include(a => a.Artist)
                .Include(a => a.Genre)
                .FirstOrDefaultAsync();

            return album;
        }
    }

    protected TFixture Fixture { get; }

    protected MusicStoreContext CreateContext()
        => Fixture.CreateContext();

    public abstract class MusicStoreFixtureBase : SharedStoreFixtureBase<MusicStoreContext>
    {
        public virtual IDisposable BeginTransaction(DbContext context)
            => context.Database.BeginTransaction();

        protected override string StoreName
            => "MusicStore";

        protected override bool UsePooling
            => false;
    }
}
