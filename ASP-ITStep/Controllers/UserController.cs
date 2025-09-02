using ASP_ITStep.Data;
using ASP_ITStep.Data.Entities;
using ASP_ITStep.Models.User;
using ASP_ITStep.Services.Email;
using ASP_ITStep.Services.Jwt;
using ASP_ITStep.Services.Kdf;
using ASP_ITStep.Services.Random;
using ASP_ITStep.Services.Time;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ASP_ITStep.Controllers
{
    public class UserController(IRandomService randomService,
        IKdfService kdfService,
        DataContext context,
        ILogger<UserController> logger,
        ITimeService timeService,
        IEmailService emailService,
        IJwtService jwtService,
        DataAccessor dataAccessor) : Controller
    {
        private readonly IRandomService _randomService = randomService;
        private readonly IKdfService _kdfService = kdfService;
        private readonly DataContext _dataContext = context;
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly ILogger<UserController> _logger = logger;
        private readonly ITimeService _timeService = timeService;
        private readonly IEmailService _emailService = emailService;
        private readonly IJwtService _jwtService = jwtService;

        const String authSessionKey = "userAccess";

        [HttpPost]
        public JsonResult Email()
        {
            if(HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                //try
                //{
                //    _emailService.Send("azure.spd111.od.0@ukr.net",
                //        "ASP - P26",
                //        "console.log(coffee break) ");
                //}
                //catch (Exception ex)
                //{
                //    return Json(new
                //    {
                //        Status = 500,
                //        Data = ex.Message
                //    });
                //}

                return Json(new
                {
                    Status = 200,
                    Data = "ok"
                });
            }
            else
            {
                return Json(new
                {
                    Status = 401,
                    Data = "UnAutothorized"
                });
            }
        }
        private UserAccess Authenticate()
        {
            // Authorization: Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            String authHeader = Request.Headers.Authorization.ToString();  // Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            if (String.IsNullOrEmpty(authHeader))
            {
                throw new Exception("Missing 'Authorization' header");
            }
            String authScheme = "Basic ";
            if (!authHeader.StartsWith(authScheme))
            {
                throw new Exception($"Authorization scheme error: '{authScheme}' only");
            }
            String credentials = authHeader[authScheme.Length..];  // QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            String decoded;
            try
            {
                decoded = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(credentials));
            }
            catch (Exception ex)
            {
                _logger.LogError("SignIn: {ex}", ex.Message);
                throw new Exception($"Authorization credentials decode error");
            }
            String[] parts = decoded.Split(':', 2);
            if (parts.Length != 2)
            {
                throw new Exception($"Authorization credentials decompose error");
            }
            String login = parts[0];
            String password = parts[1];
            var userAccess = _dataAccessor.GetUserAccessByLogin(login);

            if (userAccess == null)
            {
                throw new Exception($"Authorization credentials rejected");
            }
            if (_kdfService.Dk(password, userAccess.Salt) != userAccess.Dk)
            {
                throw new Exception($"Authorization credentials rejected.");
            }
            return userAccess;
        }

        [HttpGet]
        public JsonResult SignIn()
        {
            UserAccess userAccess;
            try
            {
                userAccess = Authenticate();
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Status = 401,
                    Data = ex.Message
                });
            }
            
            HttpContext.Session.SetString(authSessionKey,
                JsonSerializer.Serialize(userAccess));

            return Json(new
            {
                Status = 200,
                Data = "OK"
            });
        }

        [HttpGet]
        public JsonResult LogIn()
        {
            UserAccess userAccess;
            try
            {
                userAccess = Authenticate();
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Status = 401,
                    Data = ex.Message
                });
            }
            // Токени.
            // цифрові "посвідчення", що несуть інформацію про користувача
            // За прямою наявністю інформації токени поділяють на 
            //  JWT - з наявністю інформації
            //  Bearer - лише з ідентифікатором токена
            AccessToken accessToken = new()
            {
                Jti = Guid.NewGuid().ToString(),
                Sub = userAccess.Id,
                Iat = _timeService.TimeStamp().ToString(),
                Exp = (_timeService.TimeStamp() + (long)1e5).ToString(),
                //Exp = ((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) + 10_000).ToString(),
                iss = nameof(ASP_ITStep),
                Aud = userAccess.RoleId
            };

            _dataContext.AccessTokens.Add(accessToken);
            _dataContext.SaveChanges();

            var jwt = new
            {
                accessToken.Jti,
                accessToken.Sub,
                accessToken.Iat,
                accessToken.Exp,
                accessToken.iss,
                accessToken.Aud,
                userAccess.UserData.Name,
                userAccess.UserData.Email,
            };

            return Json(new
            {
                Status = 200,
                Data = _jwtService.EncodeJwt(jwt)
            });
        }

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

        [HttpPatch]
        public async Task<JsonResult> UpdateAsync()
        {
            bool isAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated)
            {
                return Json(new
                {
                    Status = 401,
                    Data = "UnAuthorized"
                });
            }
            var userLogin = HttpContext
               .User
               .Claims
               .First(c => c.Type == ClaimTypes.Sid)
               .Value;

            var ua = _dataAccessor.GetUserAccessByLogin(userLogin, isEditable: true);
            if (ua == null)
            {
                return Json(new
                {
                    Status = 403,
                    Data = "Forbidden"
                });
            }

            //  звертаємось до тіла запиту напряму, зчитуємо як String 
            using StreamReader reader = new(Request.Body, Encoding.UTF8);
            var requestBody = await reader.ReadToEndAsync();
            if(requestBody == null)
            {
                return Json(new
                {
                    Status = 400,
                    Data = "Body must not be empty"
                });
            }
            JsonElement json;
            try
            {
                json = JsonSerializer.Deserialize<JsonElement>(requestBody);
            }
            catch (Exception ex) 
            {
                _logger.LogInformation("JSON decode error {ex}", ex.Message);
                return Json(new
                {
                    Status = 400,
                    Data = "Body must be JSON array"
                });
            }
            if(json.ValueKind != JsonValueKind.Array)
            {
                return Json(new
                {
                    Status = 422,
                    Data = "Body must be valid JSON string"
                });
            }

            foreach (var element in json.EnumerateArray())
            {
                String value = element.GetProperty("value").GetString()!;
                String field = element.GetProperty("field").GetString()!;
                switch(element.GetProperty("field").GetString())
                {
                    case "Name": ua.UserData.Name = value; break;
                    case "Email": ua.UserData.Email = value; break;
                    default:
                        return Json(new
                        {
                            Status = 409,
                            Data = $"Conflict: undefined field '{field}'"
                        });

                }
            }
            await _dataContext.SaveChangesAsync();
            return Json(new
            {
                Status = 202,
                Data = "Accepted"
            });
        }

        [HttpDelete]
        public async Task<JsonResult> DeleteAsync()
        {
            String authControl = HttpContext.Request.Headers["Authentication-Control"].ToString();
            if(String.IsNullOrEmpty(authControl))
            {
               return Json(new
               {
                Status = 400,
                Data = "Missing Header'Authentication-Control'"
               });
            }
            authControl = Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(authControl));

            bool isAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated)
            {
                return Json(new
                {
                    Status = 401,
                    Data = "UnAuthorized"
                });
            }
            String userLogin = HttpContext
                        .User
                        .Claims
                        .First(c => c.Type == ClaimTypes.Sid)
                        .Value;
            if(userLogin != authControl)
            {
                return Json(new
                {
                    Status = 403,
                    Data = "Forbidden"
                });
            }

            bool isDeleted = await _dataAccessor.DeleteUserAsync(authControl);
            if (isDeleted)
            {
                HttpContext.Session.Remove(authSessionKey);
                return Json(new
                {
                    Status = 200,
                    Data = "Deleted"
                });
            }
            else {
                return Json(new
                {
                    Status = 409,
                    Data = "Conflict.Not Deleted"
                });
            }
        }

        public ViewResult Profile(String id)
        {
            UserProfilePageModel model = new();
            var ua = _dataAccessor.GetUserAccessByLogin(id);
            if (ua == null)
            {
                model.IsPersonal = null;
            }
            else
            {
                model.Name = ua.UserData.Name;
               
                model.RegisteredAt = ua.UserData.RegisteredAt;
                bool isAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false;
                if (isAuthenticated)
                {
                    model.Email = ua.UserData.Email;
                    // дістаємо свій логін (з яким автентифіковані)
                    String userLogin = HttpContext
                        .User
                        .Claims
                        .First(c => c.Type == ClaimTypes.Sid)
                        .Value;
                    if (ua.Login == userLogin)  // Перегляд свого профілю
                    {
                        model.IsPersonal = true;
                        model.Birthdate = ua.UserData.Birthdate;
                    }
                    else  // Перегляд профілю іншого користувача
                    {
                        model.IsPersonal = false;
                    }
                }
                else  // Перегляд невідомого профілю у неавторизованому режимі
                {
                    model.IsPersonal = false;
                }
            }

            return View(model);
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