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
            var diplomacy = new Diplomacy(realms);
            var provinces = InitializeProvinces(data.RawProvinces, mapGraph, provincesTagId);
            var units     = InitializeUnits(data.RawArmies, provincesTagId, realmsTagId, provinces, diplomacy, events);
            var worldDate = new WorldDate(new DateTime(1452, 6, 23));;
            ProcessRawRealms(data.RawRealms, data.RawArmies, realmsTagId, provinces, units);

            var war = new War(new HashSet<int> { 0 }, new HashSet<int> { 1 }, 0, 1, CasusBellis.Conquest);
            diplomacy.StartRelation(0, 1, war);

            RenderInstruction[][] standardInstrs = new RenderInstruction[provinces.Boundaries.Length][];
            RenderInstruction[][] idleInstrs     = new RenderInstruction[provinces.Boundaries.Length][];

            for (int id = 0; id < provinces.Boundaries.Length; id++)
            {
                MultiPoly mpoly = provinces.Boundaries[id];

                Color c = data.RawProvinces[id].Color;
                if (data.RawProvinces[id].IsWater) c = Color.Blue;

				standardInstrs[id] = RenderInstruction.MultiPolygon(mpoly, c, Color.Blue);
				idleInstrs[id]     = new RenderInstruction[standardInstrs[id].Length];

				for (int n = 0; n < standardInstrs[id].Length; n++)
				{
					idleInstrs[id][n] = new RenderInstruction(standardInstrs[id][n], c.Darken());
				}
            }
                
            var dialogManager = new DialogManager();
            var mapView       = new MapView(provinces, units, standardInstrs, idleInstrs, dialogManager, diplomacy);
            var worldDateView = new WorldDateView(worldDate);

            List<IModel> models = new List<IModel> {
                events, provinces, realms, units
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

        private static Realms ProcessRawRealms(IList<RawRealm> rRealms, IList<RawArmy> rArmies, TagIdMap realmTagId,
            Provinces provinces, Units units)
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

            // Assign ownership of armies
            for (int n = 0; n < rArmies.Count; n++)
            {
                units.Recruit(n, realmTagId[rArmies[n].Owner]);
            }

            return realms;
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

        private static Units InitializeUnits(IList<RawArmy> rawArmies, TagIdMap provinceTagId, TagIdMap realmsTagId,
            Provinces provinces, Diplomacy diplomacy, EventController events)
        {
            Units units = new Units(provinces, diplomacy, events);

            foreach (RawArmy rawArmy in rawArmies)
            {
                int ownerID    = realmsTagId[rawArmy.Owner];
                int locationID = provinceTagId[rawArmy.Location];
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

