using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Haumea_Core.Geometric;
using Haumea_Core.Collections;

namespace Haumea_Core.Game
{ 
    // This is implemented using Data Oriented Design (DOD, see http://www.dataorienteddesign.com/dodmain/).

    public class Provinces : IEntity
    {
        
        private readonly Haumea _game;

        #region properties

        /// <summary>
        /// The polygon boundaries for every province.
        /// </summary>
        public Poly[] Boundaries { get; }

        /// <summary>
        /// Bidirectional dictionary mapping tag => id and id => tag.
        /// </summary>
        public BiDictionary<int, string> TagIdMapping { get; }

        /// <summary>
        /// Indicate which province the mouse is over.
        /// If -1, no province exists under the mouse.
        /// </summary>
        public int MouseOver { get; private set; }
    
        /// <summary>
        /// Holds the value which <code>Mouseover</code> changed from.
        /// </summary>
        public int LastMouseOver { get; private set; }

        /// <summary>
        /// Indicate which province is selected. If -1, not province is selected.
        /// </summary>
        public int Selected { get; private set; }

        /// <summary>
        /// Holds the value which <code>MouseOver</code> changed from.
        /// </summary>
        public int LastSelected { get; private set; }

        /// <summary>
        //  Graph over how the provinces are connected,
        //  with a distance value assigned to every connection.
        /// </summary>
        public NodeGraph<int> MapGraph { get; }

        /// <summary>
        /// Handles the realms: keeps track of which realm each province belongs to.
        /// </summary>
        public Realms Realms { get; }

        /// <summary>
        /// Indicate which realm ID belongs to the player.
        /// </summary>
        public int PlayerID { get; }

        #endregion

        public Provinces(RawGameData data, Haumea game)
        {
            Boundaries = new Poly[data.RawProvinces.Count];
            _game  = game;

            MouseOver     = -1;
            LastMouseOver = -1;
            Selected      = -1;
            LastSelected  = -1;

            TagIdMapping = new BiDictionary<int, string>();

            Realms = new Realms();
            PlayerID = 0;

            for (int id = 0; id < Boundaries.Length; id++) {
                TagIdMapping.Add(id, data.RawProvinces[id].Tag);
                Boundaries[id] = data.RawProvinces[id].Poly;
            }

            foreach (RawRealm realm in data.RawRealms)
            {
                foreach (string provinceTag in realm.ProvincesOwned)
                {
                    int provinceID = TagIdMapping[provinceTag];
                    Realms.AssignOwnership(provinceID, realm.Tag);
                }
            }

            IList<Connector<int>> conns = new List<Connector<int>>();

            foreach (RawConnector rconn in data.RawConnectors)
            {
                int ID1 = TagIdMapping[rconn.Tag1];
                int ID2 = TagIdMapping[rconn.Tag2];
                conns.Add(new Connector<int>(ID1, ID2, rconn.Cost));
            }

            MapGraph = new NodeGraph<int>(conns, true);
        }
            
        public void Update(WorldDate date) {}

        public void Select(int provinceID)
        {
            LastSelected = Selected;
            Selected = provinceID;   
        }

        public void Hover(int provinceID)
        {
            LastMouseOver = MouseOver;
            MouseOver = provinceID;
        }

        public void ClearSelection()
        {
            Selected = -1;
            LastSelected = -1;
        }
    }

    // Not intended to be used for anyting else other than temporarily holding parsed data. 
    public struct RawProvince
    {
        public Poly Poly { get; }
        public string Tag { get; }
        public Color Color { get; }
        public int Units { get; }

        public RawProvince(Poly poly, String tag, Color color, int units)
        {
            Poly = poly;
            Tag = tag;
            Color = color;
            Units = units;
        }
    }

    public struct RawRealm
    {
        public IList<string> ProvincesOwned { get; }
        public string Tag { get; }

        public RawRealm(IList<string> provinces, string tag)
        {
            ProvincesOwned = provinces;
            Tag = tag;
        }
    }
}
