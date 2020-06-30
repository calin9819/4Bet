using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _4bet.Models
{
    public class Bet
    {
        [Key]
        public int BetId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public double Odd { get; set; }
        [Required]
        public int MatchId { get; set; }
        public virtual FootballMatch FootballMatch { get; set; }
        [Required]
        public int Amount { get; set; }
        [Required]
        public int OutcomeChoice { get; set; }
        [Required]
        public string FirstPlayer { get; set; }
        [Required]
        public string SecondPlayer { get; set; }
        [Required]
        public bool Committed { get; set; }
        [Required]
        public bool MatchedResolved { get; set; }
        [Required]
        public DateTime ResolveTime { get; set; }
        public bool Winner { get; set; }
    }
}