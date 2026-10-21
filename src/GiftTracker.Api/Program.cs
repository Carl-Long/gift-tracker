using GiftTracker.Api.Extensions;
using GiftTracker.Application;
using GiftTracker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApiDocumentation(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapApiDocumentation();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
