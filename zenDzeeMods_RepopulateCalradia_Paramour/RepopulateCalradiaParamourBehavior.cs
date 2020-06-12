//#define ENABLE_LOGS

using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using zenDzeeMods_RepopulateCalradia_Common;

namespace zenDzeeMods_RepopulateCalradia_Paramour
{
    internal class RepopulateCalradiaParamourBehavior : CampaignBehaviorBase
    {
        private static float DesperateToFindSpouseAge = ZenDzeeRomanceHelper.LoversAgeMin + 10f;

        public override void SyncData(IDataStore dataStore)
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTickHero);
            CampaignEvents.RomanticStateChanged.AddNonSerializedListener(this, OnRomanticStateChanged);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
        }

        private static void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail actionDetail, bool showNotification)
        {
            ZenDzeeRomanceHelper.EndLoverRomances(victim);
        }

        private static void OnRomanticStateChanged(Hero hero1, Hero hero2, Romance.RomanceLevelEnum level)
        {
            if ((int)level == ZenDzeeRomanceHelper.RomanceLevel_Lovers)
            {
#if ENABLE_LOGS
                InformationManager.DisplayMessage(new InformationMessage(hero1.Name + " and " + hero2.Name + " started romance as lovers."));
#endif
            }
            else if (level == Romance.RomanceLevelEnum.Marriage)
            {
                ZenDzeeRomanceHelper.EndLoverRomances(hero1);
                ZenDzeeRomanceHelper.EndLoverRomances(hero2);
            }
        }

        private static void OnDailyTickHero(Hero hero)
        {
            if (!hero.IsTemplate
                    && hero.IsNoble && hero.IsAlive && hero.IsActive
                    && hero.Clan != null
                    && hero.Clan != Clan.PlayerClan
                    && hero.Age >= ZenDzeeRomanceHelper.LoversAgeMin
                    && (!hero.IsFemale || hero.Age < ZenDzeeRomanceHelper.PregnancyAgeMax)
                    && (hero.Spouse == null
                            || hero.Spouse.IsDead
                            || (hero.Spouse.IsFemale && hero.Spouse.Age > ZenDzeeRomanceHelper.PregnancyAgeMax))
                    && hero.Clan.Heroes.Count() < 6
                    && hero.Children.Count(c => c.IsAlive && c.Clan == hero.Clan) == 0)
            {
                // Next day after birthday.
                if ((int)(hero.BirthDay.ElapsedDaysUntilNow - 1) % CampaignTime.DaysInYear == 0)
                {
                    Hero lover = ZenDzeeRomanceHelper.GetLover(hero);
                    if (lover == null)
                    {
                        // Find a lover. TODO maybe this should had random chance?
                        IEnumerable<Hero> list = hero.Clan.SupporterNotables;
                        if (list != null)
                        {
                            lover = GetSuitableLoverForHero(hero, list);
                            if (lover != null)
                            {
                                ChangeRomanticStateAction.Apply(hero, lover, (Romance.RomanceLevelEnum)ZenDzeeRomanceHelper.RomanceLevel_Lovers);
                                return;
                            }
                        }

                        if (hero.Age >= DesperateToFindSpouseAge && lover == null)
                        {
                            list = Hero.All.Where(h => h.IsWanderer
                                && h.CurrentSettlement != null
                                && h.CurrentSettlement.MapFaction == hero.MapFaction
                                && h.Clan != Clan.PlayerClan
                                && IsHeroSuitableLoverForHero(hero, h));

                            if (list != null)
                            {
                                lover = GetSuitableLoverForHero(hero, list);
                                if (lover != null)
                                {
                                    ChangeRomanticStateAction.Apply(hero, lover, (Romance.RomanceLevelEnum)ZenDzeeRomanceHelper.RomanceLevel_Lovers);
                                    return;
                                }
                            }
                        }

                        // Add here other romance options
                    }
                }
            }
        }

        private static bool IsHeroSuitableLoverForHero(Hero hero, Hero lover)
        {
            return lover.IsFemale != hero.IsFemale
                    && !lover.IsTemplate
                    && lover.IsActive
                    && lover.IsAlive
                    && lover.Spouse == null
                    && lover.Age >= ZenDzeeRomanceHelper.LoversAgeMin
                    && (!lover.IsFemale || lover.Age < ZenDzeeRomanceHelper.PregnancyAgeMax)
                    && ZenDzeeRomanceHelper.GetLover(lover) == null;
        }

        private static Hero GetSuitableLoverForHero(Hero hero, IEnumerable<Hero> list)
        {
            return list.FirstOrDefault(h => IsHeroSuitableLoverForHero(hero, h));
        }
    }
}