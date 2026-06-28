using BookHub.Mvc;
using BookHub.Mvc.Data;
using BookHub.Mvc.Middleware;
using BookHub.Mvc.Models;
using BusinessLayer.Cache;
using BusinessLayer.Facade;
using BusinessLayer.Service;
using BusinessLayer.Service.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using DataAccessLayer.Data;          
using DataAccessLayer;
using DataAccessLayer.Repository.Author;
using DataAccessLayer.Repository.Book;
using DataAccessLayer.Repository.Cart;
using DataAccessLayer.Repository.CartItem;
using DataAccessLayer.Repository.GiftCard;
using DataAccessLayer.Repository.Coupon;
using DataAccessLayer.Repository.Genre;
using DataAccessLayer.Repository.Image;
using DataAccessLayer.Repository.Publisher;
using DataAccessLayer.Repository.Order;
using DataAccessLayer.Repository.OrderItem;
using DataAccessLayer.Repository.Review;
using DataAccessLayer.Repository.User;
using DataAccessLayer.Repository.WishList;
using DataAccessLayer.Repository.WishListItem;

var builder = WebApplication.CreateBuilder(args);

// ---------- DATABASES ----------
builder.Services.AddConfiguredDatabase(builder.Configuration, builder.Environment);

builder.Services.AddConfiguredElasticSearch(builder.Configuration);

// ---------- IDENTITY ----------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ---------- REPOSITORIES ----------
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<IGiftCardRepository, GiftCardRepository>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IPublisherRepository, PublisherRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();            
builder.Services.AddScoped<IUserRepository, UserRepository>();               
builder.Services.AddScoped<IWishListRepository, WishListRepository>();       
builder.Services.AddScoped<IWishListItemRepository, WishListItemRepository>(); 
builder.Services.AddScoped<IGlobalSearchService, GlobalSearchService>();


// ---------- UNIT OF WORK ----------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ---------- BUSINESS LAYER SERVICES ----------
builder.Services.AddScoped<IImageService>(sp => {
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var path = Path.Combine(env.WebRootPath, "images", "books");
    return new ImageService(path);
});
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IGiftCardService, GiftCardService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IWishListService, WishListService>();
builder.Services.AddSingleton<IRequestLogService, RequestLogService>();
builder.Services.AddSingleton<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ICheckoutFacade, CheckoutFacade>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IGiftCardService, GiftCardService>();


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
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CacheSettings>>().Value);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

// ---------- APP PIPELINE ----------
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------- Database ensure ----------
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookHubDbContext>();
    context.Database.Migrate();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
