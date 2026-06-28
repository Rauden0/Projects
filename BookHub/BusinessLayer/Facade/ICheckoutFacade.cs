using BusinessLayer.Dto.Order;
using LanguageExt.Common;

namespace BusinessLayer.Facade;

public interface ICheckoutFacade
{
    Task<Result<OrderDto>> GenerateOrderFromCartAsync(int cartId, CreateOrderDto createOrderDto);
}