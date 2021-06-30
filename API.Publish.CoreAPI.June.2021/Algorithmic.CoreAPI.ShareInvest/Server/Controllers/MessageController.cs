﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using ShareInvest.Catalog.Models;
using ShareInvest.Hubs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ShareInvest.Controllers
{
	[Authorize, ApiController, Route(Security.route), Produces(Security.produces)]
	public class MessageController : ControllerBase
	{
		[AllowAnonymous, HttpPost, ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> PostContextAsync([FromBody] Message param)
		{
			try
			{
				if (Security.User.TryGetValue(param.Key, out User user))
				{
					var log = param.Convey.Split('(');
					var message = log[0].Split(']');
					user.Logs.Enqueue(new Log
					{
						Time = DateTime.Now,
						Message = message[^1].Trim(),
						Code = message[0].Replace("[", string.Empty),
						Screen = log[^1].Remove(log[^1].Length - 1),
						Name = user.Account.Name
					});
					if (hub is not null && user.Id.Length > 0)
					{
						if (user.Id.Length == 1)
							await hub.Clients.User(user.Id[0]).SendAsync(method, log[0].Trim());

						else
							await hub.Clients.Users(user.Id).SendAsync(method, log[0].Trim());
					}
					return Ok(param.Convey);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{GetType()}\n{ex.Message}\n{nameof(this.PostContextAsync)}");
			}
			return Ok();
		}
		[HttpGet]
		public IEnumerable<Log> GetContext(string key)
		{
			if (string.IsNullOrEmpty(key) is false)
			{
				List<Log> list = null;

				foreach (var str in from o in context.User.AsNoTracking() where o.Email.Equals(key) || o.Email.Equals(HttpUtility.UrlDecode(key)) select o)
					if (Security.User.TryGetValue(str.Kiwoom, out User user))
					{
						if (list is null)
							list = new List<Log>(user.Logs);

						else
							list.AddRange(user.Logs);
					}
				if (list is not null)
					return list.ToArray();
			}
			return Array.Empty<Log>();
		}
		public MessageController(IHubContext<MessageHub> hub, CoreApiDbContext context)
		{
			this.hub = hub;
			this.context = context;
		}
		readonly CoreApiDbContext context;
		readonly IHubContext<MessageHub> hub;
		const string method = "ReceiveMessage";
	}
}