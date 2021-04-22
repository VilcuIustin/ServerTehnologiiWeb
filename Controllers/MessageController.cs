using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server_Tehnologii_Web.Models;
using Server_Tehnologii_Web.Payloads;

namespace Server_Tehnologii_Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MessageController : Controller
    {
        private readonly ServerContext _db;
        private readonly IHubContext<Server> _hub;

        public MessageController(ServerContext db, IHubContext<Server> hub)
        {
            _db = db;
            _hub = hub;
        }


        [HttpPost("AddMessage")]
        public async Task<ActionResult> AddMessage([FromBody] MessagePayload messagePayload)
        {
            var id = 0L;
            var isValid = long.TryParse(this.User.Claims.FirstOrDefault(c => c.Type == "Id").Value, out id);
            if (!isValid)
            {
                Response.StatusCode = 400;
                return new JsonResult(new {message = "Invalid token"});
            }

            var group = await _db.Groups
                .Include(g => g.Members)
                .Include(g => g.Messages)
                .Where(g => g.Id == messagePayload.Id)
                .FirstOrDefaultAsync();
            if (group == null)
            {
                Response.StatusCode = 404;
                return new JsonResult(new {message = "Group not found"});
            }

            var user = group.Members.Where(m => m.Id == id).FirstOrDefault();
            if (user == null)
            {
                Response.StatusCode = 401;
                return new JsonResult(new {message = "You are not a member of this group"});
            }

            var mesaj = new Message
            {
                Mesaj = messagePayload.Message,
                GroupId = messagePayload.Id,
                SenderId = id
            };
            group.Messages.Add(mesaj);
            _db.Update(group);
            // await _db.Messages.AddAsync(mesaj);
            await _db.SaveChangesAsync();
            await sendMessageInGroup("" + group.Id, mesaj.Mesaj, user.Nume, mesaj.Id,user.Id, group.Id);
            return Ok();
        }

        [HttpGet("GetMessage")]
        public async Task<ActionResult> GetMessage([FromQuery] long groupId, [FromQuery] int pagesize,
            [FromQuery] int pagenumber)
        {
            var id = 0L;
            var isValid = long.TryParse(this.User.Claims.FirstOrDefault(c => c.Type == "Id").Value, out id);
            if (!isValid)
            {
                Response.StatusCode = 400;
                return new JsonResult(new {message = "Invalid token"});
            }

            var group = await _db.Groups
                .Include(g => g.Members)
                .Include(g => g.Messages)
                .Where(g => g.Id == groupId).FirstOrDefaultAsync();
            if (group == null)
            {
                Response.StatusCode = 404;
                return new JsonResult(new {message = "Nu a fost gasit nici un grup cu acest id"});
            }

            var user = group.Members.Where(m => m.Id == id).FirstOrDefault();
            if (user == null)
            {
                Response.StatusCode = 401;
                return new JsonResult(new {message = "Nu faci parte din acest grup"});
            }

            var mesaje = group.Messages.Reverse().Skip(pagenumber * pagesize).Take(pagesize).Select(m => new
            {
                m.Mesaj,
                m.Id,
                userId = m.Sender.Id,
                m.Sender.Nume,
            });
            return new JsonResult(mesaje);
        }


        private async Task sendMessageInGroup(string grupId, string message, string username, long id, long userId, long groupId)
        {
            await _hub.Clients.Group(grupId).SendAsync("new message", message, username, id, userId, groupId);
        }
        
        
        
    }
}