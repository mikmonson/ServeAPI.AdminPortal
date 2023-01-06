using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AdminPortal.Models;
using AdminPortal.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPortal.Controllers
{
    public class AccountController : Controller
    {
        EdgeDBContext db;
        public AccountController(EdgeDBContext context)
        {
            db = context;
        }

        /*
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    // добавляем пользователя в бд
                    user = new User { Email = model.Email, Password = model.Password };
                    Role userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "user");
                    if (userRole != null)
                        user.Role = userRole;

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    await Authenticate(user); // аутентификация

                    return RedirectToAction("Index", "Home");
                }
                else
                    ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }*/

        [HttpGet]
        public IActionResult Login(string ReturnUrl)
        {
            ViewData["error"] = "";
            if (ReturnUrl != null)
            {
                Debug.WriteLine(ReturnUrl);
                ViewData["redirect"] = ReturnUrl;
                return View();
            }
            else
            {
                ViewData["redirect"] = "";
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string redirect)
        {
            ViewData["error"] = "";
            try
            {
                if ((ModelState.IsValid) && (model.Password.Length > 4) && (model.Login.Length > 2))
                {
                    //var hash = SecurePasswordHasher.Hash(model.Password);
                    User user = await db.Users.FirstOrDefaultAsync(u => u.Username == model.Login);
                    if (user != null)
                        if (SecurePasswordHasher.Verify(model.Password, user.Passwordhash) == true)
                        {
                            //var result = SecurePasswordHasher.Verify("mypassword", hash);
                            await Authenticate(user, user.Mustchangepassword); // аутентификация
                            if (user.Mustchangepassword == true)
                            {
                                return RedirectToAction("ChangePassword", "Account");
                            }
                            if (redirect != null)
                            {
                                return Redirect(redirect);
                            }
                            else
                            {
                                return RedirectToAction("Index", "Admin");
                            }
                        }
                    ModelState.AddModelError("", "Wrong username or password");
                }
            }
            catch {
                Debug.WriteLine("Error during login -< db search failed");
            }
            
                ViewData["error"] = "Wrong credentials. Please try again.";
                return View(model);            
        }

        private async Task Authenticate(User user, bool changepassword)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username),
                new Claim(ClaimsIdentity.DefaultIssuer, Convert.ToString(user.Customer_id)),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Userclass)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            if (changepassword == false) //Authentication with cookies
            {
                var authProperties = new AuthenticationProperties();
                authProperties.IsPersistent = false;
                authProperties.ExpiresUtc=DateTimeOffset.UtcNow.AddMinutes(60);
                authProperties.AllowRefresh = true;
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), authProperties);
            }
            else //Authentication with default session-based cookies which expire after browser is closed
            {
                var authProperties = new AuthenticationProperties();
                authProperties.IsPersistent = false;
                authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(2);
                authProperties.AllowRefresh = true;
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), authProperties);
            }
        }

        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordModel cpm)
        {
            try
            {
                if ((ModelState.IsValid))
                    if ((cpm.Oldpassword != null) && (cpm.Newpassword != null))
                        if ((cpm.Newpassword.Length > 4) && (cpm.Newpassword!= cpm.Oldpassword))
                        {
                            //string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
                            Debug.WriteLine(User.Identity.Name);
                            User user = db.Users.First(x => x.Username == User.Identity.Name);
                            if (SecurePasswordHasher.Verify(cpm.Oldpassword, user.Passwordhash) == true)
                            {
                                var hash = SecurePasswordHasher.Hash(cpm.Newpassword);
                                user.Lastpasswordchange = DateTime.Now;
                                user.Mustchangepassword = false;
                                user.Passwordhash = hash;
                                db.Users.Update(user);
                                db.SaveChanges();
                                return RedirectToAction("Index", "Admin");
                            } else
                            {
                                ViewData["error"] = "Old password is wrong";
                                return View();
                            }
                        }
            } catch
            {
                Debug.WriteLine("Error during password change -< db search failed");
            }
            ViewData["error"] = "Error. Please enter credentials again";
            return View();
        }
    }
}