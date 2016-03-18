using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Haumea_Core.Geometric;

namespace Haumea_Core.Game.Parsing
{
    // None of these raw-structs are intended for anyting other than temporarily holding parsed data. 
    public struct RawProvince
    {
        public Poly Poly { get; }
        public string Tag { get; }
        public Color Color { get; }
        public bool IsWater { get; }

        public RawProvince(Poly poly, string tag, Color color, bool isWater)
        {
            Poly = poly;
            Tag = tag;
            Color = color;
            IsWater = isWater;
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
}

