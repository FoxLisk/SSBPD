using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;

namespace SSBPD.ViewModels
{
    public class BracketViewModel
    {
        public BracketCell[,] winnersGrid;
        public BracketCell[,] losersGrid;
        public string errorMessage;
        public string bracketName;
        public BracketViewModel(BracketCell[,] winnersGrid, BracketCell[,] losersGrid, string bracketName, string error)
        {
            if (String.IsNullOrWhiteSpace(error))
            {
                this.winnersGrid = winnersGrid;
                this.losersGrid = losersGrid;
                this.bracketName = bracketName;
            }
            else
            {
                this.errorMessage = error;
            }
            
        }

    }

    public class BracketCell
    {
        private Player player;
        public string tag
        {
            get
            {
                if (player != null)
                {
                    return player.Tag;
                }
                else if (playerId == 0)
                {
                    return "Bye";
                }
                else return "";
            }
        }
        private int playerId;
        private List<string> _classes;
        public string classes
        {
            get
            {
                return String.Join(" ", _classes);
            }
        }
        private SSBPDContext _db;
        public SSBPDContext db
        {
            get
            {
                if (_db == null)
                {
                    _db = new SSBPDContext();
                } return _db;
            }
            set
            {
                _db = value;
            }
        }
        public BracketCell(int playerId, params string[] classes)
        {
            this.playerId = playerId;
            this.player = db.Players.Find(playerId);
            _classes = classes.ToList();
            if (playerId == 0)
            {
                _classes.Add("bye");
            }
        }
        public BracketCell(params string[] classes)
        {
            playerId = -1;
            _classes = classes.ToList();
        }

    }
}
