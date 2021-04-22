using System.ComponentModel.DataAnnotations;

namespace Server_Tehnologii_Web.Payloads
{
    public class JoinGroupPayload
    {
        [Required]public long GroupId { get; set; }
    }
}