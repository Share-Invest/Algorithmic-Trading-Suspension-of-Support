﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ShareInvest.Controllers
{
	[ApiController, Route(Security.route), Produces(Security.produces)]
	public class ConclusionController : ControllerBase
	{
		[HttpPut, ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> PutContextAsync([FromBody] Catalog.OpenAPI.Conclusion conclusion)
		{
			try
			{
				if (Progress.Collection.TryGetValue(conclusion.Code[0] is 'A' ? conclusion.Code[1..] : conclusion.Code, out Analysis analysis))
				{
					if (analysis.OrderNumber is null)
						analysis.OrderNumber = new Dictionary<string, dynamic>();

					if (await analysis.OnReceiveConclusion(conclusion) is Tuple<dynamic, bool, int> response)
					{
						analysis.Current = response.Item1;
						analysis.Wait = response.Item2;
						Strategics.Cash += response.Item3;
					}
				}
			}
			catch (Exception ex)
			{
				Base.SendMessage(ex.StackTrace, GetType());
			}
			return Ok();
		}
	}
}