using System;
using System.Collections.Generic;

using Haumea.Collections;

namespace Haumea.Components
{
    public class Realms : IModel
    {
        /// <summary>
        /// Indicate which realm ID belongs to the player.
        /// </summary>
        public static int PlayerID { get; private set; } = 0;

        // Maps province => realm
        public BiDictionary<int, string> TagIdMapping { get; }

        private readonly Provinces _provinces;
        private readonly Units _units;

        public Realms(BiDictionary<int, string> tagIdMapping, Provinces provinces, Units units)
        {
            TagIdMapping = tagIdMapping;

            _provinces = provinces;
            _units = units;
        }

        public void Update(WorldDate date) {}

        // These seem a little out of place, since they are just updating other objects properties.

        public void Annex(int province, int realm)
        {
            _provinces.Ownership.Add(province, realm);
        }

        public void Recruit(int army, int realm)
        {
            _units.Ownership.Add(army, realm);
        }
    }    
}
