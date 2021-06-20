using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectMagar.Models
{
    public class Account
    {
        public int AccountID { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "First Name")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, ErrorMessage = "Must be more than 8 characters, at least 1 upper case, 1 lower case and 1 special characters. ", MinimumLength = 8)]
        [RegularExpression(@"^((?=.*[A-Z])(?=.*[a-z])(?=.*[^\da-zA-Z])).+$")]
        [DataType(DataType.Password)]
        public string Password { get; set; } 

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [NotMapped]
        public string ConfirmPassword { get; set; }
        public bool IsActive { get; set; }
        public int LoginFailed { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}