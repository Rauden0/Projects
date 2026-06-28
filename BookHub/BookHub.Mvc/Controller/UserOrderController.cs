using BookHub.Mvc.Models.User;
using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.Cart;
using BusinessLayer.Dto.Coupon;
using BusinessLayer.Dto.Order;
using BusinessLayer.Dto.User;
using BusinessLayer.Facade;
using BusinessLayer.Mapping.Enums;
using BusinessLayer.Service;
using DataAccessLayer.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.Mvc.Controller;

public class UserOrderController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    private readonly IBookService _bookService;
    private readonly ICartService _cartService;
    private readonly ICheckoutFacade _checkoutFacade;
    private readonly ICouponService _couponService;

    public UserOrderController(IOrderService orderService, IUserService userService, IBookService bookService,
        ICartService cartService, ICheckoutFacade checkoutFacade, ICouponService couponService)
    {
        _orderService = orderService;
        _userService = userService;
        _bookService = bookService;
        _cartService = cartService;
        _checkoutFacade = checkoutFacade;
        _couponService = couponService;
    }

    private async Task FillDetailModel(int id, UserOrderDetailViewModel dvm)
    {
        var orderResult = await _orderService.GetOrder(id);
        var orderDto = orderResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading order: {err.Message}");
            return new OrderDto();
        });

        var orderItemListResult = await _orderService.GetOrderItemsByOrderIdAsync(id);
        var orderItemList = orderItemListResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading order item list: {err.Message}");
            return new List<OrderItemDto>();
        });

        var bookIds = orderItemList.Select(ci => ci.BookId).Distinct();
        var bookListResult = await _bookService.GetBooksByIdsAsync(bookIds, includeImages: false);
        var bookList = bookListResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading books.");
            return new List<BookDto>();
        });

        dvm.Id = orderDto.Id;
        dvm.TotalPrice = orderDto.TotalPrice;
        dvm.OrderState = OrderStateEnumMapper.ToCode(orderDto.OrderState);
        dvm.PaymentMethod = PaymentMethodEnumMapper.ToCode(orderDto.PaymentMethod);
        dvm.Paid = orderDto.Paid;
        dvm.OrderItems = orderItemList;
        dvm.Books = bookList;
        dvm.FirstName = orderDto.FirstName;
        dvm.LastName = orderDto.LastName;
        dvm.Email = orderDto.Email;
        dvm.PhoneNumber = orderDto.PhoneNumber;
        dvm.Street = orderDto.Street;
        dvm.City = orderDto.City;
        dvm.PostalCode = orderDto.PostalCode;
        dvm.Country = orderDto.Country;
    }

    public async Task<IActionResult> Index()
    {
        var email = User.Identity?.Name ?? "unknown";

        var userResult = await _userService.GetUserByEmailAsync(email!);
        var userDto = userResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading user {email}: {err.Message}");
            return new UserDto();
        });

        var result = await _orderService.GetOrders(null);
        var orders = result.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading user {email}: {err.Message}");
            ViewData["Error"] = err.Message;
            return new List<OrderDto>();
        });

        return View(orders.Where(o => o.UserId == userDto.Id).ToList());
    }

    public async Task<IActionResult> Details(int id)
    {
        var dvm = new UserOrderDetailViewModel();
        await FillDetailModel(id, dvm);

        if (!ModelState.IsValid)
        {
            ViewData["Error"] = "Error loading order";
        }

        return View(dvm);
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var model = new CheckoutViewModel();
        await FillCheckoutModel(model);

        if (model.Cart == null || !model.Cart.CartItemsIds.Any())
            return RedirectToAction("Index", "Cart");

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyCoupon(CheckoutViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.CouponCode))
        {
            var result = await _cartService.ApplyCoupon(model.CartId, model.CouponCode);
            result.IfFail(err => ModelState.AddModelError(nameof(model.CouponCode), err.Message));
        }
        else
        {
            ModelState.AddModelError(nameof(model.CouponCode), "Please enter a code.");
        }

        await FillCheckoutModel(model);

        return View(model);
    }

    private async Task FillCheckoutModel(CheckoutViewModel model)
    {
        var email = User.Identity?.Name ?? "unknown";
        var userResult = await _userService.GetUserByEmailAsync(email);
        var user = userResult.Match(u => u, _ => null!);

        if (user != null)
        {
            var cartResult = await _cartService.GetCartByUserId(user.Id);
            model.Cart = cartResult.Match(c => c, _ => null!);

            if (model.Cart != null)
            {
                model.CartId = model.Cart.Id;

                var itemsResult = await _cartService.GetCartItemsByCartIdAsync(model.Cart.Id);
                var items = itemsResult.Match(i => i, _ => new List<CartItemDto>());

                var bookIds = items.Select(i => i.BookId).Distinct().ToList();
                var booksResult = await _bookService.GetBooksByIdsAsync(bookIds, false);
                var books = booksResult.Match(b => b,_  => new List<BookDto>()).ToDictionary(b => b.Id);
                
                model.CartItems = items
                    .Where(i => books.ContainsKey(i.BookId))
                    .ToList();

                model.Books = model.CartItems
                    .Select(i => books[i.BookId])
                    .ToList();
            }
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await FillCheckoutModel(model);
            return View(model);
        }

        var email = User.Identity!.Name!;
        var user = (await _userService.GetUserByEmailAsync(email)).Match(u => u, _ => null!);
        var coupon = null as CouponDto;
        if (model.CouponCode != null)
        {
            var couponResult = await _couponService.GetCouponByCodeAsync(model.CouponCode);
            coupon = couponResult.Match(c => c, _ => null!);
        }

        var createOrderDto = new CreateOrderDto
        {
            UserId = user.Id,
            PaymentMethod = (PaymentMethodEnum)model.PaymentMethod!,
            OrderState = OrderStateEnum.Preparing,
            Paid = false,
            CouponId = coupon?.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Street = model.Street,
            City = model.City,
            PostalCode = model.PostalCode,
            Country = model.Country
        };

        var result = await _checkoutFacade.GenerateOrderFromCartAsync(model.CartId, createOrderDto);

        return result.Match<IActionResult>(
            order => RedirectToAction("Details", new { id = order.Id }),
            err =>
            {
                ModelState.AddModelError(string.Empty, err.Message);
                return View(model);
            }
        );
    }
}