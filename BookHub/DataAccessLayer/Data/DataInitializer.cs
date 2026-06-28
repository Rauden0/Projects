using Bogus;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Data;

public static class DataInitializer
{
    private const int BooksCount = 40;
    private const int UsersCount = 30;
    private const int AuthorsCount = 25;
    private const int PublishersCount = 15;
    private const int GenresCount = 12;
    private const int ReviewsCount = 50;
    private const int WishlistsCount = 35;
    private const int OrdersCount = 20;
    private const int CartCount = 5;
    private const int GiftCardsCount = 10;
    private const int CouponsCount = 15;
    private static List<string> imagePaths = new();

    public static void Seed(this ModelBuilder modelBuilder)
    {
        Randomizer.Seed = new Random(1337);

        imagePaths.Clear();
        imagePaths.Add("/images/books/book_9c2ae1b7-29ee-4dda-8bd5-2e0bad87a95c.png");
        imagePaths.Add("/images/books/book_eca6d865-83b4-44d7-8721-7ab42aaf9b1d.png");
        imagePaths.Add("/images/books/book_f65e10d1-04b9-441b-a556-fb7d52618c60.png");

        SeedGenres(modelBuilder);
        SeedPublishers(modelBuilder);
        SeedAuthors(modelBuilder);
        SeedUsers(modelBuilder);
        SeedGiftCards(modelBuilder);
        SeedCoupons(modelBuilder);

        var books = SeedBooksWithGenres(modelBuilder);

        SeedBookAuthors(modelBuilder);
        SeedReviews(modelBuilder);
        var orders = SeedOrders(modelBuilder);
        SeedOrdersItems(modelBuilder, books, orders);
        var carts = SeedCarts(modelBuilder, orders);
        SeedCartItems(modelBuilder, carts);
        SeedWishLists(modelBuilder);
        SeedWishlistItems(modelBuilder);
    }

    private static List<Book> SeedBooksWithGenres(ModelBuilder modelBuilder)
    {
        var rnd = new Random(1337);
        var faker = new Faker<Book>("cz")
            .RuleFor(b => b.Id, f => f.IndexFaker + 1)
            .RuleFor(b => b.Name, f => f.Lorem.Sentence(3))
            .RuleFor(b => b.Description, f => f.Lorem.Sentence(12))
            .RuleFor(b => b.Price, f => f.Random.Float(150, 450))
            .RuleFor(b => b.StockQuantity, 20)
            .RuleFor(b => b.PublisherId, f => f.Random.Int(1, PublishersCount))
            .RuleFor(b => b.CreatedAt, new DateTime(2024, 01, 01));

        var books = faker.Generate(BooksCount);
        var bookGenreLinks = new List<object>();
        var usedPairs = new HashSet<(int BookId, int GenreId)>();

        for (var genreId = 1; genreId <= GenresCount; genreId++)
        {
            int bookId = rnd.Next(1, BooksCount + 1);
            usedPairs.Add((bookId, genreId));
            bookGenreLinks.Add(new { BooksId = bookId, GenresId = genreId });
        }

        foreach (var book in books)
        {
            book.ImagePath = imagePaths[rnd.Next(0, imagePaths.Count)];

            var currentBookGenres = new List<int>();
            var genreCount = rnd.Next(1, 3);

            for (int i = 0; i < genreCount; i++)
            {
                int genreId;
                do
                {
                    genreId = rnd.Next(1, GenresCount + 1);
                } while (usedPairs.Contains((book.Id, genreId)) && usedPairs.Count < BooksCount * GenresCount);

                if (usedPairs.Add((book.Id, genreId)))
                {
                    bookGenreLinks.Add(new { BooksId = book.Id, GenresId = genreId });
                }

                currentBookGenres.Add(genreId);
            }

            if (currentBookGenres.Count > 0)
            {
                book.PrimaryGenreId = currentBookGenres[rnd.Next(currentBookGenres.Count)];
            }
        }

        modelBuilder.Entity<Book>().HasData(books);
        modelBuilder.Entity("BookGenre").HasData(bookGenreLinks);

        return books;
    }
    private static void SeedGiftCards(ModelBuilder modelBuilder)
    {
        var faker = new Faker<GiftCard>("cz")
            .RuleFor(g => g.Id, f => f.IndexFaker + 1)
            .RuleFor(g => g.ReductionAmount, f => f.Random.Int(1, 10) * 100) // 100, 200... 1000 CZK
            .RuleFor(g => g.ValidFrom, new DateTime(2024, 01, 01))
            .RuleFor(g => g.ValidTo, new DateTime(2025, 12, 31));

        var giftCards = faker.Generate(GiftCardsCount);
        modelBuilder.Entity<GiftCard>().HasData(giftCards);
    }

