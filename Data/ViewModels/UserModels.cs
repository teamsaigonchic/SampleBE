using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels
{
    public class UserRegisterModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Username { get; set; }
        public string Role { get; set; }
    }

    public class UserLoginModel
    {
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class LoginSuccessViewModel
    {
        public object Token { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public DateTime DateCreated { get; set; }
        public string Role { get; set; }
    }

    public class ResetPasswordModel
    {
        public string Username { get; set; }
    }
    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ChangeInfoModel
    {
        public string Fullname { get; set; }
    }
}
