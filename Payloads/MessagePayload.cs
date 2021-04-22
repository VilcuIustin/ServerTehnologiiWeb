using System.ComponentModel.DataAnnotations;

namespace Server_Tehnologii_Web.Payloads
{
    public class MessagePayload
    {
        [Required] public string Message { get; set; }
        [Required] public long Id { get; set; }
    }
}