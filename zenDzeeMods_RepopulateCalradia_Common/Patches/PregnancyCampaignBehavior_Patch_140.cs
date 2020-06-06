using HarmonyLib;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace zenDzeeMods_RepopulateCalradia_Common.Patches
{
    [HarmonyPatch(typeof(PregnancyCampaignBehavior))]
    internal class PregnancyCampaignBehavior_Patch_140
    {

        [HarmonyPatch("DailyTickHero", MethodType.Normal)]
        [HarmonyPriority(Priority.VeryHigh)]
        public static void Prefix(ref Hero hero)
        {
            if (!hero.IsFemale || hero.IsDead || hero.Age < 18f || hero.IsPregnant)
            {
                return;
            }

            Settlement settlement = hero.CurrentSettlement;

            if (hero.IsHumanPlayerCharacter && !hero.IsPrisoner && hero.Spouse != null && hero.Spouse.CurrentSettlement == settlement)
            {
                return;
            }

            Hero lover;
            if (settlement != null)
            {
                lover = ZenDzeeRomanceHelper.GetLover(hero, ZenDzeeRomanceHelper.RomanceLevel_Prisoner);
                if (lover != null)
                {
                    ZenDzeeRomanceHelper.FakeSpouses(hero, lover);
                    return;
                }
            }

            lover = ZenDzeeRomanceHelper.GetLover(hero, ZenDzeeRomanceHelper.RomanceLevel_Lovers);
            if (lover != null)
            {
                if (lover.IsDead)
                {
                    ZenDzeeRomanceHelper.EndLoverRomances(lover);
                    return;
                }
                if (!lover.IsPrisoner)
                {
                    ZenDzeeRomanceHelper.FakeSpouses(hero, lover);
                    return;
                }
            }
        }


        [HarmonyPatch("DailyTickHero", MethodType.Normal)]
        [HarmonyPriority(Priority.VeryLow)]
        public static void Postfix()
        {
            ZenDzeeRomanceHelper.ClearFakes();
        }
    }
}
