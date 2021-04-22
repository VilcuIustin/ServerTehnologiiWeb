using System.ComponentModel.DataAnnotations;

namespace Server_Tehnologii_Web.Payloads
{
    public class InfoGroupPayload
    {
        [Required] public bool inGroup { get; set; }
        [Required] public string nume { get; set; } 
        [Required] public long participanti { get; set; }
    }
}