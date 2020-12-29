﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareInvest.Models
{
	public class Security
	{
		[ForeignKey("Privacy"), Column(Order = 1)]
		public string Identify
		{
			get; set;
		}
		[ForeignKey("Codes"), Column(Order = 2), MinLength(6), MaxLength(8)]
		public string Code
		{
			get; set;
		}
		[Column(Order = 3), Required, StringLength(2)]
		public string Strategics
		{
			get; set;
		}
		[Column(Order = 4)]
		public string Contents
		{
			get; set;
		}
	}
}