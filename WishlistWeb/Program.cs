using Microsoft.EntityFrameworkCore;
using Models;
using WishlistWeb;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(cfg => {
    cfg.LicenseKey = builder.Configuration.GetValue<string>("AutomapperKey");
}, typeof(MappingProfile));
builder.Services.AddDbContext<WishlistDbContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