    private static void SeedCoupons(ModelBuilder modelBuilder)
    {
        var rng = new Random(1337);
        var faker = new Faker<Coupon>("cz")
            .RuleFor(c => c.Id, f => f.IndexFaker + 1)
            .RuleFor(c => c.Code, f => $"GIFT-{f.Random.Replace("####-####")}")
            .RuleFor(c => c.IsUsed, f => f.Random.Bool(0.2f))
            .RuleFor(c => c.GiftCardId, f => f.Random.Int(1, GiftCardsCount))
            .RuleFor(c => c.OrderId, _ => null);

        var coupons = faker.Generate(CouponsCount);
        modelBuilder.Entity<Coupon>().HasData(coupons);
    }
    private static void SeedGenres(ModelBuilder modelBuilder)
    {
        var faker = new Faker<Genre>("cz")
            .RuleFor(g => g.Id, f => f.IndexFaker + 1)
            .RuleFor(g => g.Name, f => f.Lorem.Word());

        var genres = faker.Generate(GenresCount);
        modelBuilder.Entity<Genre>().HasData(genres);
    }

    private static void SeedPublishers(ModelBuilder modelBuilder)
    {
        var faker = new Faker<Publisher>("cz")
            .RuleFor(p => p.Id, f => f.IndexFaker + 1)
            .RuleFor(p => p.Name, f => f.Company.CompanyName());

        var publishers = faker.Generate(PublishersCount);
        modelBuilder.Entity<Publisher>().HasData(publishers);
    }

    private static void SeedAuthors(ModelBuilder modelBuilder)
    {
        var faker = new Faker<Author>("cz")
            .RuleFor(a => a.Id, f => f.IndexFaker + 1)
            .RuleFor(a => a.Name, f => f.Name.FirstName() + " " + f.Name.LastName());

        var authors = faker.Generate(AuthorsCount);
        modelBuilder.Entity<Author>().HasData(authors);
    }

    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        var user = new Faker<User>("cz")
            .RuleFor(u => u.Id, f => f.IndexFaker + 1)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.DisplayName, f => f.Internet.UserName())
            .RuleFor(u => u.Role, RoleType.User)
            .RuleFor(u => u.PasswordHash, f => $"HASHED_{f.Internet.UserName()}");

        var users = user.Generate(UsersCount);

