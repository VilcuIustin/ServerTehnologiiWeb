using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Server_Tehnologii_Web.Payloads
{
    public class LoginPayload
    {
        [Required] public string Email { get; set; }
        [Required] public string Nume { get; set; }
        [Required] public string Password { get; set; }
    }
}