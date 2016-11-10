
using System.Linq;
using ICities;

namespace MissingSerializersFix
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            PackageHelperDetour.subBuildings.Clear();
            PackageHelperDetour.propVariations.Clear();
            PackageHelperDetour.treeVariations.Clear();
            PackageHelperDetour.Init();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                var info = PrefabCollection<BuildingInfo>.GetLoaded(i);
                if (info.m_subBuildings == null || info.m_subBuildings.Length == 0)
                {
                    continue;
                }
                foreach (var subInfo in info.m_subBuildings)
                {
                    if (PackageHelperDetour.subBuildings.ContainsKey(subInfo))
                    {
                        subInfo.m_buildingInfo = PrefabCollection<BuildingInfo>.FindLoaded(PackageHelperDetour.subBuildings[subInfo]);
                    }
                }
            }
            PackageHelperDetour.subBuildings.Clear();

            foreach (var kvp in PackageHelperDetour.propVariations)
            {
                var mainProp = PrefabCollection<PropInfo>.FindLoaded(kvp.Key);
                if (mainProp== null)
                {
                    continue;
                }
                var variations = kvp.Value;
                for (var index = 0; index < variations.Count; index++)
                {
                    mainProp.m_variations[index].m_finalProp = mainProp.m_variations[index].m_prop = PrefabCollection<PropInfo>.FindLoaded(variations[index]);
                }
            }
            PackageHelperDetour.propVariations.Clear();

            for (uint i = 0; i < PrefabCollection<TreeInfo>.LoadedCount(); i++)
            {
                var info = PrefabCollection<TreeInfo>.GetLoaded(i);
                if (info.m_variations == null || info.m_variations.Length == 0)
                {
                    continue;
                }
                foreach (var variation in info.m_variations)
                {
                    if (PackageHelperDetour.treeVariations.ContainsKey(variation))
                    {
                        variation.m_finalTree = variation.m_tree = PrefabCollection<TreeInfo>.FindLoaded(PackageHelperDetour.treeVariations[variation]);
                    }
                }
            }
            PackageHelperDetour.treeVariations.Clear();
        }

        public override void OnReleased()
        {
            base.OnReleased();
            PackageHelperDetour.Revert();
        }
    }
}
