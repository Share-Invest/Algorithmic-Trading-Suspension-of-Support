﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareInvest.Models
{
    public class Logs
    {
        [Key, Column(Order = 1), MaxLength(8)]
        public string Code
        {
            get; set;
        }
        [Key, Column(Order = 2)]
        public string Strategy
        {
            get; set;
        }
        [Required]
        public int Date
        {
            get; set;
        }
        [Required]
        public long Unrealized
        {
            get; set;
        }
        [Required]
        public long Revenue
        {
            get; set;
        }
        [Required]
        public long Cumulative
        {
            get; set;
        }
        [ForeignKey("Code")]
        public virtual Codes Codes
        {
            get; set;
        }
    }
}