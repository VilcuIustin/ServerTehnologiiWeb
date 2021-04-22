using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server_Tehnologii_Web.Models
{
    public class User
    {
        [Key]
        public long Id { get; set; }
        public string Email { get; set; }
        public string Nume { get; set; }
        public string Password { get; set; }
        public ICollection<Group> MyGroups { get; set; }
    }
}