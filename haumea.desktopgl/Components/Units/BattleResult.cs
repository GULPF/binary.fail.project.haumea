using System;

namespace Haumea.Components
{
    public class BattleResult
    {
        public int Attacker { get; }
        public int Defender { get; }
        public int Winner { get; }
        public int AttackerLoses { get; }
        public int DefenderLoses { get; }

        public BattleResult(int attacker, int defender, int winner, int attackerLoses, int defenderLoses)
        {
            Attacker = attacker;
            Defender = defender;
            AttackerLoses = attackerLoses;
            DefenderLoses = defenderLoses;
        }
    }
}

