using ASP_ITStep.Data;
using ASP_ITStep.Data.Entities;
using ASP_ITStep.Models.User;
using ASP_ITStep.Services.Kdf;
using ASP_ITStep.Services.Random;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ASP_ITStep.Controllers
{
    public class UserController(IRandomService randomService, IKdfService kdfService, DataContext context, ILogger<UserController> logger) : Controller
    {
        private readonly IRandomService _randomService = randomService;
        private readonly IKdfService _kdfService = kdfService;
        private readonly DataContext _dataContext = context;
        private readonly ILogger<UserController> _logger = logger;

        public ViewResult SignUp()
        {
            UserSignupPageModel model = new();
            if (HttpContext.Session.Keys.Contains("UserSignupFormModel"))
            {
                model.FormModel = JsonSerializer.Deserialize<UserSignupFormModel>(
                    HttpContext.Session.GetString("UserSignupFormModel")!
                );
                model.FormErrors = ProcessSignupData(model.FormModel!);
                HttpContext.Session.Remove("UserSignupFormModel");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<RedirectToActionResult> Register(UserSignupFormModel model)
        {
            HttpContext.Session.SetString("UserSignupFormModel", JsonSerializer.Serialize(model));
            return RedirectToAction(nameof(SignUp));
        }

        [HttpPost]
        public JsonResult SignIn([FromBody] SignInModel model)
        {
            Dictionary<String, String> errors = [];

            if (String.IsNullOrEmpty(model.Login))
            {
                errors["login"] = "Логін не може бути порожнім";
            }

            if (String.IsNullOrEmpty(model.Password))
            {
                errors["password"] = "Пароль не може бути порожнім";
            }

            if (errors.Count > 0)
            {
                return Json(new { success = false, errors = errors });
            }
            return Json(new { success = true });
        }

        private Dictionary<String, String> ProcessSignupData(UserSignupFormModel model)
        {
            Dictionary<String, String> errors = [];

            if (String.IsNullOrEmpty(model.UserName))
            {
                errors[nameof(model.UserName)] = "Ім'я не може бути порожнім";
            }

            if (String.IsNullOrEmpty(model.UserEmail))
            {
                errors[nameof(model.UserEmail)] = "E-mail не може бути порожнім";
            }
            else
            {
                if (!Regex.IsMatch(model.UserEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    errors[nameof(model.UserEmail)] = "Неправильний формат E-mail";
                }
                else
                {
                    if (_dataContext.Users.Any(u => u.Email == model.UserEmail))
                    {
                        errors[nameof(model.UserEmail)] = "E-mail вже використовується";
                    }
                }
            }

            if (String.IsNullOrEmpty(model.UserLogin))
            {
                errors[nameof(model.UserLogin)] = "Логін не може бути порожнім";
            }
            else
            {
                if (model.UserLogin.Length < 3)
                {
                    errors[nameof(model.UserLogin)] = "Логін повинен містити мінімум 3 символи";
                }
                else if (model.UserLogin.Contains(":"))
                {
                    errors[nameof(model.UserLogin)] = "Логін не може містити символ ':'";
                }
                else
                {
                    if (_dataContext.UserAccesses.Any(ua => ua.Login == model.UserLogin))
                    {
                        errors[nameof(model.UserLogin)] = "Логін у вжитку";
                    }
                }
            }

            if (String.IsNullOrEmpty(model.UserPassword))
            {
                errors[nameof(model.UserPassword)] = "Пароль не може бути порожнім";
            }
            else
            {
                if (!IsPasswordStrong(model.UserPassword))
                {
                    errors[nameof(model.UserPassword)] = "Пароль повинен містити мінімум 8 символів, включаючи великі та малі літери, цифри та спеціальні символи";
                }
            }

            if (String.IsNullOrEmpty(model.UserRepeat))
            {
                errors[nameof(model.UserRepeat)] = "Повтор пароля не може бути порожнім";
            }
            else if (model.UserPassword != model.UserRepeat)
            {
                errors[nameof(model.UserRepeat)] = "Повтор пароля не збігається з паролем";
            }

            if (model.Birthdate == null)
            {
                errors[nameof(model.Birthdate)] = "Дата народження обов'язкова";
            }
            else
            {
                var age = DateTime.Now.Year - model.Birthdate.Value.Year;
                if (age < 13 || age > 120)
                {
                    errors[nameof(model.Birthdate)] = "Недійсна дата народження";
                }
            }

            if (!model.Agree)
            {
                errors[nameof(model.Agree)] = "Необхідно погодитись з умовами сайту";
            }

            if (errors.Count == 0)
            {
                Guid userId = Guid.NewGuid();

                UserData user = new()
                {
                    Id = userId,
                    Name = model.UserName,
                    Email = model.UserEmail,
                    Birthdate = model.Birthdate,
                    RegisteredAt = DateTime.Now,
                };
                String salt = _randomService.Otp(12);
                UserAccess userAccess = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Login = model.UserLogin,
                    Salt = salt,
                    Dk = _kdfService.Dk(model.UserPassword, salt),
                    RoleId = "SelfRegistered"
                };
                _dataContext.Database.BeginTransaction();
                _dataContext.Users.Add(user);
                _dataContext.UserAccesses.Add(userAccess);
                _dataContext.SaveChanges();
                try
                {
                    _dataContext.SaveChanges();
                    _dataContext.Database.CommitTransaction();
                }
                catch (Exception ex)
                {
                    _logger.LogError("ProcessSignupData: {ex}", ex.Message);
                    _dataContext.Database.RollbackTransaction();
                    errors["500"] = "Проблема зі збереженням. Спробуйте пізніше";
                }
            }

            return errors;
        }

        private bool IsPasswordStrong(string password)
        {
            if (password.Length < 8)
                return false;
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;
            if (!Regex.IsMatch(password, @"[0-9]"))
                return false;
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                return false;

            return true;
        }
    }

    public class SignInModel
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}