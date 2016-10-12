using System;
using System.Collections.Generic;
using Haumea.Collections;

namespace Haumea.Components
{
    public class Resources : IModel
    {
        public IDictionary<int, Stockpile> Stockpiles { get; }

        private readonly Provinces _provinces;
        private readonly Units _units;

        public Resources(Provinces provinces, Units units, BiDictionary<int, string> realmsTagId)
        {
            Stockpiles = new Dictionary<int, Stockpile>();

            foreach (var pair in realmsTagId)
            {
                Stockpiles.Add(pair.Key, new Stockpile());
            }

            _provinces = provinces;
            _units = units;
        }

        public void Update(WorldDate date)
        {
            if (date.IsNewDay && date.Date.Day == 1)
            {
                foreach (var pair in _provinces.Ownership)
                {
                    int owner = pair.Value;
                    Stockpiles[owner].Gold += 1;
                }
            }

            Debug.WriteToScreen("Stockpile", Stockpiles[Realms.PlayerID].ToString());
        }
    }

    public class Stockpile
    {
        public int Food  { get; set; }
        public int Stone { get; set; }
        public int Gold  { get; set; }
        public int Wood  { get; set; }

        public Stockpile()
        {
            Food  = 0;
            Stone = 0;
            Gold  = 0;
            Wood  = 0;
        }

        public string ToString()
        {
            return string.Format("food = {0} stone = {1} gold = {2} wood = {3}",
                    Food, Stone, Gold, Wood);
        }
    }
}

