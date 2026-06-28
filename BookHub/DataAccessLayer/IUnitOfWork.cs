using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using DataAccessLayer.Repository.Author;
using DataAccessLayer.Repository.Book;
using DataAccessLayer.Repository.Genre;
using DataAccessLayer.Repository.Cart;
using DataAccessLayer.Repository.CartItem;
using DataAccessLayer.Repository.Coupon;
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

public interface IUnitOfWork
{
    public ICartRepository Carts { get; }
    public ICartItemRepository CartItems { get; }
    public IOrderRepository Orders { get; }
    public IOrderItemRepository OrderItems { get; }
    public IBookRepository Books { get; }
    public IWishListItemRepository WishlistItems { get; }
    public IReviewRepository Reviews { get; }
    public IWishListRepository WishLists { get; }
    public IAuthorRepository Authors { get; }
    public IGenreRepository Genres { get; }
    public IPublisherRepository Publishers { get; }
    public IUserRepository Users { get; }
    public IImageRepository Images { get; }
    public IGiftCardRepository GiftCards { get; }
    public ICouponRepository Coupons { get; }
    

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task ExecuteInTransactionAsync(
        Func<Task> operation,
        Func<Task<bool>>? verifySucceeded = null);
}
