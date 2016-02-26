using System;
using System.Reflection;
using ColossalFramework.Packaging;
using UnityEngine;

namespace LoadingExtension
{
    public class PackageHelperDetour
    {
        private static RedirectCallsState state;
        private static IntPtr _intPtr;
        private static IntPtr _intPtr2;

        private static RedirectCallsState state2;
        private static IntPtr _intPtr1;
        private static IntPtr _intPtr3;

        public static void Init()
        {
            _intPtr = typeof (PackageHelper).GetMethod("CustomSerialize",
                BindingFlags.Static | BindingFlags.Public).MethodHandle.GetFunctionPointer();
            _intPtr2 = typeof (PackageHelperDetour).GetMethod("CustomSerialize",
                BindingFlags.Static | BindingFlags.Public).MethodHandle.GetFunctionPointer();
            state = RedirectionHelper.PatchJumpTo(
                _intPtr,
                _intPtr2
                );
            _intPtr1 = typeof (PackageHelper).GetMethod("CustomDeserialize",
                BindingFlags.Static | BindingFlags.Public).MethodHandle.GetFunctionPointer();
            _intPtr3 = typeof (PackageHelperDetour).GetMethod("CustomDeserialize",
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
            if (o.GetType() == typeof (BuildingInfo.SubInfo))
            {
                BuildingInfo.SubInfo subInfo = (BuildingInfo.SubInfo) o;
                int num = subInfo.m_buildingInfo.name.LastIndexOf(".");
                string str = num >= 0 ? subInfo.m_buildingInfo.name.Remove(0, num + 1) : subInfo.m_buildingInfo.name;
                w.Write(str);
                w.Write(subInfo.m_position);
                w.Write(subInfo.m_angle);
                w.Write(subInfo.m_fixedHeight);
                return true;
            }
            if (o.GetType() == typeof (DepotAI.SpawnPoint))
            {
                DepotAI.SpawnPoint spawnPoint = (DepotAI.SpawnPoint) o;
                w.Write(spawnPoint.m_position);
                w.Write(spawnPoint.m_target);
                return true;
            }
            if (o.GetType() == typeof(PropInfo.Effect))
            {
                PropInfo.Effect effect = (PropInfo.Effect)o;
                int num = effect.m_effect.name.LastIndexOf(".");
                string str = num >= 0 ? effect.m_effect.name.Remove(0, num + 1) : effect.m_effect.name;
                w.Write(str);
                w.Write(effect.m_position);
                w.Write(effect.m_direction);
                return true;
            }
            RedirectionHelper.RevertJumpTo(_intPtr, state);
            var result = PackageHelper.CustomSerialize(p, o, w);
            RedirectionHelper.PatchJumpTo(_intPtr, _intPtr2);
            return result;
        }

        public static object CustomDeserialize(Package p, Type t, PackageReader r)
        {
            if (t == typeof (BuildingInfo.SubInfo))
            {
                var subInfo = new BuildingInfo.SubInfo
                {
                    m_buildingInfo = PrefabCollection<BuildingInfo>.FindLoaded(r.ReadString()),
                    m_position = r.ReadVector3(),
                    m_angle = r.ReadSingle(),
                    m_fixedHeight = r.ReadBoolean()
                };
                return subInfo;
            }
            if (t == typeof (DepotAI.SpawnPoint))
            {
                var spawnPoint = new DepotAI.SpawnPoint()
                {
                    m_position = r.ReadVector3(),
                    m_target = r.ReadVector3()
                };
                if (spawnPoint.m_target.magnitude > 700.0f || spawnPoint.m_position.magnitude > 700.0f)
                {
                    spawnPoint = new DepotAI.SpawnPoint()
                    {
                        m_position = Vector3.zero,
                        m_target = Vector3.zero
                    };
                }
                return spawnPoint;
            }
            if (t == typeof(PropInfo.Effect))
            {
                return new PropInfo.Effect
                {
                    m_effect = EffectCollection.FindEffect(r.ReadString()),
                    m_position = r.ReadVector3(),
                    m_direction = r.ReadVector3()
                };
            }
            RedirectionHelper.RevertJumpTo(_intPtr1, state2);
            var result = PackageHelper.CustomDeserialize(p, t, r);
            RedirectionHelper.PatchJumpTo(_intPtr1, _intPtr3);
            return result;
        }
    }
}