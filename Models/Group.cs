using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server_Tehnologii_Web.Models
{
    public class Group
    {
        [Key] public long Id { get; set; }
        public string Denumire { get; set; }
        public ICollection<User> Members { get; set; }
        public ICollection<Message> Messages { get; set; }

    }
}