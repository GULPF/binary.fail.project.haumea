using System.Collections.Generic;

using Haumea.Geometric;
using Haumea.Collections;

namespace Haumea.Components
{ 
    // This is implemented using Data Oriented Design (DOD, see http://www.dataorienteddesign.com/dodmain/).

    public class Provinces : IModel
    {
        public NodeGraph<int> Graph { get; }

        /// <summary>
        /// The polygon boundaries for every province.
        /// </summary>
        public IShape[] Boundaries { get; }

        /// <summary>
        /// Set of all provinces that are water.
        /// </summary>
        public ISet<int> WaterProvinces { get; }

        public Provinces(IShape[] boundaries, ISet<int> waterProvinces, NodeGraph<int> graph)
        {
            Boundaries = boundaries;
            WaterProvinces = waterProvinces;
            Graph = graph;
        }
            
        public void Update(WorldDate date) {}
    }
}