        users[0].Role = RoleType.Admin;
        users[0].Email = "admin@bookhub.cz";
        users[0].DisplayName = "Admin";
        users[0].PasswordHash = "HASHED_admin";
        users[18].Email ="bara@mail.com";
        modelBuilder.Entity<User>().HasData(users);
    }

    /**
     * Add 1-3 authors for every book
     */
    private static void SeedBookAuthors(ModelBuilder modelBuilder)
    {
        var bookAuthors = new List<object>();
        var rng = new Random(1337);

        // Global protection against duplicate pairs
        var usedPairs = new HashSet<(int AuthorId, int BookId)>();

        // Every author has 1 book minimal  
        for (var authorId = 1; authorId <= AuthorsCount; authorId++)
        {
            int bookId;
            do
            {
                bookId = rng.Next(1, BooksCount + 1);
            } while (!usedPairs.Add((authorId, bookId)));

            bookAuthors.Add(new { AuthorsId = authorId, BooksId = bookId });
        }

        // For every book add 1–3 unique authors
        for (var bookId = 1; bookId <= BooksCount; bookId++)
        {
            var authorCount = rng.Next(1, 4);

            for (int i = 0; i < authorCount; i++)
            {
                int authorId;
                do
                {
                    authorId = rng.Next(1, AuthorsCount + 1);
                } while (!usedPairs.Add((authorId, bookId))); // Ensure global uniqueness

                bookAuthors.Add(new { AuthorsId = authorId, BooksId = bookId });
            }
        }

        modelBuilder.Entity("BookAuthor").HasData(bookAuthors);
    }


    private static void SeedReviews(ModelBuilder modelBuilder)
    {
        var review = new Faker<Review>("cz")
            .RuleFor(r => r.Id, f => f.IndexFaker + 1)
            .RuleFor(r => r.UserId, f => f.Random.Int(1, UsersCount))
            .RuleFor(r => r.BookId, f => f.Random.Int(1, BooksCount))
            .RuleFor(r => r.Rating, f => f.PickRandom<Rating>())
            .RuleFor(r => r.Comment, f => f.Lorem.Sentence())
            .RuleFor(r => r.CreatedAt, new DateTime(2024, 01, 01));

        var reviews = review.Generate(ReviewsCount);
        modelBuilder.Entity<Review>().HasData(reviews);
    }

    private static List<Order> SeedOrders(ModelBuilder modelBuilder)
    {
        var order = new Faker("cz");
        var orderStates = Enum.GetValues<OrderStateEnum>();
        var paymentMethods = Enum.GetValues<PaymentMethodEnum>();
        var processingUsers = new HashSet<int>();

        var orders = new List<Order>(OrdersCount);

        for (var orderId = 1; orderId <= OrdersCount; orderId++)
        {
            var userId = order.Random.Int(1, UsersCount);
            var state = order.PickRandom(orderStates);

            // paid logic
            bool paid = state switch
            {
                OrderStateEnum.Completed => true,
                _ => order.Random.Bool(0.7f)
            };

            // paymentMethod logic
            var paymentMethod = order.PickRandom(paymentMethods);

            if (state == OrderStateEnum.Sending && !paid)
                paymentMethod = PaymentMethodEnum.Cash;

            orders.Add(new Order
            {
                Id = orderId,
                UserId = userId,
                PaymentMethod = paymentMethod,
                OrderState = state,
                Paid = paid,
                TotalPrice = 0f,
                CreatedAt = new DateTime(2024, 01, 01),
                FirstName = order.Person.FirstName,
                LastName = order.Person.LastName,
                Email = order.Internet.Email(),
                PhoneNumber = order.Phone.PhoneNumber(),
                Street = order.Address.StreetAddress(),
                City = order.Address.City(),
                PostalCode = order.Address.ZipCode(),
                Country = order.Address.Country(),
                IsRemoved = false
            });
        }

        return orders;
    }

    private static void SeedOrdersItems(ModelBuilder modelBuilder, List<Book> books, List<Order> orders)
    {
        var rng = new Random(1337);

        var itemFaker = new Faker<OrderItem>("cz")
            .RuleFor(oi => oi.Quantity, f => f.Random.Int(1, 3))
            .RuleFor(oi => oi.AddedAt, new DateTime(2024, 01, 01));

        var orderItems = new List<OrderItem>();

        var itemId = 1;

        for (var orderId = 1; orderId <= OrdersCount; orderId++)
        {
            var usedBooks = new HashSet<int>();
            var itemsCount = rng.Next(1, 4);

            for (var i = 0; i < itemsCount; i++)
            {
                int bookId;
                do
                {
                    bookId = rng.Next(1, BooksCount + 1);
                } while (!usedBooks.Add(bookId));

                var book = books[bookId - 1];

                var item = itemFaker.Generate();
                item.Id = itemId++;
                item.OrderId = orderId;
                item.BookId = bookId;

                item.Price = book.Price;

                orderItems.Add(item);

                orders[orderId - 1].TotalPrice += item.Price * item.Quantity;
            }
        }

        modelBuilder.Entity<Order>().HasData(orders);
        modelBuilder.Entity<OrderItem>().HasData(orderItems);
    }

    private static void SeedWishLists(ModelBuilder modelBuilder)
    {
        var wishList = new Faker<WishList>("cz")
            .RuleFor(w => w.Id, f => f.IndexFaker + 1)
            .RuleFor(w => w.UserId, f => f.Random.Int(1, UsersCount))
            .RuleFor(w => w.Name, f => f.Lorem.Sentence(3))
            .RuleFor(w => w.CreatedAt, new DateTime(2024, 01, 01));

        var wishlists = wishList.Generate(WishlistsCount);
        modelBuilder.Entity<WishList>().HasData(wishlists);
    }

    private static void SeedWishlistItems(ModelBuilder modelBuilder)
    {
        var rng = new Random(1337);
        var wishlistItem = new Faker<WishlistItem>("cz").RuleFor(wi => wi.AddedAt, new DateTime(2024, 01, 01));
        var wishlistItems = new List<WishlistItem>();
        var idCounter = 1;

        for (var wishlistId = 1; wishlistId <= WishlistsCount; wishlistId++)
        {
            var booksForWishlist = rng.Next(1, 6);
            var usedBooks = new HashSet<int>();

            for (var i = 0; i < booksForWishlist; i++)
            {
                int bookId;
                do
                    bookId = rng.Next(1, BooksCount + 1);
                while (!usedBooks.Add(bookId));

                var item = wishlistItem.Generate();
                item.Id = idCounter++;
                item.WishlistId = wishlistId;
                item.BookId = bookId;

                wishlistItems.Add(item);
            }
        }

        modelBuilder.Entity<WishlistItem>().HasData(wishlistItems);
    }

    private static List<Cart> SeedCarts(ModelBuilder modelBuilder, List<Order> orders)
    {
        var finalUserIds = new HashSet<int>();

        foreach (var userId in orders.Select(o => o.UserId))
        {
            finalUserIds.Add(userId);
        }

        var rng = new Random(1337);
        var allUserIds = Enumerable.Range(1, UsersCount).ToList();
    
        var shuffledUsers = allUserIds.OrderBy(_ => rng.Next()).ToList();
    
        foreach (var userId in shuffledUsers)
        {
            if (finalUserIds.Count >= Math.Max(orders.Select(o => o.UserId).Distinct().Count(), CartCount))
                break;
            
            finalUserIds.Add(userId);
        }

        var carts = new List<Cart>();
        int currentId = 1;

        foreach (var userId in finalUserIds)
        {
            carts.Add(new Cart
            {
                Id = currentId++, 
                UserId = userId,  
                CreatedAt = new DateTime(2024, 01, 01)
            });
        }

        modelBuilder.Entity<Cart>().HasData(carts);
        return carts;
    }

    private static void SeedCartItems(ModelBuilder modelBuilder, List<Cart> carts)
    {
        const int maxItemsPerCart = 3;
        var faker = new Faker("cz");
        var cartItemList = new List<CartItem>();
        int nextId = 1;
        if (!carts.Any()) return;

        foreach (var cart in carts)
        {
            int itemCount = faker.Random.Int(0, maxItemsPerCart);
            var usedBooksInThisCart = new HashSet<int>();

            for (int i = 0; i < itemCount; i++)
            {
                int bookId = faker.Random.Int(1, BooksCount);

                if (usedBooksInThisCart.Add(bookId))
                {
                    cartItemList.Add(new CartItem
                    {
                        Id = nextId++,
                        CartId = cart.Id,
                        BookId = bookId,
                        Quantity = faker.Random.Int(1, 3)
                    });
                }
            }
        }

        modelBuilder.Entity<CartItem>().HasData(cartItemList);
    }
}