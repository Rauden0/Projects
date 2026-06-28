using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Author;
using DataAccessLayer.Repository.Book;
using DataAccessLayer.Repository.Cart;
using DataAccessLayer.Repository.CartItem;
using DataAccessLayer.Repository.Coupon;
using DataAccessLayer.Repository.Genre;
using DataAccessLayer.Repository.GiftCard;
using DataAccessLayer.Repository.Image;
using DataAccessLayer.Repository.Order;
using DataAccessLayer.Repository.OrderItem;
using DataAccessLayer.Repository.Publisher;
using DataAccessLayer.Repository.Review;
using DataAccessLayer.Repository.User;
using DataAccessLayer.Repository.WishList;
using DataAccessLayer.Repository.WishListItem;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer;

public class UnitOfWork : IUnitOfWork
{
    private readonly BookHubDbContext _context;

    public IAuthorRepository Authors { get; }
    public ICartRepository Carts { get; }
    public ICartItemRepository CartItems { get; }
    public IOrderRepository Orders { get; }
    public IOrderItemRepository OrderItems { get; }
    public IBookRepository Books { get; }
    public IWishListItemRepository WishlistItems { get; }
    public IReviewRepository Reviews { get; }
    public IWishListRepository WishLists { get; }
    public IGenreRepository Genres { get; }
    public IPublisherRepository Publishers { get; }
    public IUserRepository Users { get; }
    public IImageRepository Images { get; }
    public IGiftCardRepository GiftCards { get; }
    public ICouponRepository Coupons { get; }

    public UnitOfWork
    (
        BookHubDbContext context,
        IReviewRepository reviews,
        ICartRepository carts,
        ICartItemRepository cartItems,
        IOrderRepository orders,
        IOrderItemRepository orderItems,
        IBookRepository books,
        IWishListRepository wishLists,
        IWishListItemRepository wishlistItems,
        IAuthorRepository authors,
        IGenreRepository genres,
        IPublisherRepository publishers,
        IUserRepository users,
        IImageRepository images,
        IGiftCardRepository giftCards,
        ICouponRepository coupons
    )
    {
        _context = context;
        Reviews = reviews;
        Carts = carts;
        CartItems = cartItems;
        Orders = orders;
        OrderItems = orderItems;
        Books = books;
        WishLists = wishLists;
        WishlistItems = wishlistItems;
        Authors = authors;
        Genres = genres;
        Publishers = publishers;
        Users = users;
        Images = images;
        GiftCards = giftCards;
        Coupons = coupons;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(
        Func<Task> operation,
        Func<Task<bool>>? verifySucceeded = null)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteInTransactionAsync(
            async () => await operation(),
            async () => verifySucceeded == null || await verifySucceeded()
        );
    }

}