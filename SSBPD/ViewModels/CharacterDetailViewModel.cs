using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.ViewModels
{
    public class CharacterDetailViewModel
    {
        public IEnumerable<Player> players;
        public int averageElo;
        public string characterName;

        public CharacterDetailViewModel(IEnumerable<Player> players, Character character)
        {
            this.players = players;
            this.averageElo = Convert.ToInt32(players.Average(p => p.ELO));
            this.characterName = character.DisplayString();
        }
    }
}