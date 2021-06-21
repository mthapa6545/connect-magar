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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ConnectMagar.Services;

namespace ConnectMagar.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ConnectMagarContext _db;
        private readonly AuthService _authService;

        public HomeController(ILogger<HomeController> logger, ConnectMagarContext db)
        {
            _logger = logger;
            _db= db;
            _authService = new AuthService();
        }

        public IActionResult Index()
        {
            List<Person> persons = _db.Persons
            .Include(x=> x.USAAddress)
            .Include(x=> x.NepalAddress)
            .ToList();
            return View(persons);
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
                model.DateCreated = DateTime.Now;
                model.Password = _authService.HashPassword(model.Password);

                _db.Accounts.Add(model);
               

                //save info to person table
                _db.Persons.Add(new Person()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Approved = true,
                    Visible = true,
                    DateCreated = DateTime.Now,
                    DateApproved = DateTime.Now
                });
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
        public async Task<IActionResult> LoginAsync(string email, string password)
        {
            if(ModelState.IsValid)
            {
                var account = _db.Accounts.FirstOrDefault(x=> x.Email== email);
                if(account != null  && _authService.VerifyPassword(account.Password, password))
                {
                    var imageFile = string.Format("{0}-{1}-{2}.jpg", account.FirstName, account.LastName, account.AccountID);
                    var chk = System.IO.File.Exists(@"/img/persons/"+imageFile);
                    if(!chk)
                        imageFile="firstname-lastname-id.jpg";
                    
                    var claims = new List<Claim>
                    {
                        new Claim("email", email),
                        new Claim("image", imageFile),
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
                    else
                    {
                        ViewBag.Error="Account not found";
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

        [Route("Profile")]
        [Authorize]
        public IActionResult Profile()
        {
            ProfileViewModel model = new ProfileViewModel();

            model.StatesOfNepal = LookUp.GetStatesOfNepal().Select(x=> new SelectListItem { Value = x, Text = x }).ToList();
            model.StatesOfUSA = LookUp.GetStatesOfUSA().Select(x=> new SelectListItem { Value = x.Value, Text = x.Value }).ToList();
            var email=User.Claims.AsEnumerable().FirstOrDefault(c => c.Type == "email").Value;
            model.Person = _db.Persons
            .Include(x=>x.USAAddress)
            .Include(x=>x.NepalAddress)
            .FirstOrDefault(x=> x.Email == email);

            if(model.Person == null)
            {
                ViewBag.Error="Account not found";
            }
            return View(model);
        }

        [Route("Profile")]
        [HttpPost]
        [Authorize]
        public IActionResult Profile(ProfileViewModel model)
        {

            model.StatesOfNepal = LookUp.GetStatesOfNepal().Select(x=> new SelectListItem { Value = x, Text = x }).ToList();
            model.StatesOfUSA = LookUp.GetStatesOfUSA().Select(x=> new SelectListItem { Value = x.Value, Text = x.Value }).ToList();
            
            var email=User.Claims.AsEnumerable().FirstOrDefault(c => c.Type == "email").Value; 
            //if email is changed, update account table as well
            if(email!=model.Person.Email)
            {     
                //check email if its already exist
                if(_db.Persons.Any(x=> x.Email==model.Person.Email))
                {
                    ViewBag.Error="Email already exist in the system.";
                    return View(model);
                }
                else
                {
                    var account = _db.Accounts.FirstOrDefault(x=> x.Email == email);
                    account.Email = model.Person.Email;
                    account.DateUpdated = DateTime.Now;
                }
            }



            var person = _db.Persons
            .Include(x=>x.USAAddress)
            .Include(x=>x.NepalAddress)
            .FirstOrDefault(x=> x.Email== email);

            person.FirstName = model.Person.FirstName;
            person.LastName= model.Person.LastName;
            person.Gender = model.Person.Gender;
            person.Phone = model.Person.Phone;
            person.Email = model.Person.Email;
            person.Bio = model.Person.Bio;
            if(person.USAAddress==null)
            {
                person.USAAddress = new Address();
            }
            person.USAAddress.StreetName = model.Person.USAAddress.StreetName;
            person.USAAddress.City = model.Person.USAAddress.City;
            person.USAAddress.State = model.Person.USAAddress.State;
            person.USAAddress.ZipCode = model.Person.USAAddress.ZipCode;
            person.USAAddress.Country = "USA";

            if(person.NepalAddress== null)
            {
                person.NepalAddress = new Address();
            }
            person.NepalAddress.City = model.Person.NepalAddress.City;
            person.NepalAddress.State = model.Person.NepalAddress.State;
            person.NepalAddress.Country = "Nepal";

            person.DateUpdated = DateTime.Now;

            _db.SaveChanges();
            
            ViewBag.Msg = "Success";
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
