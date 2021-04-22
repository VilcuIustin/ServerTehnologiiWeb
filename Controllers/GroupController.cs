using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server_Tehnologii_Web.Models;
using Server_Tehnologii_Web.Payloads;

namespace Server_Tehnologii_Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class GroupController : Controller
    {

        private readonly ServerContext _db;

        public GroupController(ServerContext db)
        {
            _db = db;
        }

        [HttpPost("creategroup")]
        public async Task<ActionResult> CreateGroup([FromBody] GroupPayload groupPayload)
        {
            var id = 0L;
            var isValid = long.TryParse(this.User.Claims.FirstOrDefault(c => c.Type == "Id").Value, out id);
            if (!isValid)
            {
                Response.StatusCode = 400;
                return new JsonResult(new {message = "Invalid token"});
            }

            var user = await _db.Users
                .Include(u => u.MyGroups)
                .Where(u => u.Id == id).FirstOrDefaultAsync();

            if (user == null)
            {
                Response.StatusCode = 404;
                return new JsonResult(new {message = "User not found"});
            }

            var group = new Group
            {
                Denumire = groupPayload.Denumire,
            };

            group.Members = new List<User> {user};
            user.MyGroups.Add(group);
            await _db.Groups.AddAsync(group);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("AfisareGrupuri")]
        public async Task<ActionResult> GetGroups([FromQuery] int pagezise, [FromQuery] int pagenumber)
        {
            var id = 0L;
            var isValid = long.TryParse(this.User.Claims.FirstOrDefault(c => c.Type == "Id").Value, out id);
            if (!isValid)
            {
                Response.StatusCode = 400;
                return new JsonResult(new {message = "Invalid token"});
            }

            var user = await _db.Users.Include(u => u.MyGroups)
                .ThenInclude(g => g.Messages)
                .Where(u => u.Id == id).AsNoTracking().FirstOrDefaultAsync();
            if (user == null)
            {
                Response.StatusCode = 404;
                return new JsonResult(new {message = "User not found"});
            }

            var last_message = new List<Message>();
            var grupuri = user.MyGroups.Skip(pagezise * pagenumber).Take(pagezise).ToList();
            foreach (var grup in grupuri)
            {
                if (grup.Messages.Count > 0)
                    last_message.Add(grup.Messages.ToList()[grup.Messages.Count - 1]);
                else
                    last_message.Add(new Message {Mesaj = "Nu exista mesaje"});

            }

            grupuri = grupuri.Select(g => new Group
            {
                Id = g.Id,
                Denumire = g.Denumire,
            }).ToList();


            return new JsonResult(new {grupuri, last_message});

        }

        [HttpGet("InfoGroup")]
        public async Task<ActionResult> getNumeGrup([FromQuery] long idGroup)
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
                .Where(g => g.Id == idGroup).SingleOrDefaultAsync();
            if (group == null)
            {
                Response.StatusCode = 404;
                return new JsonResult(new {message = "Group not found"});
            }

            var inGroup = group.Members.Where(m => m.Id == id).FirstOrDefault() != null;
            return new JsonResult(new
            {
                inGroup,
                nume = group.Denumire,
                participanti = group.Members.Count
            });
        }

        [HttpPost("joingroup")]
        public async Task<ActionResult> JoinGroup([FromBody] JoinGroupPayload payload)
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
                .Where(g => g.Id == payload.GroupId).SingleOrDefaultAsync();
            if (group == null)
            {
                Response.StatusCode = 404;
                return new JsonResult(new {message = "Group not found"});
            }

            var user = await _db.Users
                .Include(u => u.MyGroups)
                .Where(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                Response.StatusCode = 404;
                return new JsonResult(new
                {
                    message = "User not found"
                });
            }
            if (user.MyGroups.Where(g => g.Id == payload.GroupId).FirstOrDefault() != null)
            {
                Response.StatusCode = 400; 
                return new JsonResult(new
                {
                    message="you are already member in this chat"
                });
            }

            group.Members.Add(user);
            await _db.SaveChangesAsync();
            return new JsonResult(new {message="you are now a member of "+group.Denumire });
        }

    }
}