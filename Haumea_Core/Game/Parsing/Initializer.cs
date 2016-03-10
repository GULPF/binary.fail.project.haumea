using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Haumea_Core.Collections;
using Haumea_Core.Geometric;
using Haumea_Core.Rendering;

namespace Haumea_Core.Game.Parsing
{
    public static class Initializer
    {
        public static InitializedWorld Initialize(RawGameData data)
        {
            Provinces provinces = InitializeProvinces(data.RawProvinces);
            NodeGraph<int> mapGraph = InitializeMapGraph(data.RawConnectors, provinces);
            Realms realms = InitializeRealms(data.RawRealms, provinces);
            Units units = InitializeUnits(data.RawArmies, provinces, mapGraph, realms);

            // TODO: I have realized these two are so tangled into each other they should probably be merged
            // ..... Something like "mapView"
            ProvincesView provincesView = InitializeProvincesView(data.RawProvinces, provinces);
            UnitsView unitsView = InitializeUnitsView(data.RawProvinces, provinces, units);

            IList<IEntity> entities = new List<IEntity> {
                EventController.Instance, provinces, realms, units
            };

            IList<IView> views = new List<IView> {
                provincesView, unitsView
            };

            return new InitializedWorld(entities, views, provincesView);
        }

        private static Provinces InitializeProvinces(IList<RawProvince> rProvinces)
        {
            Poly[] boundaries = new Poly[rProvinces.Count];
            BiDictionary<int, string> tagIdMapping = new BiDictionary<int, string>();

            for (int id = 0; id < boundaries.Length; id++) {
                tagIdMapping.Add(id, rProvinces[id].Tag);
                boundaries[id] = rProvinces[id].Poly;
            }

            return new Provinces(boundaries, tagIdMapping);
        }

        private static NodeGraph<int> InitializeMapGraph(IList<RawConnector> rConns, Provinces provinces)
        {
            IList<Connector<int>> conns = new List<Connector<int>>();

            foreach (RawConnector rconn in rConns)
            {
                int ID1 = provinces.TagIdMapping[rconn.Tag1];
                int ID2 = provinces.TagIdMapping[rconn.Tag2];
                conns.Add(new Connector<int>(ID1, ID2, rconn.Cost));
            }

            return new NodeGraph<int>(conns, true);
        }

        private static Realms InitializeRealms(IList<RawRealm> rawRealms, Provinces provinces)
        {
            Realms realms = new Realms();

            foreach (RawRealm realm in rawRealms)
            {
                foreach (string provinceTag in realm.ProvincesOwned)
                {
                    int provinceID = provinces.TagIdMapping[provinceTag];
                    realms.AssignOwnership(provinceID, realm.Tag);
                }
            }

            return realms;
        }

        private static Units InitializeUnits(IList<RawArmy> rawArmies, Provinces provinces, NodeGraph<int> mapGraph, Realms realms)
        {
            Units units = new Units(mapGraph);

            foreach (RawArmy rawArmy in rawArmies)
            {
                int ownerID = realms.RealmTagIdMapping[rawArmy.Owner];
                int locationID = provinces.TagIdMapping[rawArmy.Location];
                Units.Army army = new Units.Army(ownerID, locationID, rawArmy.NUnits);
                units.AddArmy(army);
            }

            return units;
        }
    
        private static ProvincesView InitializeProvincesView(IList<RawProvince> rProvinces, Provinces provinces)
        {
            IDictionary<int, IDictionary<ProvincesView.RenderState, RenderInstruction>> instructions =
                new Dictionary<int, IDictionary<ProvincesView.RenderState, RenderInstruction>>();

            for (int id = 0; id < rProvinces.Count; id++) {

                var provinceInstructions = new Dictionary<ProvincesView.RenderState, RenderInstruction>();

                Color color = rProvinces[id].Color;

                provinceInstructions[ProvincesView.RenderState.Idle] = RenderInstruction.
                    Polygon(rProvinces[id].Poly.Points, color);
                provinceInstructions[ProvincesView.RenderState.Hover] = RenderInstruction.
                    Polygon(rProvinces[id].Poly.Points, color.Darken());

                instructions[id] = provinceInstructions;
            }

            return new ProvincesView(instructions, provinces);
        }
    
        private static UnitsView InitializeUnitsView(IList<RawProvince> rProvinces, Provinces provinces, Units units)
        {
            AABB[] labelBoxes = new AABB[rProvinces.Count];

            for (int id = 0; id < rProvinces.Count; id++)
            {
                labelBoxes[id] = rProvinces[id].Poly.FindBestLabelBox();
            }

            return new UnitsView(labelBoxes, provinces, units);
        }
    }
        
    public struct InitializedWorld
    {
        public IList<IEntity> Entities { get; }
        public IList<IView> Views { get; }

        // Until rendering is fixed, this has to be included by itself.
        public ProvincesView ProvincesView { get; }

        public InitializedWorld(IList<IEntity> entities, IList<IView> views, ProvincesView provincesView)
        {
            Entities = entities;
            Views = views;
            ProvincesView = provincesView;
        }
    }
}

