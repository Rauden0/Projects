using BusinessLayer.Dto.Order;
using BusinessLayer.Extension;
using BusinessLayer.Mapping;
using DataAccessLayer;
using DataAccessLayer.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;

    public OrderService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<OrderDto>> GetOrder(int id)
    {
        var order = await _uow.Orders.GetByIdAsync(id);
        return order is not null
            ? OrderMapper.ToDto(order)
            : new Result<OrderDto>(new Exception($"Order {id} not found"));
    }

    public async Task<Result<List<OrderDto>>> GetOrders(ODataQueryOptions<OrderDto>? options)
    {
        return await OrderMapper.ProjectToDto(_uow.Orders.Query()).ApplyIfNotNull(options).ToListAsync();
    }

    public async Task<Result<OrderDto>> AddOrder(CreateOrderDto createOrderDto)
    {
        var order = new Order
        {
            UserId = createOrderDto.UserId,
            PaymentMethod = createOrderDto.PaymentMethod,
            OrderState = createOrderDto.OrderState,
            Paid = createOrderDto.Paid,
            TotalPrice = 0.0f,
            FirstName = createOrderDto.FirstName,
            LastName = createOrderDto.LastName,
            Email = createOrderDto.Email,
            PhoneNumber = createOrderDto.PhoneNumber,
            Street = createOrderDto.Street,
            City = createOrderDto.City,
            PostalCode = createOrderDto.PostalCode,
            Country = createOrderDto.Country,
            CouponId = createOrderDto.CouponId
        };
        
        _uow.Orders.Add(order);
        await _uow.SaveChangesAsync();
        return OrderMapper.ToDto(order);
    }
    
    public async Task<Result<OrderDto>> UpdateOrder(int id, UpdateOrderDto updateOrderDto)
    {
        var order = await _uow.Orders.GetByIdAsync(id);
        if (order is null)
        {
            return new Result<OrderDto>(new Exception($"Order {id} not found"));
        }
        
        order.PaymentMethod = updateOrderDto.PaymentMethod;
        order.OrderState = updateOrderDto.OrderState;
        order.Paid = updateOrderDto.Paid;
        
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();
        return OrderMapper.ToDto(order);
    }

    public async Task<Result<OrderDto>> UpdateOrderTotalPriceUnsaved(int id, float totalPrice)
    {
        var order = await _uow.Orders.GetByIdAsync(id);
        if (order is null)
        {
            return new Result<OrderDto>(new Exception($"Order {id} not found"));
        }
        order.TotalPrice = totalPrice;
        _uow.Orders.Update(order);
        return OrderMapper.ToDto(order);
    }
    
    public async Task DeleteOrder(int id)
    {
        await _uow.ExecuteInTransactionAsync(async () => 
        {
            var order = await _uow.Orders.GetByIdAsync(id);
            if (order != null)
                _uow.Orders.Remove(order);
            await _uow.SaveChangesAsync();
        });
    }

    public async Task<Result<OrderDto>> AddOrderItemToOrder(OrderItemDto orderItemDto)
    {
        var order = await _uow.Orders.GetByIdAsync(orderItemDto.OrderId);
        if (order is null)
            return new Result<OrderDto>(new Exception($"Order {orderItemDto.OrderId} not found"));
        
        var newBook = await _uow.Books.GetByIdAsync(orderItemDto.BookId);
        if (newBook is null)
            return new Result<OrderDto>(new Exception($"Book {orderItemDto.BookId} not found"));
        
        var orderItem = new OrderItem
        {
            Price = newBook.Price,
            OrderId = orderItemDto.OrderId, 
            BookId = orderItemDto.BookId, 
            Quantity = orderItemDto.Quantity
        }; 
        _uow.OrderItems.Add(orderItem);
        await _uow.SaveChangesAsync();
        return OrderMapper.ToDto(order);
    }

    public async Task<Result<OrderDto>> AddOrderItemToOrderUnsaved(OrderItemDto orderItemDto)
    {
        var order = await _uow.Orders.GetByIdAsync(orderItemDto.OrderId);
        if (order is null)
            return new Result<OrderDto>(new Exception($"Order {orderItemDto.OrderId} not found"));
        
        var book = await _uow.Books.GetByIdAsync(orderItemDto.BookId);
        if (book is null)
            return new Result<OrderDto>(new Exception($"Book {orderItemDto.BookId} not found"));
        
        var orderItem = new OrderItem
        {
            Price = book.Price,
            OrderId = orderItemDto.OrderId, 
            BookId = orderItemDto.BookId, 
            Quantity = orderItemDto.Quantity
        }; 
        _uow.OrderItems.Add(orderItem);
        return OrderMapper.ToDto(order);
    }

    public async Task RemoveOrderItemFromOrder(OrderItemDto orderItemDto)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
             var orderItem = await _uow.OrderItems.GetOrderItemByOrderIdAndBookIdAsync(orderItemDto.OrderId, orderItemDto.BookId);
             var order = await _uow.Orders.GetByIdAsync(orderItemDto.OrderId);
             if (orderItem != null && order != null)
             {
                 order.TotalPrice -= orderItem.Price * orderItemDto.Quantity;
                 _uow.Orders.Update(order);
                 _uow.OrderItems.Remove(orderItem);
             }
             await _uow.SaveChangesAsync();   
        });
    }

    public async Task<Result<List<OrderItemDto>>> GetOrderItemsByOrderIdAsync(int orderId)
    {
        var order = await _uow.Orders.GetByIdAsync(orderId);
        if (order is null)
            return new Result<List<OrderItemDto>>(new Exception($"Order {orderId} not found"));
        var orderItems = await _uow.OrderItems.GetOrderItemsByOrderIdAsync(orderId);
        return OrderItemMapper.ProjectToDto(orderItems.AsQueryable()).ToList();
    }
}