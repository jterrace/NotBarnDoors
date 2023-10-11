using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace NotBarnDoors.DoorPatches
{
    [HarmonyPatch(typeof(Door), nameof(Door.Awake))]
    public static class Awake
    {
        static void Postfix(Door __instance)
        {
            __instance.gameObject.AddComponent<DoorComponent>();
        }
    }

    [HarmonyPatch(typeof(Door), nameof(Door.GetHoverText))]
    public static class GetHoverText
    {

        static void Postfix(Door __instance, ref string __result)
        {
            if (!__result.Contains("[")) return;
            if (__instance.m_keyItem != null) return;

            int doorState = __instance.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0);
            if (doorState == 0)
            {
                __result += "\n[<color=yellow><b>$button_lshift+$KEY_Use</b></color>] Pull";
            }

            int autoCloseTime = __instance.m_nview.GetZDO().GetInt(Main.s_doorAutoCloseTime, 0);
            __result += "\n[<color=yellow><b>$button_lctrl+$KEY_Use</b></color>] Auto Close";
            if (autoCloseTime > 0) __result += " (" + autoCloseTime + "s)";

            __result = Localization.instance.Localize(__result);
        }
    }

    [HarmonyPatch(typeof(Door), nameof(Door.Interact))]
    public static class Interact
    {
        static bool Prefix(Door __instance, bool hold, bool alt)
        {
            if (hold || __instance.m_keyItem != null || __instance.m_canNotBeClosed) return true;

            DoorComponent comp = __instance.gameObject.GetComponent<DoorComponent>();
            if (comp == null) return true;

            if (ZInput.GetKey(KeyCode.LeftControl))
            {
                if (__instance.m_checkGuardStone &&
                    !PrivateArea.CheckAccess(__instance.transform.position, 0f, false, false))
                {
                    return true;
                }
                TextInput.instance.RequestText(comp, "Auto Close Time (seconds):", 4);
                return false;  // skip original
            }

            Main.logger.LogInfo("Door Prefix interact: hold=" + hold + " alt=" + alt);
            if (alt) comp.AltOn();
            return true;
        }

        static void Postfix(Door __instance)
        {
            Main.logger.LogInfo("Door postfix");
            DoorComponent c = __instance.gameObject.GetComponent<DoorComponent>();
            if (c == null) return;
            c.AltOff();
        }
    }

    [HarmonyPatch(typeof(Door), nameof(Door.Open))]
    public static class Open
    {
        static bool Prefix(Door __instance, Vector3 userDir)
        {
            DoorComponent c = __instance.gameObject.GetComponent<DoorComponent>();
            if (c == null) return true;

            bool forward = Vector3.Dot(__instance.transform.forward, userDir) < 0f;
            if (c.Alt) {
                forward = !forward;
            }
            __instance.m_nview.InvokeRPC("UseDoor", new object[] { forward });
            return false; // skip original
        }
    }

    [HarmonyPatch(typeof(Door), nameof(Door.RPC_UseDoor))]
    public static class RPC_UseDoor
    {
        static void Prefix(Door __instance,out int __state)
        {
            Main.logger.LogInfo("RPC_UseDoor");
            __state = __instance.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0);
        }

        static void Postfix(Door __instance, int __state)
        {
            int newState = __instance.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0);
            bool wasOpened = __state == 0 && newState != 0;
            Main.logger.LogInfo("RPC_UseDoor postfix pre state = " + __state + " newstate = " + newState + " wasOpened? " + wasOpened);

            DoorComponent c = __instance.gameObject.GetComponent<DoorComponent>();
            if (c == null) return;

            c.CheckDoorNeedsClosing();
        }
    }
}
