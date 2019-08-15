using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace ICoaster.Services.Token
{
    public class ValidateJwtTokenActiveAttribute : TypeFilterAttribute
    {
        public ValidateJwtTokenActiveAttribute() : base(typeof(ValidateJwtToken))
        {

        }
    }

    public class ValidateJwtToken : IAsyncAuthorizationFilter
    {
        private readonly TokenManager token;
        public ValidateJwtToken(TokenManager tokenMgmr)
        {
            token = tokenMgmr;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (await token.IsCurrentTokenActive())
            {
                return;
            }
            context.Result = new UnauthorizedResult();
        }

    }
}
