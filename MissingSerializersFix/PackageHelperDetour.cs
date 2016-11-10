using System;
using System.Collections.Generic;
using System.Reflection;
using ColossalFramework.Packaging;

namespace MissingSerializersFix
{
    public class PackageHelperDetour
    {
        private static RedirectCallsState state;
        private static IntPtr _intPtr;
        private static IntPtr _intPtr2;

        private static RedirectCallsState state2;
        private static IntPtr _intPtr1;
        private static IntPtr _intPtr3;

        public static Dictionary<string, List<string>> subBuildings = new Dictionary<string, List<string>>();
        public static Dictionary<string, List<string>> propVariations = new Dictionary<string, List<string>>();

        public static void Init()
        {
            _intPtr = typeof(PackageHelper).GetMethod("CustomSerialize",
                BindingFlags.Static | BindingFlags.Public).MethodHandle.GetFunctionPointer();
            _intPtr2 = typeof(PackageHelperDetour).GetMethod("CustomSerialize",
                BindingFlags.Static | BindingFlags.Public).MethodHandle.GetFunctionPointer();
            state = RedirectionHelper.PatchJumpTo(
                _intPtr,
                _intPtr2
                );
            _intPtr1 = typeof(PackageHelper).GetMethod("CustomDeserialize",
                BindingFlags.Static | BindingFlags.Public).MethodHandle.GetFunctionPointer();
            _intPtr3 = typeof(PackageHelperDetour).GetMethod("CustomDeserialize",
                BindingFlags.Static | BindingFlags.Public).MethodHandle.GetFunctionPointer();
            state2 = RedirectionHelper.PatchJumpTo(
                _intPtr1,
                _intPtr3
                );
        }

        public static void Revert()
        {
            RedirectionHelper.RevertJumpTo(_intPtr, state);
            RedirectionHelper.RevertJumpTo(_intPtr1, state2);
        }

        public static bool CustomSerialize(Package p, object o, PackageWriter w)
        {
            if (o.GetType() == typeof(PropInfo.Variation))
            {
                PropInfo.Variation varitation = (PropInfo.Variation)o;
                int num = varitation.m_prop.name.LastIndexOf(".");
                string str = num >= 0 ? varitation.m_prop.name.Remove(0, num + 1) : varitation.m_prop.name;
                w.Write(str);
                w.Write(varitation.m_probability);
                return true;
            }
            RedirectionHelper.RevertJumpTo(_intPtr, state);
            try
            {
                return PackageHelper.CustomSerialize(p, o, w);
            }
            finally
            {
                RedirectionHelper.PatchJumpTo(_intPtr, _intPtr2);
            }

        }

        public static object CustomDeserialize(Package p, Type t, PackageReader r)
        {
            if (t == typeof(BuildingInfo.SubInfo))
            {
                var buildingId = r.ReadString();
                var mainBuildingId = $"{p.packageName}.{p.packageMainAsset}_Data";
                if (!subBuildings.ContainsKey(mainBuildingId))
                {
                    subBuildings.Add(mainBuildingId, new List<string>());
                }
                subBuildings[mainBuildingId].Add(buildingId);
                return (object)new BuildingInfo.SubInfo()
                {
                    m_buildingInfo = PrefabCollection<BuildingInfo>.FindLoaded("Metro Entrance"), //a fake sub building to prevent exception
                    m_position = r.ReadVector3(),
                    m_angle = r.ReadSingle(),
                    m_fixedHeight = r.ReadBoolean()
                };
            }
            if (t == typeof(PropInfo.Variation))
            {
                var propId = r.ReadString();
                var mainPropId = $"{p.packageName}.{p.packageMainAsset}_Data";
                if (!propVariations.ContainsKey(mainPropId))
                {
                    propVariations.Add(mainPropId, new List<string>());
                }
                propVariations[mainPropId].Add(propId);
                var stubProp = PrefabCollection<PropInfo>.FindLoaded("Door Marker"); //a fake sub building to prevent exception
                return new PropInfo.Variation()
                {
                    m_prop = stubProp,
                    m_finalProp = stubProp,
                    m_probability = r.ReadInt32()
                };
            }
            RedirectionHelper.RevertJumpTo(_intPtr1, state2);
            try
            {
                return PackageHelper.CustomDeserialize(p, t, r);
            }
            finally
            {
                RedirectionHelper.PatchJumpTo(_intPtr1, _intPtr3);
            }
        }
    }
}