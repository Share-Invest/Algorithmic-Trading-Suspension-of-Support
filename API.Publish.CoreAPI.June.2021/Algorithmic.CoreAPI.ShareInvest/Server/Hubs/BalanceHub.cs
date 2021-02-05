﻿using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ShareInvest.Hubs
{
	[Authorize]
	public class BalanceHub : Hub
	{
		public override async Task OnConnectedAsync() => await base.OnConnectedAsync();
		public override async Task OnDisconnectedAsync(Exception exception) => await base.OnDisconnectedAsync(exception);
		public async Task SendBalanceMessage(string user, Catalog.Models.Balance balance) => await Clients.User(user).SendAsync("ReceiveBalanceMessage", balance);
	}
}