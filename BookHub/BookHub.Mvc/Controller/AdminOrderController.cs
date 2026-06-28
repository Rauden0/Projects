using BookHub.Mvc.Models.Admin;
using BusinessLayer.Dto.Order;
using BusinessLayer.Mapping.Enums;
using BusinessLayer.Service;
using LanguageExt.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookHub.Mvc.Controller;

[Authorize(Roles = "Admin")]
public class AdminOrderController : Microsoft.AspNetCore.Mvc.Controller
{
    
    private readonly IOrderService _orderService;

    public AdminOrderController(
        IOrderService orderService)
    {
        _orderService = orderService;
    }

    private async Task FillSelectListsAsync(AdminOrderFormViewModel vm)
    {
        var orderItemsResult = await _orderService.GetOrderItemsByOrderIdAsync(vm.Id);
        var orderItems = orderItemsResult.Match(
            succ => succ,
            err =>
            {
                ModelState.AddModelError(string.Empty, $"Error loading order items: {err.Message}");
                return new List<OrderItemDto>();
            });

        vm.OrderItems = orderItems.Select(oi => new SelectListItem
        {
            Value = oi.BookId.ToString(),
            Text = oi.Quantity.ToString()
        });

        var selectedPaymentMethod = new List<string>() {"CASH", "CARD", "BANK"};
        vm.SelectedPaymentMethod = selectedPaymentMethod.Select(pm => new SelectListItem
        {
            Value = pm,
            Text = pm
        });
        
        var selectedOrderStates = new List<string>() {"PREPARING", "SENDING", "COMPLETED"};
        vm.SelectedOrderState = selectedOrderStates.Select(os => new SelectListItem
        {
            Value = os,
            Text = os
        });
    }
    
    public async Task<IActionResult> Index()
    {
        Result<List<OrderDto>> result = await _orderService.GetOrders(null);

        if (result.IsFaulted)
        {
            var error = result.Match(_ => null, ex => ex.Message);
            ViewData["Error"] = error;
            return View(Enumerable.Empty<OrderDto>());
        }

        var orders = result.Match(list => list, _ => new List<OrderDto>());
        return View(orders);
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var result = await _orderService.GetOrder(id);
        if (result.IsFaulted)
        {
            return NotFound();
        }

        var orderDto = result.Match(b => b, _ => null);
        var vm = new AdminOrderFormViewModel
        {
            Id = orderDto.Id,
            PaymentMethod = PaymentMethodEnumMapper.ToCode(orderDto.PaymentMethod),
            OrderState = OrderStateEnumMapper.ToCode(orderDto.OrderState),
            TotalPrice = orderDto.TotalPrice,
            SelectedOrderItemIds = orderDto.OrderItemsIds,
            Paid = orderDto.Paid
        };

        await FillSelectListsAsync(vm);
        return View(vm);
    }
    
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _orderService.GetOrder(id);
        if (result.IsFaulted)
        {
            return NotFound();
        }

        var orderDto = result.Match(b => b, _ => null)!;

        var vm = new AdminOrderFormViewModel
        {
            Id = orderDto.Id,
            PaymentMethod = PaymentMethodEnumMapper.ToCode(orderDto.PaymentMethod),
            OrderState = OrderStateEnumMapper.ToCode(orderDto.OrderState),
            TotalPrice = orderDto.TotalPrice,
            SelectedOrderItemIds = orderDto.OrderItemsIds,
            Paid = orderDto.Paid
        };

        await FillSelectListsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminOrderFormViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await FillSelectListsAsync(vm);
            return View(vm);
        }

        var request = new UpdateOrderDto
        {
            PaymentMethod = PaymentMethodEnumMapper.FromCode(vm.PaymentMethod),
            OrderState = OrderStateEnumMapper.FromCode(vm.OrderState),
            Paid = vm.Paid
        };

        var result = await _orderService.UpdateOrder(id, request);

        var error = result.Match<string?>(
            _ => null,
            ex => ex.Message);

        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            await FillSelectListsAsync(vm);
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }
}