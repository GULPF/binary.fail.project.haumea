using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Haumea.Geometric;
using Haumea.Collections;

namespace Haumea.Components
{ 
    public class Provinces : IModel
    {
        public NodeGraph<int> Graph { get; }

        /// <summary>
        /// The polygon boundaries for every province.
        /// </summary>
        public MultiPoly[] Boundaries { get; }

        /// <summary>
        /// Set of all provinces that are water.
        /// </summary>
        public ISet<int> WaterProvinces { get; }

        /// <summary>
        /// Indicates the owner (realm id) of every province.
        /// (-1 indicates province without owner maybe?)
        /// </summary>
        public IDictionary<int, int> Ownership { get; }

        /// <summary>
        /// Id|Tag => Tag|Id mappings
        /// </summary>
        public BiDictionary<int, string> TagIdMapping { get; }

        public Provinces(MultiPoly[] boundaries, ISet<int> waterProvinces, NodeGraph<int> graph,
            BiDictionary<int, string> tagIdMapping)
        {
            Boundaries = boundaries;
            WaterProvinces = waterProvinces;
            Graph = graph;
            TagIdMapping = tagIdMapping;
            Ownership = new Dictionary<int, int>();
        }
            
        public void Update(WorldDate date) {}

        public bool TryGetProvinceFromPoint(Vector2 point, out int foundID)
        {
            for (int id = 0; id < Boundaries.Length; id++)
            {
                if (Boundaries[id].IsPointInside(point))
                {
                    foundID = id;
                    return true;
                }
            }

            foundID = -1;
            return false;
        }

        public void Annex(int province, int realm)
        {
            Ownership.Add(province, realm);
        }
    }
}
