namespace Tests.UnitTests.Servicies;

using BusinessLayer.Dto.Cart;
using BusinessLayer.Service;
using DataAccessLayer;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Cart;
using DataAccessLayer.Repository.CartItem;
using DataAccessLayer.Repository.Book;
using DataAccessLayer.Repository.Coupon;
using Moq;


[TestFixture]
public class CartServiceTests
{
    private Mock<IUnitOfWork> _uowMock = null!;
    private Mock<ICartRepository> _cartRepoMock = null!;
    private Mock<ICartItemRepository> _cartItemRepoMock = null!;
    private Mock<IBookRepository> _bookRepoMock = null!;
    private Mock<ICouponRepository> _couponRepoMock = null!;
    private CartService _cartService = null!;

    [SetUp]
    public void SetUp()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _cartRepoMock = new Mock<ICartRepository>();
        _cartItemRepoMock = new Mock<ICartItemRepository>();
        _bookRepoMock = new Mock<IBookRepository>();
        _couponRepoMock = new Mock<ICouponRepository>();

        _uowMock.Setup(u => u.Carts).Returns(_cartRepoMock.Object);
        _uowMock.Setup(u => u.CartItems).Returns(_cartItemRepoMock.Object);
        _uowMock.Setup(u => u.Books).Returns(_bookRepoMock.Object);
        _uowMock.Setup(u => u.Coupons).Returns(_couponRepoMock.Object);

        _uowMock.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), null))
                .Returns<Func<Task>, Func<Task<bool>>?>(async (op, _) => { await op();});

        _cartService = new CartService(_uowMock.Object);
    }

    #region Cart Operations
    [Test]
    public async Task GetCart_ShouldReturnCart_WhenExists()
    {
        var cart = new Cart { Id = 1, UserId = 10 };
        _cartRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cart);

        var result = await _cartService.GetCart(1);

        Assert.That(result.IsSuccess, Is.True);
        _cartRepoMock.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Test]
    public async Task AddCart_ShouldSaveAndReturnDto()
    {
        var dto = new CreateCartDto { UserId = 5 };
        
        var result = await _cartService.AddCart(dto);

        Assert.That(result.IsSuccess, Is.True);
        _cartRepoMock.Verify(r => r.Add(It.IsAny<Cart>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    #endregion

    #region Cart Items
    [Test]
    public async Task AddCartItemToCart_ShouldIncreaseQuantity_WhenItemAlreadyInCart()
    {
        var cartItemDto = new CartItemDto { CartId = 1, BookId = 1, Quantity = 2 };
        var cart = new Cart { Id = 1 };
        var book = new Book { Id = 1 };
        var existingItem = new CartItem { CartId = 1, BookId = 1, Quantity = 1 };

        _cartRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cart);
        _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
        _cartItemRepoMock.Setup(r => r.GetCartItemByCartIdAndBookIdAsync(1, 1)).ReturnsAsync(existingItem);

        await _cartService.AddCartItemToCart(cartItemDto);

        Assert.That(existingItem.Quantity, Is.EqualTo(3));
        _cartItemRepoMock.Verify(r => r.Update(existingItem), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    #endregion

    #region Coupons
    [Test]
    public async Task ApplyCoupon_ShouldUpdateCart_WhenCouponIsValid()
    {
        var cartId = 1;
        var code = "DISCOUNT2025";
        var cart = new Cart { Id = cartId };
        var giftCard = new GiftCard { ValidFrom = DateTime.UtcNow.AddDays(-1), ValidTo = DateTime.UtcNow.AddDays(1) };
        var coupon = new Coupon { Id = 50, Code = code, IsUsed = false, GiftCard = giftCard };
        
        var couponsQuery = new List<Coupon> { coupon }.AsQueryable();

        _cartRepoMock.Setup(r => r.GetByIdAsync(cartId)).ReturnsAsync(cart);
        _couponRepoMock.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Coupon>(couponsQuery.Expression));

        var result = await _cartService.ApplyCoupon(cartId, code);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(cart.AppliedCouponId, Is.EqualTo(50));
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ApplyCoupon_ShouldReturnError_WhenCouponIsExpired()
    {
        var cartId = 1;
        var code = "EXPIRED";
        var cart = new Cart { Id = cartId };
        var giftCard = new GiftCard { ValidFrom = DateTime.UtcNow.AddDays(-10), ValidTo = DateTime.UtcNow.AddDays(-5) };
        var coupon = new Coupon { Code = code, IsUsed = false, GiftCard = giftCard };
        
        var couponsQuery = new List<Coupon> { coupon }.AsQueryable();

        _cartRepoMock.Setup(r => r.GetByIdAsync(cartId)).ReturnsAsync(cart);
        _couponRepoMock.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Coupon>(couponsQuery.Expression));

        var result = await _cartService.ApplyCoupon(cartId, code);

        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("not valid at this time"));
    }
    #endregion
}