using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _4bet.Models
{
    public class Profile
    {
        [Required]
        public string DisplayName { get; set; }
        public double Betcoins { get; set; }
        [Key]
        public string UserId { get; set; }
        public double AmountBetSoFar { get; set; }
    }
}