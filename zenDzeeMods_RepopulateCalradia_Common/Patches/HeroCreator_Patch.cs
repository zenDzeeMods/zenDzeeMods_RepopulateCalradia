using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace zenDzeeMods_RepopulateCalradia_Common.Patches
{
    [HarmonyPatch(typeof(HeroCreator))]
    internal class HeroCreator_Patch
    {

        [HarmonyPrefix]
        [HarmonyPatch("DeliverOffSpring", MethodType.Normal)]
        [HarmonyPriority(Priority.VeryLow)]
        public static void Prefix_DeliverOffSpring(ref Hero mother, ref Hero father, ref bool isOffspringFemale)
        {
            if (isOffspringFemale)
            {
                if (mother.IsWanderer || mother.IsNotable)
                {
                    ZenDzeeRomanceHelper.FakeNoble(mother);
                }
            }
            else
            {
                if (father.IsWanderer || father.IsNotable)
                {
                    ZenDzeeRomanceHelper.FakeNoble(father);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("DeliverOffSpring", MethodType.Normal)]
        [HarmonyPriority(Priority.VeryLow)]
        public static void Postfix_DeliverOffSpring()
        {
            ZenDzeeRomanceHelper.ClearFakes();
        }
    }
}
