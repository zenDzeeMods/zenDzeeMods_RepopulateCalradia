using HarmonyLib;
using System.Reflection;
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
        public static void Postfix_DeliverOffSpring(ref Hero __result)
        {
            ZenDzeeRomanceHelper.ClearFakes();

            if (__result.Mother.IsNoble && __result.Father.IsNoble)
            {
                return;
            }

            FieldInfo _originCharacterInfo = typeof(CharacterObject).GetField("_originCharacter", BindingFlags.NonPublic | BindingFlags.Instance);
            if (_originCharacterInfo != null)
            {
                CharacterObject origin = null;
                if (__result.Mother.IsNoble)
                {
                    origin = __result.Mother.CharacterObject;
                }
                else if (__result.Father.IsNoble)
                {
                    origin = __result.Father.CharacterObject;
                }
                if (origin != null)
                {
                    _originCharacterInfo.SetValue(__result.CharacterObject, origin);
                }
            }
        }
    }
}
