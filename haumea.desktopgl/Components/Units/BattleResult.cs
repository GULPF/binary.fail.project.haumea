using System;

namespace Haumea.Components
{
    public class BattleResult
    {
        public int Winner { get; }
        public int Losses { get; }
        public int WarID { get; }

        public BattleResult(int winner, int losses, int warID)
        {
            Winner = winner;
            Losses = losses;
            WarID = warID;
        }
    }
}

