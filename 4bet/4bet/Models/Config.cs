using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _4bet.Models
{
    public class Config
    {
        [Key]
        public int ConfigId { get; set; }
        [Required]
        public bool AutoPlay { get; set; }
        public double PrizePool { get; set; }
        public bool ContestEnabled { get; set; }
        public bool ContestOver { get; set; }
    }
}