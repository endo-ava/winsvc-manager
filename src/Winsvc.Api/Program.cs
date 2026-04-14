using Winsvc.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.AddWinsvcApi();

var app = builder.Build();
app.MapWinsvcApiEndpoints();

app.Run();
