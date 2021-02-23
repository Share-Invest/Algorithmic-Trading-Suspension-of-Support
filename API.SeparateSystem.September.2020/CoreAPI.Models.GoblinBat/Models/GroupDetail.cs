﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareInvest.Models
{
	public class GroupDetail
	{
		[StringLength(6), Column(Order = 1), ForeignKey("Group")]
		public string Code
		{
			get; set;
		}
		[StringLength(6), Column(Order = 2)]
		public string Date
		{
			get; set;
		}
		[NotMapped]
		public int[] Tick
		{
			get; set;
		}
		[NotMapped]
		public double[] Inclination
		{
			get; set;
		}
		[NotMapped]
		public string Title
		{
			get; set;
		}
		[NotMapped]
		public string Index
		{
			get; set;
		}
		[Required]
		public int Current
		{
			get; set;
		}
		[Required]
		public double Rate
		{
			get; set;
		}
		[Required]
		public double Compare
		{
			get; set;
		}
		[Required]
		public double Percent
		{
			get; set;
		}
		public virtual ICollection<Tendency> Tendencies
		{
			get; set;
		}
		public GroupDetail() => Tendencies = new HashSet<Tendency>();
	}
}