using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Haumea_Core.Collections;
using Haumea_Core.Geometric;
using Haumea_Core.Rendering;
using Haumea_Core.UIForms;

using TagIdMap = Haumea_Core.Collections.BiDictionary<int, string>;

namespace Haumea_Core.Game.Parsing
{
    public static class Initializer
    {
        public static InitializedWorld Initialize(RawGameData data, ContentManager content)
        {
            var realmsTagId    = InitializeRealmTags(data.RawRealms);
            var provincesTagId = InitializeProvinceTags(data.RawProvinces);

            var events    = new EventController();
            var mapGraph  = InitializeMapGraph(data.RawConnectors, provincesTagId);
            var provinces = InitializeProvinces(data.RawProvinces, mapGraph);
            var realms    = new Realms();
            var units     = InitializeUnits(data.RawArmies, provincesTagId, realmsTagId, provinces, events);

            // TODO: I have realized these two are so tangled into each other they should probably be merged
            // ..... Something like "mapView"
         
            WindowsTree windows = new WindowsTree();
            FormCreator ui = new FormCreator(content, windows);
            ProvincesView provincesView = InitializeProvincesView(data.RawProvinces, provinces);
            UnitsView unitsView = InitializeUnitsView(provinces, units, ui);
            DebugTextInfo debugView = new DebugTextInfo(provinces, units, realmsTagId, provincesTagId);

            IList<IModel> models = new List<IModel> {
                events, provinces, realms, units
            };
            
            IList<IView> views = new List<IView> {
                provincesView, unitsView, windows, debugView
            };

            foreach (IView view in views)
            {
                view.LoadContent(content);
            }

            return new InitializedWorld(models, views, windows);
        }

        private static BiDictionary<int, string> InitializeProvinceTags(IList<RawProvince> rProvinces)
        {
            TagIdMap tagIdMapping = new TagIdMap();

            for (int id = 0; id < rProvinces.Count; id++) {
                tagIdMapping.Add(id, rProvinces[id].Tag);
            }

            return tagIdMapping;            
        }

        private static BiDictionary<int, string> InitializeRealmTags(IList<RawRealm> rRealms)
        {
            TagIdMap tagIdMapping = new TagIdMap();

            for (int id = 0; id < rRealms.Count; id++)
            {
                tagIdMapping[id] = rRealms[id].Tag;
            }

            return tagIdMapping;
        }

        private static Provinces InitializeProvinces(IList<RawProvince> rProvinces, NodeGraph<int> graph)
        {
            Poly[] boundaries = new Poly[rProvinces.Count];
            ISet<int> waterProvinces = new HashSet<int>();

            for (int id = 0; id < rProvinces.Count; id++) {
                boundaries[id] = rProvinces[id].Poly;
                if (rProvinces[id].IsWater)
                {
                    waterProvinces.Add(id);
                }
            }

            return new Provinces(boundaries, waterProvinces, graph);
        }

        private static NodeGraph<int> InitializeMapGraph(IList<RawConnector> rConns, TagIdMap provinceTagId)
        {
            IList<Connector<int>> conns = new List<Connector<int>>();

            foreach (RawConnector rconn in rConns)
            {
                int ID1 = provinceTagId[rconn.Tag1];
                int ID2 = provinceTagId[rconn.Tag2];
                conns.Add(new Connector<int>(ID1, ID2, rconn.Cost));
            }

            return new NodeGraph<int>(conns, true);
        }

        private static Units InitializeUnits(IList<RawArmy> rawArmies, TagIdMap provinceTagId, TagIdMap realmsTagId,
            Provinces provinces, EventController events)
        {
            Units units = new Units(provinces, events);

            foreach (RawArmy rawArmy in rawArmies)
            {
                int ownerID    = realmsTagId[rawArmy.Owner];
                int locationID = provinceTagId[rawArmy.Location];
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
    
        private static UnitsView InitializeUnitsView(Provinces provinces, Units units, FormCreator ui)
        {
            return new UnitsView(provinces, units, ui);
        }
    }
        
    public struct InitializedWorld
    {
        public IList<IModel> Models { get; }
        public IList<IView> Views { get; }
        public WindowsTree Windows  { get; }

        public InitializedWorld(IList<IModel> models, IList<IView> views, WindowsTree windows)
        {
            Models = models;
            Views = views;
            Windows = windows;
        }
    }
}

