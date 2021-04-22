using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server_Tehnologii_Web.Models;

namespace Server_Tehnologii_Web.Controllers
{
    [Authorize]
    public class Server : Hub
    {
        private readonly ServerContext _db;

        public Server(ServerContext db)
        {
            _db = db;
        }


        public override async Task OnConnectedAsync()
        {
            var user = await getUser();
            if (user == null)
            {
                throw new HubException("User not found");
            }
            

            foreach (var group in user.MyGroups)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ""+group.Id);
                await Clients.Caller.SendAsync("conectat", "" + group.Id);
            }

            await base.OnConnectedAsync();
            //  var isValid = long.TryParse(Cont.Claims.FirstOrDefault(c => c.Type == "Id").Value, out id);
        }
        
        
        private async Task<User> getUser()
        {
            var id = 0L;
            var claims = (ClaimsIdentity) Context.User.Identity;

            var isValid = long.TryParse(claims.Claims.FirstOrDefault(c => c.Type == "Id").Value, out id);
            if (!isValid)
            {
                throw new HubException("Invalid token");
            }

            return  await _db.Users
                .Include(u => u.MyGroups)
                .Where(u => u.Id == id).FirstOrDefaultAsync();
          
        }
        
        
        

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await getUser();
            if (user == null)
            {
                throw new HubException("User not found");
            }
            
            foreach (var group in user.MyGroups)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, ""+group.Id);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}