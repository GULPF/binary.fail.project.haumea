using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Haumea.Geometric;

namespace Haumea.Parsing
{
    public struct RawGameData
    {
        public IList<RawProvince> RawProvinces { get; }
        public IList<RawRealm> RawRealms { get; }
        public IList<RawConnector> RawConnectors { get; }
        public IList<RawArmy> RawArmies { get; }

        public RawGameData(IList<RawProvince> rawProvinces, IList<RawRealm> rawRealms,
            IList<RawConnector> rawConnectors, IList<RawArmy> rawArmies)
        {
            RawProvinces = rawProvinces;
            RawRealms = rawRealms;
            RawConnectors = rawConnectors;
            RawArmies = rawArmies;
        }
    }

    public struct RawArmy
    {
        public string Owner { get; }
        public string Location { get; }
        public int NUnits { get; }

        public RawArmy(string owner, string location, int nUnits)
        {
            Owner = owner;
            Location = location;
            NUnits = nUnits;
        }
    }

    public struct RawConnector
    {
        public string Tag1 { get; }
        public string Tag2 { get; }
        public int Cost { get; }

        public RawConnector(string tag1, string tag2, int cost)
        {
            Tag1 = tag1;
            Tag2 = tag2;
            Cost = cost;
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

    public struct RawProvince
    {
        public IShape Shape { get; }
        public string Tag { get; }
        public Color Color { get; }
        public bool IsWater { get; }

        public RawProvince(IShape shape, string tag, Color color, bool isWater)
        {
            Shape = shape;
            Tag = tag;
            Color = color;
            IsWater = isWater;
        }
    }
}

