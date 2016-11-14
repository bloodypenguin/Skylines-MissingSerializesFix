
using System.Linq;
using ICities;

namespace MissingSerializersFix
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            PackageHelperDetour.Init();
            PackageHelperDetour.SubBuildings.Clear();
            PackageHelperDetour.PropVariations.Clear();
            PackageHelperDetour.TreeVariations.Clear();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            try
            {
                PatchSubBuildings();
            }
            finally
            {
                try
                {
                    PatchPropVariations();
                }
                finally
                {
                    PatchTreeVariations();
                }
            }
        }

        private static void PatchSubBuildings()
        {
            if (PackageHelperDetour.SubBuildings.Count > 0)
            {
                for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
                {
                    var info = PrefabCollection<BuildingInfo>.GetLoaded(i);
                    if (info?.m_subBuildings == null || info.m_subBuildings.Length == 0)
                    {
                        continue;
                    }
                    foreach (var subInfo in info.m_subBuildings)
                    {
                        if (subInfo == null)
                        {
                            continue;
                        }
                        if (PackageHelperDetour.SubBuildings.ContainsKey(subInfo))
                        {
                            subInfo.m_buildingInfo =
                                PrefabCollection<BuildingInfo>.FindLoaded(PackageHelperDetour.SubBuildings[subInfo]);
                        }
                    }
                }
            }
            PackageHelperDetour.SubBuildings.Clear();
        }

        private static void PatchPropVariations()
        {
            foreach (var kvp in PackageHelperDetour.PropVariations)
            {
                var mainProp = PrefabCollection<PropInfo>.FindLoaded(kvp.Key);
                if (mainProp == null)
                {
                    continue;
                }
                var variations = kvp.Value;
                for (var index = 0; index < variations.Count; index++)
                {
                    mainProp.m_variations[index].m_finalProp =
                        mainProp.m_variations[index].m_prop = PrefabCollection<PropInfo>.FindLoaded(variations[index]);
                }
            }
            PackageHelperDetour.PropVariations.Clear();
        }

        private static void PatchTreeVariations()
        {
            if (PackageHelperDetour.TreeVariations.Count > 0)
            {
                for (uint i = 0; i < PrefabCollection<TreeInfo>.LoadedCount(); i++)
                {
                    var info = PrefabCollection<TreeInfo>.GetLoaded(i);
                    if (info?.m_variations == null || info.m_variations.Length == 0)
                    {
                        continue;
                    }
                    foreach (var variation in info.m_variations)
                    {
                        if (variation == null)
                        {
                            continue;
                        }
                        if (PackageHelperDetour.TreeVariations.ContainsKey(variation))
                        {
                            variation.m_finalTree =
                                variation.m_tree =
                                    PrefabCollection<TreeInfo>.FindLoaded(PackageHelperDetour.TreeVariations[variation]);
                        }
                    }
                }
            }
            PackageHelperDetour.TreeVariations.Clear();
        }

        public override void OnReleased()
        {
            base.OnReleased();
            PackageHelperDetour.Revert();
            PackageHelperDetour.SubBuildings.Clear();
            PackageHelperDetour.PropVariations.Clear();
            PackageHelperDetour.TreeVariations.Clear();
        }
    }
}
