using ICoaster.Services.Token;
using Microsoft.AspNetCore.Mvc;

namespace ICoaster.Controllers
{
    [Route("user/auth")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly JwtManager _jwt;
        public IdentityController(JwtManager jwt)
        {
            _jwt = jwt;
        }

        [HttpGet("login")]
        public JsonResult Login()
        {
            return new JsonResult(_jwt.Create("ICoaster"));
        }
    }
}
