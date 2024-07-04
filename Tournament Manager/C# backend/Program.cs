using api.Repository.Interfaces;
using api.Repository.TeamRepository;
using api.Repository.UserRepository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TournamentBackEnd.Repository.NodeRepository;
using TournamentBackEnd.Repository.TeamRepository;
using TournamentBackEnd.Repository.TournamentRepository;
using TournamentBackEnd.TournamentDatabase;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddControllers().AddNewtonsoftJson(
    options => { options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; }
);
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TournamentDbContext>(
    options => { options.UseSqlServer(builder.Configuration.GetConnectionString("LaptopConnection")); }
);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITournamentRepository, TournamentRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<INodeRepository, NodeRepository>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.MapControllers();
app.Run("http://localhost:5194");