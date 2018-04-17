using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PtcApi.Model;

namespace PtcApi.Controllers
{
    [Route("api/[controller]")]
    public class SecurityController : BaseApiController
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] AppUser user)
        {
            IActionResult ret = null;
            var auth = new AppUserAuth();
            var mgr = new SecurityManager();

            auth = mgr.ValidateUser(user);
            if (auth.IsAuthenticated)
                ret = StatusCode(StatusCodes.Status200OK, auth);
            else
                ret = StatusCode(StatusCodes.Status404NotFound, "Invalid User name/Password");

            return ret;
        }
    }
}