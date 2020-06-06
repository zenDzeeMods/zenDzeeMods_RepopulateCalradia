using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace zenDzeeMods_RepopulateCalradia_Common.Patches
{
    [HarmonyPatch(typeof(Hero))]
    internal class Hero_Patch
    {
        [HarmonyPatch("Spouse", MethodType.Getter)]
        [HarmonyPriority(Priority.VeryLow)]
        public static void Postfix(Hero __instance, ref Hero __result)
        {
            if (ZenDzeeRomanceHelper.fakeSpouse1 != null && __instance == ZenDzeeRomanceHelper.fakeSpouse1)
            {
                __result = ZenDzeeRomanceHelper.fakeSpouse2;
            }
            else if (ZenDzeeRomanceHelper.fakeSpouse2 != null && __instance == ZenDzeeRomanceHelper.fakeSpouse2)
            {
                __result = ZenDzeeRomanceHelper.fakeSpouse1;
            }
        }
    }
}
