﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ShareInvest.Catalog;
using ShareInvest.Catalog.Models;

namespace ShareInvest.Controllers
{
    [ApiController, Route(Security.route), Produces(Security.produces)]
    public class RetentionController : ControllerBase
    {
        [HttpPost, ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PostContextAsync([FromBody] Retention param)
        {
            var retention = await Security.Client.GetContextAsync(param.Code);
            var now = DateTime.Now;

            if (string.IsNullOrEmpty(retention.LastDate) == false && retention.LastDate.Substring(0, 6).Equals(now.ToString("yyMMdd")) ||
                string.IsNullOrEmpty(retention.Code) == false && retention.LastDate == null ||
                Security.SecuritiesCompany == 'X' && string.IsNullOrEmpty(retention.Code) == false && string.IsNullOrEmpty(retention.LastDate))
                return Ok();

            try
            {
                if (retention.Code.Length == 6)
                {
                    if (Security.Consensus.GrantAccess)
                    {
                        Queue<ConvertConsensus> queue;
                        Queue<FinancialStatement> context = null;

                        for (int i = 0; i < retention.Code.Length / 3; i++)
                        {
                            queue = await Security.Consensus.GetContextAsync(i, retention.Code);
                            int status = int.MinValue;

                            if (queue != null && queue.Count > 0)
                            {
                                status = await Security.Client.PostContextAsync(queue);

                                if (i == 0)
                                    context = new Client.Summary(Security.Key.Security).GetContext(retention.Code);

                                if (i == 1 && context != null)
                                    status = await Security.Client.PostContextAsync(context);
                            }
                            Base.SendMessage(retention.Code, status, GetType());
                        }
                    }
                    else if (await Security.Client.GetContextAsync(new IncorporatedStocks { Market = 'P' }) is int next &&
                        await Security.Client.PostContextAsync(new Client.IncorporatedStocks(Security.Key.Security).OnReceiveSequentially(next)) != 0xC8)
                        Base.SendMessage(retention.Code, next, GetType());
                }
            }
            catch (Exception ex)
            {
                Base.SendMessage(ex.StackTrace, GetType());
            }
            return Ok(retention);
        }
    }
}