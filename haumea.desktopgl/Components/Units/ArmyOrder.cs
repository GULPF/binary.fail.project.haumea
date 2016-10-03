using System;
using Haumea.Collections;

namespace Haumea.Components
{
    internal class ArmyOrder
    {
        public int ArmyID { get; }
        public GraphPath<int> Path { get; }
        public int PathIndex { get; private set; }

        public int CurrentNode
        {
            get { return Path.Nodes[PathIndex]; }
        }

        public int NextNode
        {
            get { return Path.Nodes[PathIndex + 1]; }
        }

        public bool MoveForward()
        {
            PathIndex++;
            return PathIndex < Path.NJumps;
        }

        public ArmyOrder(int armyID, GraphPath<int> path)
        {
            ArmyID = armyID;
            Path = path;
            PathIndex = 0;
        }
    }
}

