using WikiAvesScrapper.Services.Base;
using WikiAvesScrapper.Services.Family;
using WikiAvesScrapper.Services.Sound;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<ScrapperBase, ScrapperBase>();

builder.Services.AddScoped<IIndexService, IndexService>();
builder.Services.AddTransient<ISoundService, SoundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
