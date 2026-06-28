using BookHub;
using BookHub.Api.Middleware;
using BusinessLayer.Dto.Author;
using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.Genre;
using BusinessLayer.Dto.Publisher;
using BusinessLayer.Dto.Review;
using BusinessLayer.Dto.User;
using BusinessLayer.Dto.Wishlist;
using BookHub.Extension;
using BookHub.Middleware;
using BusinessLayer.Cache;
using BusinessLayer.Dto.Cart;
using BusinessLayer.Dto.Order;
using BusinessLayer.Facade;
using BusinessLayer.Service.Logging;
using BusinessLayer.Service;
using DataAccessLayer;
using DataAccessLayer.Repository.Review;
using DataAccessLayer.Repository.Order;
using DataAccessLayer.Repository.OrderItem;
using DataAccessLayer.Repository.Book;
using DataAccessLayer.Repository.WishList;
using DataAccessLayer.Repository.WishListItem;
using DataAccessLayer.Repository.User;
using DataAccessLayer.Repository.Author;
using DataAccessLayer.Repository.Cart;
using DataAccessLayer.Repository.CartItem;
using DataAccessLayer.Repository.Coupon;
using DataAccessLayer.Repository.Genre;
using DataAccessLayer.Repository.GiftCard;
using DataAccessLayer.Repository.Image;
using DataAccessLayer.Repository.Publisher;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
    .AddEnvironmentVariables("BOOKHUB_");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddOData(_ =>
{
    var odataBuilder = new ODataConventionModelBuilder();

    odataBuilder.EntitySet<AuthorDto>("Authors");
    odataBuilder.EntitySet<BookDto>("Books");
    odataBuilder.EntitySet<GenreDto>("Genres");
    odataBuilder.EntitySet<PublisherDto>("Publishers");
    odataBuilder.EntitySet<WishListDto>("WishLists");
    odataBuilder.EntitySet<CartDto>("Carts");
    odataBuilder.EntitySet<OrderDto>("Orders");
    odataBuilder.EntitySet<UserDto>("Users");
});

builder.Services.AddSwaggerGen(c =>
{
    // Need to add a document filter for each entity to support OData query options in Swagger UI because SwaggerBucker does not support
    // OData natively and will eat up RAM due to reflection errors and cannot be set anyother way do to it runnign at compile time and not runtime
    c.DocumentFilter<ODataQueryDocumentFilter<BookDto>>("/books", "Book");
    c.DocumentFilter<ODataQueryDocumentFilter<AuthorDto>>("/authors", "Authors");
    c.DocumentFilter<ODataQueryDocumentFilter<GenreDto>>("/genres", "Genres");
    c.DocumentFilter<ODataQueryDocumentFilter<PublisherDto>>("/publishers", "Publishers");
    c.DocumentFilter<ODataQueryDocumentFilter<WishListDto>>("/wishlists", "WishList");
    c.DocumentFilter<ODataQueryDocumentFilter<UserDto>>("/users", "Users");
    c.DocumentFilter<ODataQueryDocumentFilter<CartDto>>("/carts", "Cart");
    c.DocumentFilter<ODataQueryDocumentFilter<OrderDto>>("/orders", "Order");
    c.DocumentFilter<ODataQueryDocumentFilter<ReviewDto>>("/reviews", "Review");
    c.DocumentFilter<ODataQueryDocumentFilter<ReviewDto>>("/giftcards", "GiftCard");
    c.DocumentFilter<ODataQueryDocumentFilter<ReviewDto>>("/coupons", "Coupon");

    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BookHub API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Input: Bearer BookHub-Token-Dev",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{{
            new OpenApiSecurityScheme {Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>() } });
});

builder.Services.AddControllers(options =>
{
    options.OutputFormatters.Clear(); // disable default JSON serialization
    options.OutputFormatters.Add(new JsonOrXmlOutputFormatter());
});

builder.Services.AddConfiguredDatabase(builder.Configuration, builder.Environment);
builder.Services.AddConfiguredElasticSearch(builder.Configuration);
builder.Services.AddSingleton<IRequestLogService, RequestLogService>();
builder.Services.AddSingleton<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ICheckoutFacade, CheckoutFacade>();

builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IWishListService, WishListService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IWishListRepository, WishListRepository>();
builder.Services.AddScoped<IWishListItemRepository, WishListItemRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IPublisherRepository, PublisherRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IImageService>(sp => {
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var path = Path.Combine(env.WebRootPath, "images", "books");
    return new ImageService(path);
});
builder.Services.AddScoped<IGiftCardService, GiftCardService>();
builder.Services.AddScoped<IGiftCardRepository, GiftCardRepository>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();


string wwwRootPath = builder.Environment.WebRootPath;

builder.Services.AddScoped<IImageRepository>(_ => 
    new LocalImageRepository(wwwRootPath));

builder.Services.AddMemoryCache();

// Cache decorator registation
builder.Services.Decorate<IAuthorService, CachedAuthorService>();
builder.Services.Decorate<IPublisherService, CachedPublisherService>();
builder.Services.Decorate<IGenreService, CachedGenreService>();
builder.Services.Decorate<IBookService, CachedBookService>();

// Map the JSON section to the C# class
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));

var app = builder.Build();

app.UseStaticFiles();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<JwtTokenMiddleware>();
app.UseMiddleware<AuthenticationTokenMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();


app.UsePathBase("/api");
app.MapControllers();
app.Run();