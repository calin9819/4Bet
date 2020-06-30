using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _4bet.Models
{
    public class Championship
    {
        [Key]
        public int ChampionshipId { get; set; }
        [Required]
        public string ChampionshipName { get; set; }
        [Required]
        public string Country { get; set; }
        public string PhotoFileName { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
        public virtual ICollection<FootballMatch> FootballMatches { get; set; }
    }
}