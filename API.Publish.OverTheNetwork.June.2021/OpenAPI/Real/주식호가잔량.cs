﻿using System.IO;
using System.Text;

using AxKHOpenAPILib;

namespace ShareInvest.OpenAPI.Catalog
{
	class 주식호가잔량 : Real
	{
		internal override void OnReceiveRealData(_DKHOpenAPIEvents_OnReceiveRealDataEvent e)
		{
			var data = e.sRealData.Split('\t');
			var index = 0;

			if (Connect.GetInstance().StocksHeld.TryGetValue(e.sRealKey, out Analysis sis) && int.TryParse(data[1][index] is '-' ? data[1][1..] : data[1], out int offer) && int.TryParse(data[4][index] is '-' ? data[4][1..] : data[4], out int bid))
			{
				sis.Offer = offer;
				sis.Bid = bid;
			}
			if (Lite)
			{
				var sb = new StringBuilder(data[index]);

				for (index = 0; index < 0x40; index++)
					sb.Append(';').Append(data[index + 1]);

				if (sb.Length > 0x1F)
					Server.WriteLine(string.Concat(e.sRealType, '|', e.sRealKey, '|', sb));
			}
		}
		internal override StreamWriter Server
		{
			get; set;
		}
		internal override AxKHOpenAPI API
		{
			get; set;
		}
		internal override bool Lite
		{
			get; set;
		}
		protected internal override int[] Fid => new int[] { 21, 41, 61, 81, 51, 71, 91, 42, 62, 82, 52, 72, 92, 43, 63, 83, 53, 73, 93, 44, 64, 84, 54, 74, 94, 45, 65, 85, 55, 75, 95, 46, 66, 86, 56, 76, 96, 47, 67, 87, 57, 77, 97, 48, 68, 88, 58, 78, 98, 49, 69, 89, 59, 79, 99, 50, 70, 90, 60, 80, 100, 121, 122, 125, 126, 23, 24, 128, 129, 138, 139, 200, 201, 238, 291, 292, 293, 294, 295, 621, 631, 622, 632, 623, 633, 624, 634, 625, 635, 626, 636, 627, 637, 628, 638, 629, 639, 630, 640, 13, 299, 215, 216 };
	}
}