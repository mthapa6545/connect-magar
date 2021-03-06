using System;
namespace ConnectMagar.Models
{
    public class Person
    {
        public int PersonID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public int Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Address NepalAddress { get; set; }
        public Address USAAddress { get; set; }
        public bool Approved { get; set; }
        public bool Visible { get; set; }
        public string ImageFileName { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateApproved { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}