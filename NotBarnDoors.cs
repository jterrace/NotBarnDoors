using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace NotBarnDoors
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        const string pluginGUID = "com.roggo";
        const string pluginName = "NotBarnDoors";
        const string pluginVersion = "1.0.0";

        public static ManualLogSource logger;
        public static readonly int s_doorAutoCloseTime = "doorAutoCloseTime".GetStableHashCode();

        private Harmony harmony;

        public Main()
        {
            harmony = new Harmony(pluginGUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void Awake()
        {
            logger = Logger;
            logger.LogInfo("NotBarnDoors loading...");
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
        }

    }
}
