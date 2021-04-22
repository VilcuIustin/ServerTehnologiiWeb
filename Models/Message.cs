using System.ComponentModel.DataAnnotations;

namespace Server_Tehnologii_Web.Models
{
    public class Message
    {
        [Key] public long Id { get; set; }
        public string Mesaj { get; set; }
        
        public long SenderId { get; set; }
        public User Sender { get; set; }
        
        public long GroupId { get; set; }
        public Group GroupS { get; set; }
    }
}