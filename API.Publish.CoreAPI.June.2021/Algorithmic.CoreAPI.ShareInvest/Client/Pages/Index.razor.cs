﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

using Newtonsoft.Json;

using ShareInvest.Catalog.Models;

namespace ShareInvest.Pages
{
	[Authorize]
	public partial class IndexBase : ComponentBase
	{
		protected override void OnInitialized()
		{
			try
			{
				IsConfirm = true;
				Accounts = new List<string>();
			}
			catch (AccessTokenNotAvailableException exception)
			{
				exception.Redirect();
			}
			/*
			var accounts = await Http.GetFromJsonAsync<string[]>(Crypto.Security.GetRoute("Account"));

			foreach (var str in accounts)
			{
				if (str.Equals("Separation"))
				{
					IsClicked = true;

					break;
				}
				Accounts.Add(str);
			}
			if (IsClicked)
			{
				var index = Array.FindIndex(accounts, o => o.Equals("Separation"));

				Stock = accounts[index + 1];

				if (accounts.Length - index == 3)
					Futures = accounts[index + 2];
			}
			*/
		}
		protected internal async Task Send()
		{
			try
			{
				if (Accounts.Count > 0 && string.IsNullOrEmpty(Stock) is false)
					IsClicked = HttpStatusCode.OK.Equals((await Http.PutAsJsonAsync("Account", new Privacies
					{
						Account = string.IsNullOrEmpty(Futures) ? Stock : string.Concat(Stock, ';', Futures)

					})).StatusCode);
				else if (string.IsNullOrEmpty(InputIdentity) is false && InputIdentity.Length < 9 && string.IsNullOrEmpty(InputName) is false && InputName.Length < 9)
				{
					InputIdentity = Crypto.Security.Encrypt(InputIdentity);
					InputName = Crypto.Security.Encrypt(InputName);
					var confirm = new Confirm { Identity = InputIdentity, Name = InputName };
					var response = await Http.PostAsJsonAsync(confirm.GetType().Name, confirm);

					if (HttpStatusCode.OK.Equals(response.StatusCode))
					{
						var content = JsonConvert.DeserializeObject<Account>(await response.Content.ReadAsStringAsync());

						if (content.Length > 0)
						{
							Storage = content;

							foreach (var str in content.Number)
								if (string.IsNullOrEmpty(str) is false && str.StartsWith("Test") is false)
									Accounts.Add(str);
						}
					}
					Security.Identify = InputIdentity;
				}
			}
			catch (AccessTokenNotAvailableException exception)
			{
				exception.Redirect();
			}
			catch (Exception ex)
			{
				Base.SendMessage(ex.StackTrace, GetType());
			}
		}
		protected internal void OnReceiveTheSelectedButton(ChangeEventArgs e)
		{
			if (e.Value is string account)
			{
				if (account[^2..].CompareTo("31") == 0)
					Futures = account;

				else
					Stock = account;

				IsClicked = false;
			}
		}
		protected internal static string ConvertFormat(string account) => string.Format("{0}­ ─ ­{1}", account.Substring(0, 4), account.Substring(4, 4));
		protected internal bool IsClicked
		{
			get; private set;
		}
		protected internal bool IsConfirm
		{
			get; private set;
		}
		protected internal string InputName
		{
			get; set;
		}
		protected internal string InputIdentity
		{
			get; set;
		}
		protected internal string Stock
		{
			get; private set;
		}
		protected internal string Futures
		{
			get; private set;
		}
		protected internal List<string> Accounts
		{
			get; private set;
		}
		[Inject]
		HttpClient Http
		{
			get; set;
		}
		Account Storage
		{
			get; set;
		}
	}
}