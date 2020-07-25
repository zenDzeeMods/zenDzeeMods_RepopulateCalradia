//#define ENABLE_TESTS
//#define ENABLE_TEST_PREGNANCY

using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace zenDzeeMods_RepopulateCalradia_Common
{
    internal class AgingBehavior : CampaignBehaviorBase
    {
        public override void SyncData(IDataStore dataStore)
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTickHero);
        }

        private void OnDailyTickHero(Hero hero)
        {
            if (hero != Hero.MainHero
                && hero.IsAlive
                && (hero.IsNoble || hero.IsWanderer)
                && !hero.IsNotable
                && hero.Age > (float)Campaign.Current.Models.AgeModel.BecomeOldAge)
            {
                float mult = 1f;

                MobileParty party = hero.PartyBelongedTo;
                if (party != null)
                {
                    if (party.Army != null)
                    {
                        mult *= 0.5f;
                    }

                    if (party.MapEvent != null)
                    {
                        mult *= 0.5f;
                    }
                }

                if (hero.Clan != null
                    && hero.Clan.Heroes.Count(h => h.IsAlive) < 3)
                {
                    mult *= 0.5f;
                }

                if (MBRandom.RandomFloat < hero.ProbabilityOfDeath * mult)
                {
                    ZenDzeeWorkarounds.PrepareToKillHero(hero);
                    KillCharacterAction.ApplyByOldAge(hero, true);
                }
            }
        }
    }
}
