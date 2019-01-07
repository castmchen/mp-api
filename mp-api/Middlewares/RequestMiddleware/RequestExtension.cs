

namespace mp_api.Middlewares
{

    #region using

    using Microsoft.AspNetCore.Builder;

    #endregion


    public static class RequestExtension
    {
        public static IApplicationBuilder UseRequestMiddleware(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<RequestMiddleware>();
        }
    }
}
