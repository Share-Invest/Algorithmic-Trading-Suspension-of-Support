﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;

namespace ShareInvest.Client
{
	public class Local
	{
		public static Local Instance
		{
			get
			{
				if (Client is null)
					Client = new Local();

				return Client;
			}
		}
		static Local Client
		{
			get; set;
		}
		public void PostContext<T>(string route, string code, T param) where T : class => new Task(async () =>
		{
			try
			{
				var response = await client.ExecuteAsync(new RestRequest(Crypto.Security.GetRoute(route, code), Method.POST)
					.AddHeader(Security.content_type, Security.json)
					.AddParameter(Security.json, JsonConvert.SerializeObject(param), ParameterType.RequestBody), source.Token);

				if (response.StatusCode.Equals(HttpStatusCode.OK) == false)
					Base.SendMessage(route, code, param.GetType());
			}
			catch (Exception ex)
			{
				Base.SendMessage(GetType(), ex.StackTrace);
				Base.SendMessage(ex.StackTrace, GetType());
			}
		}).Start();
		Local()
		{
			var security = new Security(byte.MinValue);
			client = new RestClient(security.Url)
			{
				Timeout = -1
			};
			source = new CancellationTokenSource();
		}
		readonly CancellationTokenSource source;
		readonly IRestClient client;
	}
}