using Data.Utils;
using Data.ViewModels;
using SampleBE.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services;

namespace SampleBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService _userService) : ControllerBase
    {
        [Authorize(AuthenticationSchemes = "Bearer", Roles = SystemRoles.ADMIN)]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterModel model)
        {
            return Ok(await _userService.Register(model, model.Role));
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginModel model)
        {
            return Ok(await _userService.Login(model));
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = SystemRoles.ADMIN)]
        [HttpPut("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            return Ok(await _userService.ReNewPassword(model));
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("Password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            return Ok(await _userService.ChangePassword(model, User.GetId()));
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("Info")]
        public IActionResult ChangeInfo([FromBody] ChangeInfoModel model)
        {
            return Ok(_userService.ChangeInformation(model, User.GetId()));
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = SystemRoles.ADMIN)]
        [HttpDelete("{username}")]
        public IActionResult RemoveUser(string username)
        {
            return Ok(_userService.RemoveUser(username));
        }
    }

}
