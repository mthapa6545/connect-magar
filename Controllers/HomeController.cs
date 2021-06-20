using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ConnectMagar.Data;
using ConnectMagar.Models;

namespace ConnectMagar.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ConnectMagarContext _db;

        public HomeController(ILogger<HomeController> logger, ConnectMagarContext db)
        {
            _logger = logger;
            _db= db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("Register")]
        public IActionResult Register()
        {
            Account model = new Account();
            return View(model);
        }

        [Route("Register")]
        [HttpPost]
        public IActionResult Register(Account model)
        {
            if(ModelState.IsValid)
            {
                _db.Accounts.Add(model);
                _db.SaveChanges();
                
                TempData["Msg"]="Success";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [Route("Login")]
        public IActionResult Login()
        {
            Account model = new Account();
            return View(model);
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> LoginAsync(string username, string password)
        {
            if(ModelState.IsValid)
            {
                var account = _db.Accounts.FirstOrDefault(x=> x.Email== username);
                if(account != null  && account.Password == password)
                {
                    var claims = new List<Claim>
                    {
                        new Claim("username", username),
                        new Claim("image", string.Format("{0}-{1}-{2}.jpg",account.FirstName, account.LastName, account.AccountID)),
                        new Claim("displayname", string.Format("{0} {1}", account.FirstName, account.LastName )),
                        new Claim("role", "User")
                    };

                    await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "role")));

                    account.LoginFailed=0;
                    return RedirectToAction("Index");
                }
                else
                {
                    //count incorrect attempt
                    if(account!=null)
                    {
                        account.LoginFailed = account.LoginFailed+1;
                        if(account.LoginFailed >=3)
                        {
                            ViewBag.Error = "Your account has been locked. Please try again after 15 min.";
                        }
                        else
                        {
                            ViewBag.Error= "Username or password is incorrect.";
                        }
                    }
                }
                _db.SaveChanges();
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Logout")]

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
