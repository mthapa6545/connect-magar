using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConnectMagar.Models
{
    public class ProfileViewModel
    {
        public Person Person { get; set; }
        public List<SelectListItem> StatesOfNepal { get; set; }
        public List<SelectListItem> StatesOfUSA { get; set; }

        public IFormFile ImageFile { get; set; }
        
    }
}