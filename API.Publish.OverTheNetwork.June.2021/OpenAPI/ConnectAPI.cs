﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using AxKHOpenAPILib;

using ShareInvest.Catalog.Models;
using ShareInvest.DelayRequest;
using ShareInvest.EventHandler;
using ShareInvest.Interface;
using ShareInvest.Interface.OpenAPI;

namespace ShareInvest.OpenAPI
{
	public sealed partial class ConnectAPI : UserControl, ISecuritiesAPI<SendSecuritiesAPI>
	{
		Stack<string> CatalogStocksCode(IEnumerable<string> market)
		{
			int index = 0;
			var sb = new StringBuilder(0x100);
			var stack = new Stack<string>(0x10);

			foreach (var str in market)
				if (string.IsNullOrEmpty(str) is false && axAPI.GetMasterStockState(str).Contains(Base.TransactionSuspension) is false)
				{
					if (index++ % 0x63 == 0x62)
					{
						stack.Push(sb.Append(str).ToString());
						sb = new StringBuilder();
					}
					sb.Append(str).Append(';');
				}
			stack.Push(sb.Remove(sb.Length - 1, 1).ToString());

			return stack;
		}
		IEnumerable<Tuple<string, string>> GetInformationOfCode(List<string> list, string[] market)
		{
			string exclusion, date = Base.DistinctDate;
			Delay.Milliseconds = 0x259;

			for (int i = 2; i < 4; i++)
				foreach (var om in axAPI.GetActPriceList().Split(';'))
				{
					exclusion = axAPI.GetOptionCode(om.Insert(3, "."), i, date);

					if (list.Exists(o => o.Equals(exclusion)))
						continue;

					list.Add(exclusion);
				}
			foreach (var stock in Enum.GetNames(typeof(Market)))
				if (Enum.TryParse(stock, out Market param))
					switch (param)
					{
						case Market.장내:
						case Market.코스닥:
						case Market.ETF:
							for (int i = 0; i < market.Length; i++)
							{
								var state = axAPI.GetMasterStockState(market[i]);

								if (state.Contains(Base.TransactionSuspension))
								{
									Send?.Invoke(this, new SendSecuritiesAPI(new Codes
									{
										Code = market[i],
										Name = axAPI.GetMasterCodeName(market[i]),
										MaturityMarketCap = state,
										Price = axAPI.GetMasterLastPrice(market[i])
									}));
									market[i] = string.Empty;
								}
							}
							break;

						default:
							foreach (var str in axAPI.GetCodeListByMarket(((int)param).ToString()).Split(';'))
							{
								var index = Array.FindIndex(market, o => o.Equals(str));

								if (index > -1)
									market[index] = string.Empty;
							}
							break;
					}
			var stack = CatalogStocksCode(market.Distinct().OrderBy(o => Guid.NewGuid()));
			list[1] = axAPI.GetFutureCodeByIndex(0x18);
			list.Add(axAPI.GetFutureCodeByIndex(0xD));

			while (stack.Count > 0)
				yield return new Tuple<string, string>("OPTKWFID", stack.Pop());

			foreach (var str in axAPI.GetSFutureList(string.Empty).Split('|'))
				if (string.IsNullOrEmpty(str) == false)
				{
					var temp = str.Split('^');

					if (temp[2].Equals(date) == false)
						list.Add(temp[0]);

					date = temp[2];
				}
			foreach (var code in list.Distinct().OrderBy(o => Guid.NewGuid()))
				yield return new Tuple<string, string>("Opt50001", code);
		}
		void OnReceiveTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e) => (API as Connect)?.TR.FirstOrDefault(o => (o.RQName != null ? o.RQName.Equals(e.sRQName) : o.PrevNext.ToString().Equals(e.sPrevNext)) && o.GetType().Name[1..].Equals(e.sTrCode[1..]))?.OnReceiveTrData(e);
		void OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e)
		{
			if (e.nErrCode == 0)
			{
				foreach (var code in GetInformationOfCode(new List<string> { axAPI.GetFutureCodeByIndex(0) }, axAPI.GetCodeListByMarket(string.Empty).Split(';')))
					Send?.Invoke(this, new SendSecuritiesAPI(code));

				if ((IsServer || Base.IsDebug) && axAPI.GetConditionLoad() == 1)
					Conditions = new Dictionary<int, string>();

				Send?.Invoke(this, new SendSecuritiesAPI(axAPI.GetLoginInfo("ACCLIST").Split(';')));
			}
			else
				Send?.Invoke(this, new SendSecuritiesAPI((short)e.nErrCode));
		}
		void OnReceiveRealData(object sender, _DKHOpenAPIEvents_OnReceiveRealDataEvent e) => (API as Connect)?.Real.FirstOrDefault(o => o.GetType().Name.Replace("_", string.Empty).Equals(e.sRealType, StringComparison.Ordinal))?.OnReceiveRealData(e);
		void OnReceiveMessage(object sender, _DKHOpenAPIEvents_OnReceiveMsgEvent e) => Send?.Invoke(this, new SendSecuritiesAPI(string.Concat("[", e.sRQName, "] ", e.sMsg[9..], "(", e.sScrNo, ")")));
		void OnReceiveChejanData(object sender, _DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
		{
			if (API.Chejan.TryGetValue(e.sGubun, out Chejan chejan))
				chejan.OnReceiveChejanData(e);
		}
		void OnReceiveConditionVersion(object sender, _DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
		{
			if (e.lRet == 1)
			{
				foreach (var condition in axAPI.GetConditionNameList().Split(';'))
					if (string.IsNullOrEmpty(condition) is false)
					{
						var conditions = condition.Split('^');

						if (int.TryParse(conditions[0], out int index))
							Conditions[index++] = conditions[^1];
					}
			}
			Base.SendMessage(sender.GetType(), e.sMsg);
		}
		void OnReceiveTrCondition(object sender, _DKHOpenAPIEvents_OnReceiveTrConditionEvent e) => Writer.WriteLine(string.Concat(e.nIndex < 0xA ? e.nIndex : Enum.GetName(typeof(Index), e.nIndex), '|', e.strCodeList));
		void OnReceiveRealCondition(object sender, _DKHOpenAPIEvents_OnReceiveRealConditionEvent e) => Writer.WriteLine(string.Concat(e.strType, '|', e.strConditionIndex, ';', e.sTrCode));
		public ConnectAPI()
		{
			InitializeComponent();
			ConnectToReceiveRealTime = new NamedPipeServerStream(Process.GetCurrentProcess().ProcessName.Split(' ')[^1], PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
		}
		public dynamic API
		{
			get; private set;
		}
		public string[] Account
		{
			get; set;
		}
		public bool Start
		{
			get; private set;
		}
		public uint Count
		{
			get; private set;
		}
		public string SendErrorMessage(short error) => API.SendErrorMessage(error);
		public string Securities(string param) => axAPI.GetLoginInfo(param).Trim();
		public ISendSecuritiesAPI<SendSecuritiesAPI> InputValueRqData(string name, string param)
		{
			var ctor = Assembly.GetExecutingAssembly().CreateInstance(name) as TR;
			ctor.API = axAPI;

			if (Enum.TryParse(name[0x1C..], out CatalogTR tr) && API?.TR.Add(ctor))
			{
				switch (tr)
				{
					case CatalogTR.Opt10079:
						if (param.Length == 0x16)
							ctor.RQName = param.Substring(7, 0xC);

						ctor.Value = param.Substring(0, 6);
						API?.InputValueRqData(ctor);
						break;

					case CatalogTR.Opt50001:
						ctor.Value = param;
						ctor.RQName = param;
						API?.InputValueRqData(ctor);
						break;

					case CatalogTR.Opt50028:
					case CatalogTR.Opt50066:
						if (param.Length == 0x18)
							ctor.RQName = param.Substring(9, 0xC);

						ctor.Value = param.Substring(0, 8);
						API?.InputValueRqData(ctor);
						break;

					case CatalogTR.OPTKWFID:
						ctor.Value = param;
						API?.InputValueRqData(param.Split(';').Length, ctor);
						break;

					case CatalogTR.Opt10081:
						var str = param[7..];
						ctor.RQName = str;
						ctor.Value = string.Concat(param.Substring(0, 6), ';', str);
						API?.InputValueRqData(ctor);
						break;

					case CatalogTR.Opw00005:
					case CatalogTR.OPW20007:
					case CatalogTR.OPW20010:
						ctor.Value = param;
						API?.InputValueRqData(ctor);
						break;

					case CatalogTR.OPT50030:
					case CatalogTR.Opt50068:
						ctor.Value = param;
						ctor.RQName = param[9..];
						API?.InputValueRqData(ctor);
						break;
				}
				Count++;
			}
			return ctor;
		}
		public ISendSecuritiesAPI<SendSecuritiesAPI> RemoveValueRqData(string name, string param)
		{
			var ctor = (API as Connect)?.TR.FirstOrDefault(o => o.GetType().Name.Equals(name) && (o.RQName.Contains(param) || o.Value.Contains(param)));

			if (API?.TR.Remove(ctor))
				Base.SendMessage(GetType(), param, name);

			return ctor;
		}
		public bool TryGetValue(string key, out Analysis analysis)
		{
			var exist = API.StocksHeld.TryGetValue(key, out Analysis value);
			analysis = value;

			return exist;
		}
		public void StartProgress(bool lite) => BeginInvoke(new Action(async () =>
		{
			Start = true;
			axAPI.OnEventConnect += OnEventConnect;
			axAPI.OnReceiveMsg += OnReceiveMessage;
			axAPI.OnReceiveTrData += OnReceiveTrData;
			axAPI.OnReceiveRealData += OnReceiveRealData;
			axAPI.OnReceiveChejanData += OnReceiveChejanData;

			if (IsServer || Base.IsDebug)
			{
				axAPI.OnReceiveConditionVer += OnReceiveConditionVersion;
				axAPI.OnReceiveTrCondition += OnReceiveTrCondition;
				axAPI.OnReceiveRealCondition += OnReceiveRealCondition;
			}
			await ConnectToReceiveRealTime.WaitForConnectionAsync();
			Writer = new StreamWriter(ConnectToReceiveRealTime)
			{
				AutoFlush = true
			};
			API = Connect.GetInstance(axAPI, Writer, lite);
		}));
		public Analysis Append(string key, Analysis value) => API.StocksHeld[key] = value;
		public void SendOrder(ISendOrder order) => API?.SendOrder(order);
		public void SendCondition(int index, string name, int search) => API?.SendCondition(name, index, search);
		public int CorrectTheDelayMilliseconds(int milliseconds)
		{
			Delay.Milliseconds = milliseconds;

			return axAPI.GetConnectState();
		}
		public int CorrectTheDelayMilliseconds()
		{
			if (DateTime.Now.Hour > 0 && Delay.Milliseconds > 0xE00)
				Delay.Milliseconds--;

			return axAPI.GetConnectState();
		}
		public StreamWriter Writer
		{
			get; private set;
		}
		public Dictionary<int, string> Conditions
		{
			get; private set;
		}
		public bool IsServer
		{
			private get; set;
		}
		public IEnumerable<Analysis> Enumerator
		{
			get
			{
				foreach (var ctor in API.StocksHeld)
					yield return ctor.Value;
			}
		}
		public IEnumerable<ISendSecuritiesAPI<SendSecuritiesAPI>> Chejan
		{
			get
			{
				foreach (var ctor in API.Chejan)
					yield return ctor.Value;
			}
		}
		public ISendSecuritiesAPI<SendSecuritiesAPI> Real => (ISendSecuritiesAPI<SendSecuritiesAPI>)(API as Connect).Real.First(o => o.GetType().Equals(typeof(Catalog.장시작시간)));
		public NamedPipeServerStream ConnectToReceiveRealTime
		{
			get;
		}
		public event EventHandler<SendSecuritiesAPI> Send;
	}
}