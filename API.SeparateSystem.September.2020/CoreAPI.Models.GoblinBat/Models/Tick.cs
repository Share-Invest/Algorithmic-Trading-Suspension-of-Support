﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareInvest.Models
{
	public class Tick
	{
		[Column(Order = 1), MinLength(6), MaxLength(8)]
		public string Code
		{
			get; set;
		}
		[Column(Order = 2), StringLength(8)]
		public string Date
		{
			get; set;
		}
		[Column(Order = 3), StringLength(9), Required]
		public string Open
		{
			get; set;
		}
		[Column(Order = 4), StringLength(9), Required]
		public string Close
		{
			get; set;
		}
		[Column(Order = 5), Required]
		public string Price
		{
			get; set;
		}
		[Required]
		public virtual Contents Contents
		{
			get; set;
		}
	}
}