using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Haumea.Components;
using Haumea.Collections;
using Haumea.Geometric;
using Haumea.Rendering;
using Haumea.Parsing;
using Haumea.Dialogs;

using TagIdMap = Haumea.Collections.BiDictionary<int, string>;

namespace Haumea.Game
{
    public static class IntializeRawData
    {
        public static InitializedRawGameData Initialize(RawGameData data, ContentManager content)
        {
            var realmsTagId    = InitializeRealmTags(data.RawRealms);
            var provincesTagId = InitializeProvinceTags(data.RawProvinces);

            var events    = new EventController();
            var mapGraph  = InitializeMapGraph(data.RawConnectors, provincesTagId);
            var realms    = new Realms(realmsTagId);
            var wars      = new Wars(realmsTagId);
            var provinces = InitializeProvinces(data.RawProvinces, mapGraph, provincesTagId);
            var units     = InitializeUnits(data.RawArmies, realmsTagId, provinces, wars, events);
            var worldDate = new WorldDate(new DateTime(1452, 6, 23));;
            ProcessRawRealms(data.RawRealms, realmsTagId, provinces);

            wars.DeclareWar(0, 1, CasusBellis.Conquest, worldDate.Date);
                
            var dialogManager = new DialogManager();
            var mapView       = InitializeMapView(provinces, units, data.RawProvinces, dialogManager, wars);
            var worldDateView = new WorldDateView(worldDate);

            List<IModel> models = new List<IModel> {
                events, provinces, realms, units, wars
            };
            
            List<IView> views = new List<IView> {
                worldDateView, mapView, dialogManager
            };

            return new InitializedRawGameData(models, views, worldDate);
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

        // TODO:Remove
        private static Realms ProcessRawRealms(IList<RawRealm> rRealms, TagIdMap realmTagId, Provinces provinces)
        {
            var realms = new Realms(realmTagId);

            // Assign ownership of provinces
            foreach (var rRealm in rRealms)
            {
                foreach (string provinceTag in rRealm.ProvincesOwned)
                {
                    provinces.Annex(provinces.TagIdMapping[provinceTag], realms.TagIdMapping[rRealm.Tag]);
                }
            }

            return realms;
        }

        private static MapView InitializeMapView(Provinces provinces, Units units, IList<RawProvince> rProvinces,
            DialogManager dialogMgr, Wars wars)
        {
            RenderInstruction[][] standardInstrs = new RenderInstruction[provinces.Boundaries.Length][];
            RenderInstruction[][] idleInstrs     = new RenderInstruction[provinces.Boundaries.Length][];

            for (int id = 0; id < provinces.Boundaries.Length; id++)
            {
                MultiPoly mpoly = provinces.Boundaries[id];

                Color c = rProvinces[id].Color;
                if (rProvinces[id].IsWater) c = Color.Blue;

                standardInstrs[id] = RenderInstruction.MultiPolygon(mpoly, c, Color.Blue);
                idleInstrs[id]     = new RenderInstruction[standardInstrs[id].Length];

                for (int n = 0; n < standardInstrs[id].Length; n++)
                {
                    idleInstrs[id][n] = new RenderInstruction(standardInstrs[id][n], c.Darken());
                }
            }

            return new MapView(provinces, units, standardInstrs, idleInstrs, dialogMgr, wars);
        }

        private static Provinces InitializeProvinces(IList<RawProvince> rProvinces, NodeGraph<int> graph,
            TagIdMap provinceTagId)
        {
            MultiPoly[] boundaries = new MultiPoly[rProvinces.Count];
            ISet<int> waterProvinces = new HashSet<int>();

            for (int id = 0; id < rProvinces.Count; id++) {
                // TODO: Non-complex provinces should just be kept as a single polyon,
                // ..... not a shape.
                boundaries[id] = new MultiPoly(rProvinces[id].Polys.ToArray());
                if (rProvinces[id].IsWater)
                {
                    waterProvinces.Add(id);
                }
            }

            return new Provinces(boundaries, waterProvinces, graph, provinceTagId);
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

        private static Units InitializeUnits(IList<RawArmy> rawArmies, TagIdMap realmsTagId,
            Provinces provinces, Wars wars, EventController events)
        {
            Units units = new Units(provinces, wars, events);

            foreach (RawArmy rawArmy in rawArmies)
            {
                int ownerID    = realmsTagId[rawArmy.Owner];
                int locationID = provinces.TagIdMapping[rawArmy.Location];
                Units.Army army = new Units.Army(ownerID, locationID, rawArmy.NUnits);
                units.AddArmy(army);
            }

            return units;
        }
    }
        
    public struct InitializedRawGameData
    {
        public List<IModel> Models { get; }
        public List<IView> Views { get; }
        public WorldDate WorldDate { get; }

        public InitializedRawGameData(List<IModel> models, List<IView> views, WorldDate worldDate)
        {
            Models = models;
            Views = views;
            WorldDate = worldDate;
        }
    }
}

