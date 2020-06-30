using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace _4bet.Models
{
    public class FootballMatch
    {
        [Key]
        public int FootballMatchId { get; set; }
        public int ChampionshipId { get; set; }
        public virtual Championship Championship { get; set; }
        public int FirstTeamId { get; set; }
        public virtual Team FirstTeam { get; set; }
        public int SecondTeamId { get; set; }
        public virtual Team SecondTeam { get; set; }
        public double FirstTeamWinsOdd { get; set; }
        public double SecondTeamWinsOdd { get; set; }
        public double DrawOdd { get; set; }
        public DateTime ResolveTime { get; set; }
        public string Outcome { get; set; }
    }
}