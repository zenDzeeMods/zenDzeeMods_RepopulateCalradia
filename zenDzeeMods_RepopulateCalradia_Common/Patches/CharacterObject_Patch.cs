using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace zenDzeeMods_RepopulateCalradia_Common.Patches
{
    [HarmonyPatch(typeof(CharacterObject))]
    internal class CharacterObject_Patch
    {

        [HarmonyPostfix]
        [HarmonyPatch("Occupation", MethodType.Getter)]
        [HarmonyPriority(Priority.VeryLow)]
        public static void Postfix_Occupation(CharacterObject __instance, ref Occupation __result)
        {
            if (ZenDzeeRomanceHelper.fakeNobleOccupation != null && __instance == ZenDzeeRomanceHelper.fakeNobleOccupation.CharacterObject)
            {
                __result = Occupation.Lord;
            }
        }
    }
}
