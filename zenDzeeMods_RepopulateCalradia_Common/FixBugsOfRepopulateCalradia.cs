using System.Reflection;
using TaleWorlds.CampaignSystem;

namespace zenDzeeMods_RepopulateCalradia_Common
{
    /**
     * Fix bugs caused by previous versions of RepopulateCalradia
     */
    internal class FixBugsOfRepopulateCalradia : CampaignBehaviorBase
    {
        public override void SyncData(IDataStore dataStore)
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
        }

        private void OnHourlyTick()
        {
            foreach(Hero hero in Hero.All)
            {
                if (!hero.IsTemplate && hero.IsAlive
                     && hero.Mother != null && hero.Father != null
                     && !hero.CharacterObject.IsOriginalCharacter)
                {
                    FixAdoptBastard(hero);
                    FixBastardOccupation(hero);
                }
            }

            CampaignEvents.RemoveListeners(this);
        }

        private void FixBastardOccupation(Hero hero)
        {
            if (hero.IsNoble && hero.IsNotable)
            {
                PropertyInfo occupationInfo = typeof(CharacterObject).GetProperty("Occupation");
                if (occupationInfo == null) return;

                occupationInfo.SetValue(hero.CharacterObject, Occupation.Lord);
            }
        }

        private void FixAdoptBastard(Hero hero)
        {
            if (!hero.Mother.IsNoble && hero.Father.IsNoble)
            {
                hero.IsNoble = true;
                hero.Clan = hero.Father.Clan;
            }
            else if (hero.Mother.IsNoble && !hero.Father.IsNoble)
            {
                hero.IsNoble = true;
                hero.Clan = hero.Mother.Clan;
            }
        }
    } // FixBugsOfRepopulateCalradia
}