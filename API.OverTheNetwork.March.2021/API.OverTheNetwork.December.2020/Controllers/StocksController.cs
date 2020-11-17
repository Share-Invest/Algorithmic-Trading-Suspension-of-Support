﻿using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ShareInvest.Catalog.Models;

namespace ShareInvest.Controllers
{
    [ApiController, Route(Security.route), Produces(Security.produces)]
    public class StocksController : ControllerBase
    {
        [HttpPost, ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PostContextAsync([FromBody] Queue<Stocks> param)
        {
            if (await Security.Client.PostContextAsync(param) > 0xC8)
            {
                var peek = param.Peek();
                Base.SendMessage(GetType(), peek.Code, peek.Retention);
            }
            return Ok();
        }
    }
}