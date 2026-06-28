using BusinessLayer.Dto.Cart;
using BusinessLayer.Dto.Order;
using BusinessLayer.Dto.Book;
using BusinessLayer.Mapping;
using BusinessLayer.Service;
using DataAccessLayer;
using DataAccessLayer.Models;
using LanguageExt.Common;

namespace BusinessLayer.Facade;

public class CheckoutFacade : ICheckoutFacade
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IBookService _bookService;
    private readonly IUnitOfWork _uow;

    public CheckoutFacade(ICartService cartService, IOrderService orderService, IBookService bookService, IUnitOfWork uow)
    {
        _cartService = cartService;
        _orderService = orderService;
        _bookService = bookService;
        _uow = uow;
    }
    
    public async Task<Result<OrderDto>> GenerateOrderFromCartAsync(int cartId, CreateOrderDto createOrderDto)
    {
        var orderResult = new Result<OrderDto>();
        
        try
        {
            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var cartResult = await _cartService.GetCart(cartId);
                var cart = cartResult.Match(c => c, _ => null!) 
                           ?? throw new Exception($"Cart {cartId} not found");

                var cartItemsResult = await _cartService.GetCartItemsByCartIdAsync(cartId);
                var cartItems = cartItemsResult.Match(items => items, _ => null!) ??
                    throw new Exception($"Cart {cartId} not found");
                if (cartItems.Count == 0)
                    throw new Exception($"Cart is empty");
                
                orderResult = await _orderService.AddOrder(createOrderDto);
                var newOrder = orderResult.Match(b => b, _ => null!) 
                               ?? throw new Exception("Order creation failed");
                var totalPrice = newOrder.TotalPrice;

                foreach (var item in cartItems)
                {
                    var bookResult = await _bookService.GetBook(item.BookId);
                    var book = bookResult.Match(b => b, _ => null!)
                               ?? throw new Exception($"Book {item.BookId} not found");

                    if (book.StockQuantity < item.Quantity)
                        throw new Exception(
                            $"Quantity {item.Quantity} of book {item.BookId} is out of stock, only {book.StockQuantity} are remaining");

                    var orderDto = (await _orderService.AddOrderItemToOrderUnsaved(new OrderItemDto
                            {BookId = item.BookId, Quantity = item.Quantity, OrderId = newOrder.Id}))
                        .IfFail(ex => throw ex);

                    var bookDto = (await _bookService.UpdateBookStockQuantityUnsaved(item.BookId, book.StockQuantity - item.Quantity))
                        .IfFail(ex => throw ex);

                    await _cartService.RemoveCartItemFromCartUnsaved(new CartItemDto
                        {CartId = item.CartId, BookId = item.BookId, Quantity = item.Quantity});
                    
                    totalPrice += item.Quantity * book.Price;
                }
                var coupon = cart.AppliedCouponId is not null
                    ? await _uow.Coupons.GetByIdAsync(cart.AppliedCouponId.Value)
                    : null;
                orderResult = await _orderService.UpdateOrderTotalPriceUnsaved(
                    newOrder.Id, 
                    Math.Max(totalPrice - (float)(coupon?.GiftCard.ReductionAmount ?? 0m), 0f)
                );
                cart.AppliedCouponId = null;
                if (coupon is not null)
                {
                    coupon.IsUsed = true;
                    coupon.OrderId = newOrder.Id;
                }
                await _cartService.RemoveCoupon(cartId);
                await _uow.SaveChangesAsync();
            });
            return orderResult;
        }
        catch (Exception exception)
        {
            return new Result<OrderDto>(exception);
        }
    }
}