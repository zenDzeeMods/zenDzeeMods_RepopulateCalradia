using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace zenDzeeMods_RepopulateCalradia_Common.Patches
{
    /*
     * Protect notables
     */
    [HarmonyPatch(typeof(KillCharacterAction))]
    class KillCharacterAction_Patch
    {
        [HarmonyPatch("ApplyInLabor", MethodType.Normal)]
        public static bool Prefix(ref Hero lostMother)
        {
            return !lostMother.IsNotable;
        }
    }
}
