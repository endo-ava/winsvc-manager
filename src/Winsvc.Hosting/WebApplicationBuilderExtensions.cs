namespace Winsvc.Hosting;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddWinsvcApi(this WebApplicationBuilder builder)
    {
        var apiUrls = builder.Configuration["Winsvc:Api:Urls"];
        if (!string.IsNullOrWhiteSpace(apiUrls))
        {
            builder.WebHost.UseUrls(apiUrls);
        }

        builder.Services.AddWinsvcServices();
        return builder;
    }
}
