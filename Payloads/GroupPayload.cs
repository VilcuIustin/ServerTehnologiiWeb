using System.ComponentModel.DataAnnotations;

namespace Server_Tehnologii_Web.Payloads
{
    public class GroupPayload
    {
        [Required] public string Denumire { get; set; }
    }
}