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
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace ConnectMagar.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ConnectMagarContext _db;
        private readonly AuthService _authService;
        private readonly IHostingEnvironment hostingEnvironment;

        public HomeController(ILogger<HomeController> logger, ConnectMagarContext db, IHostingEnvironment environment)
        {
            _logger = logger;
            _db= db;
            _authService = new AuthService();
            hostingEnvironment = environment;
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
                model.Email = model.Email.Trim();
                model.Phone = model.Phone.Trim();
                model.DateCreated = DateTime.Now;
                model.Password = _authService.HashPassword(model.Password);

                _db.Accounts.Add(model);
               

                //save info to person table
                _db.Persons.Add(new Person()
                {
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    Email = model.Email.Trim(),
                    Phone = model.Phone.Trim(),
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
                var account = _db.Accounts.FirstOrDefault(x=> x.Email== email.ToLower());
                if(account != null  && _authService.VerifyPassword(account.Password, password))
                {
                    var imageFile = string.Format("{0}-{1}-{2}.jpg", account.FirstName.ToLower(), account.LastName.ToLower(), account.AccountID);
                    var chk = System.IO.File.Exists(@"/img/persons/"+imageFile);
                    if(!chk)
                        imageFile="firstname-lastname-id.jpg";
                    
                    var claims = new List<Claim>
                    {
                        new Claim("email", email.ToLower()),
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
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {

            model.StatesOfNepal = LookUp.GetStatesOfNepal().Select(x=> new SelectListItem { Value = x, Text = x }).ToList();
            model.StatesOfUSA = LookUp.GetStatesOfUSA().Select(x=> new SelectListItem { Value = x.Value, Text = x.Value }).ToList();
            
            var email=User.Claims.AsEnumerable().FirstOrDefault(c => c.Type == "email").Value; 
            //if email is changed, update account table as well
            if(email!=model.Person.Email.ToLower())
            {     
                //check email if its already exist
                if(_db.Persons.Any(x=> x.Email==model.Person.Email.ToLower()))
                {
                    ViewBag.Error="Email already exist in the system.";
                    return View(model);
                }
                else
                {
                    var account = _db.Accounts.FirstOrDefault(x=> x.Email == email);
                    account.Email = model.Person.Email.ToLower();
                    account.DateUpdated = DateTime.Now;
                }
            }



            var person = _db.Persons
            .Include(x=>x.USAAddress)
            .Include(x=>x.NepalAddress)
            .FirstOrDefault(x=> x.Email== model.Person.Email.ToLower());

            person.FirstName = model.Person.FirstName.Trim();
            person.LastName= model.Person.LastName.Trim();
            person.Gender = model.Person.Gender;
            person.Phone = model.Person.Phone.Trim();
            person.Email = model.Person.Email.Trim().ToLower();
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

            //Copy image to folder
            if(model.ImageFile.FileName.Length>0)
            {
                var fileExt = Path.GetExtension(model.ImageFile.FileName);
                 
                var fileName = $"{person.FirstName}-{person.LastName}-{person.PersonID}.{fileExt}";
                var path = Path.Combine(  
                    Directory.GetCurrentDirectory(), "wwwroot","img","persons",   
                  fileName); 

                using (var stream = System.IO.File.Create(path))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }
                person = _db.Persons
                .FirstOrDefault(x=> x.Email== model.Person.Email.ToLower());

                person.ImageFileName = fileName;
                _db.SaveChanges();
                ViewBag.Msg = "Information saved with image";
            }
            else
            {
                ViewBag.Msg = "Information saved without image file.";
            }
            
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
