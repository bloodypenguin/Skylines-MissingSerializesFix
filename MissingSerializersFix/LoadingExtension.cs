
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
            PackageHelperDetour.Init();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            foreach (var kvp in PackageHelperDetour.subBuildings)
            {
                var mainBuilding = PrefabCollection<BuildingInfo>.FindLoaded(kvp.Key);
                if (mainBuilding == null)
                {
                    continue;
                }
                var subBuildings = kvp.Value;
                for (var index = 0; index < subBuildings.Count; index++)
                {
                    mainBuilding.m_subBuildings[index].m_buildingInfo = PrefabCollection<BuildingInfo>.FindLoaded(subBuildings[index]);
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
        }

        public override void OnReleased()
        {
            base.OnReleased();
            PackageHelperDetour.Revert();
        }
    }
}
