using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ShareInvest.Catalog.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace ShareInvest.Controllers
{
	[Authorize, ApiController, Route(Security.route), Produces(Security.produces)]
	public class AccountController : ControllerBase
	{
		[AllowAnonymous, HttpPost, ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> PostContextAsync([FromBody] Account param)
		{
			try
			{
				if (param.Length > 0)
				{
					var temp = new string[param.Length];
					var stack = new Stack<string>();

					for (int i = 0; i < param.Length; i++)
						temp[i] = Crypto.Security.Decipher(param.Number[i]);

					foreach (var email in from o in context.User.AsNoTracking() where o.Kiwoom.Equals(param.Identity) select o.Email)
						stack.Push(context.Users.AsNoTracking().Single(o => o.Email.Equals(email)).Id);

					Security.User[param.Identity] = new User
					{
						Account = new Account
						{
							Length = param.Length,
							Identity = param.Identity,
							Name = param.Name,
							Security = param.Security,
							Number = temp
						},
						Logs = new Queue<Log>(),
						Balance = new Dictionary<string, Balance>(),
						Id = stack.ToArray()
					};
					return Ok(temp[^1]);
				}
			}
			catch (Exception ex)
			{
				await new Task(() => Console.WriteLine($"{GetType()}\n{ex.Message}\n{nameof(this.PostContextAsync)}"));
			}
			return Ok();
		}
		[HttpPut, ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<HttpStatusCode> PutContextAsync([FromBody] UserInformation param)
		{
			try
			{
				if (await context.Privacies.AsNoTracking().AnyAsync(o => o.CodeStrategics.Equals(param.Key)))
				{
					foreach (var pri in from o in context.Privacies where o.CodeStrategics.Equals(param.Key) select o)
					{
						var privacy = new Models.Privacy
						{
							Security = pri.Security,
							SecuritiesAPI = pri.SecuritiesAPI,
							SecurityAPI = Crypto.Security.Encrypt(new Privacies { Security = pri.Security, SecuritiesAPI = pri.SecuritiesAPI }, param.Check, param.Name is string),
							Account = pri.Account,
							CodeStrategics = pri.CodeStrategics,
							Coin = pri.Coin,
							Commission = pri.Commission
						};
						if (context.Privacies.All(o => o.Security.Equals(privacy.Security) && o.SecurityAPI.Equals(privacy.SecurityAPI)))
							return HttpStatusCode.OK;

						context.Entry(privacy).State = EntityState.Modified;
					}
					await context.BulkSaveChangesAsync();

					return HttpStatusCode.OK;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{GetType()}\n{ex.Message}\n{nameof(this.PutContextAsync)}");
			}
			return HttpStatusCode.BadRequest;
		}
		[HttpGet]
		public IEnumerable<UserInformation> GetContext(string key)
		{
			if (string.IsNullOrEmpty(key) is false)
			{
				var queue = new Queue<UserInformation>();
				string str = key, repeat = string.Empty;

				do
				{
					foreach (var user in from o in context.User.AsNoTracking() where o.Email.Equals(str) select o)
					{
						var check = string.Empty;

						if (context.Privacies.AsNoTracking().Any(o => o.CodeStrategics.Equals(user.Kiwoom)))
						{
							var tick = double.NaN;

							foreach (var privacy in from o in context.Privacies.AsNoTracking() where o.CodeStrategics.Equals(user.Kiwoom) select o)
								if (double.IsNaN(tick) || privacy.Commission / 0xE8D4A51000 == 0 && privacy.Commission > tick)
								{
									tick = privacy.Commission;

									if (string.IsNullOrEmpty(privacy.SecurityAPI) is false)
										check = Crypto.Security.Decipher(privacy.Security, privacy.SecuritiesAPI, privacy.SecurityAPI);
								}
						}
						queue.Enqueue(new UserInformation
						{
							Account = Crypto.Security.Decipher(user.Email, user.Account).Split(';'),
							Key = user.Kiwoom,
							Remaining = user.Payment > 0 ? new DateTime(user.Payment).AddDays(user.Coupon) : DateTime.Now,
							Check = check,
							Name = Security.User.TryGetValue(user.Kiwoom, out User su) ? su.Account.Name : string.Empty
						});
					}
					if (queue.Count > 0)
						return queue.ToArray();

					else if (str.Equals(key))
					{
						str = HttpUtility.UrlDecode(key);

						if (str.Equals(key) || str.Equals(repeat))
							return Array.Empty<UserInformation>();

						repeat = str;
					}
				}
				while (queue.Count == 0);
			}
			return Array.Empty<UserInformation>();
		}
		public AccountController(CoreApiDbContext context) => this.context = context;
		readonly CoreApiDbContext context;
	}
}