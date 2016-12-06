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
        public static int PlayerID { get; } = 0;

        // Maps province => realm
        public BiDictionary<int, string> TagIdMapping { get; }

        public Realms(BiDictionary<int, string> tagIdMapping)
        {
            TagIdMapping = tagIdMapping;
        }

        public void Update(WorldDate date)
        {
            // will contain income updates etc
        }
    }    
}
