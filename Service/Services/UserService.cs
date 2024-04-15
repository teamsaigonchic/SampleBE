using Data.AppException;
using Data.DataAccessLayer;
using Data.Entities;
using Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Service.Services
{
    public interface IUserService
    {
        Task<string> Register(UserRegisterModel user, string role);
        Task<LoginSuccessViewModel> Login(UserLoginModel model);
        Task<string> ReNewPassword(ResetPasswordModel model);
        string RemoveUser(string username);
        Task<string> ChangePassword(ChangePasswordModel model, string userId);
        string ChangeInformation(ChangeInfoModel model, string userId);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public UserService(AppDbContext dbContext, SignInManager<User> signInManager, UserManager<User> userManager, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        public string ChangeInformation(ChangeInfoModel model, string userId)
        {
            var user = _dbContext.Users.FirstOrDefault(f => f.Id == userId);

            if (user == null)
            {
                throw new AppException("Invalid user");
            }

            user.Fullname = model.Fullname;
            _dbContext.Update(user);
            _dbContext.SaveChanges();

            return user.Id;
        }

        public async Task<string> ChangePassword(ChangePasswordModel model, string userId)
        {
            var appUser = _userManager.Users.SingleOrDefault(u => u.Id == userId);
            var check = await _userManager.ChangePasswordAsync(appUser, model.OldPassword, model.NewPassword);

            if (!check.Succeeded)
            {
                throw new AppException("Cập nhật mật khẩu thất bại");
            }
            return appUser.Id;
        }

        public async Task<string> Register(UserRegisterModel model, string role)
        {
            var exsitedUser = _dbContext.Users.FirstOrDefault(u => u.UserName == model.Username);

            if (exsitedUser != null && exsitedUser.IsDeleted == false)
            {
                throw new AppException("Tên đăng nhập đã tồn tại!");
            }

            if (exsitedUser != null && exsitedUser.IsDeleted == true)
            {
                exsitedUser.IsDeleted = false;
                exsitedUser.Fullname = model.FullName;
                exsitedUser.DateUpdated = DateTime.UtcNow.AddHours(7);

                var appUser = _userManager.Users.SingleOrDefault(r => r.UserName == model.Username);
                await _userManager.RemovePasswordAsync(appUser);
                await _userManager.AddPasswordAsync(appUser, model.Username);

                var roles = await _userManager.GetRolesAsync(appUser);

                bool added = false;
                foreach (var item in roles)
                {
                    if (item != model.Role)
                    {
                        await _userManager.RemoveFromRoleAsync(appUser, item);
                    }
                    else
                    {
                        added = true;
                    }
                }
                if (!added)
                {
                    await _userManager.AddToRoleAsync(appUser, role);
                }
                _dbContext.Update(exsitedUser);
                _dbContext.SaveChanges();
            }
            else
            {
                var identUser = new User
                {
                    Fullname = model.FullName,
                    NormalizedUserName = model.Username,
                    UserName = model.Username
                };

                var createRs = await _userManager.CreateAsync(identUser, model.Username);

                if (createRs.Succeeded)
                {
                    await _signInManager.SignInAsync(identUser, false);

                    await _userManager.AddToRoleAsync(identUser, role);

                    _dbContext.SaveChanges();
                }
                else
                {
                    throw new Exception("Tạo tài khoản thất bại");
                }
            }
            return model.Username;
        }
        public async Task<LoginSuccessViewModel> Login(UserLoginModel model)
        {
            var signInResult = await _signInManager.PasswordSignInAsync(model.Phone, model.Password, false, false);

            if (signInResult.Succeeded)
            {
                var appUser = _userManager.Users.SingleOrDefault(r => r.UserName == model.Phone && !r.IsDeleted);
                if (appUser == null) throw new AppException("Tài khoản bị khóa.");
                var roles = await _userManager.GetRolesAsync(appUser);

                var fullname = _dbContext.Users.First(u => u.UserName == model.Phone).Fullname;
                object token = GenerateJwtToken(appUser, roles[0]);

                var successViewModel = new LoginSuccessViewModel()
                {
                    Token = token,
                    FullName = fullname,
                    Role = roles[0]
                };

                return successViewModel;
            }
            else
            {
                throw new AppException("Thông tin đăng nhập không hợp lệ.");
            }
        }
        private object GenerateJwtToken(IdentityUser user, string role)
        {
            var claims = new List<Claim>
            {
                //new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, role),
                //new Claim(ClaimTypes.GivenName, fullname)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuerOptions:Issuer"],
                _configuration["JwtIssuerOptions:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<string> ReNewPassword(ResetPasswordModel model)
        {
            var appUser = _userManager.Users.SingleOrDefault(r => r.UserName == model.Username);
            await _userManager.RemovePasswordAsync(appUser);
            await _userManager.AddPasswordAsync(appUser, model.Username);

            return model.Username;
        }
        public string RemoveUser(string username)
        {
            var user = _dbContext.Users.FirstOrDefault(f => f.UserName == username);
            if (user == null) throw new AppException("Người dùng không tồn tại");

            user.IsDeleted = true;
            user.DateUpdated = DateTime.UtcNow.AddHours(7);

            _dbContext.Update(user);
            _dbContext.SaveChanges();

            return username;
        }
    }
}
