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
            if (hero.IsNoble && (hero.IsNotable || hero.IsWanderer))
            {
                PropertyInfo occupationInfo = typeof(CharacterObject).GetProperty("Occupation");
                if (occupationInfo == null) return;
                occupationInfo.SetValue(hero.CharacterObject, Occupation.Lord);

                FieldInfo _originCharacterInfo = typeof(CharacterObject).GetField("_originCharacter", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_originCharacterInfo != null)
                {
                    CharacterObject origin = null;
                    if (hero.Mother.IsNoble)
                    {
                        origin = hero.Mother.CharacterObject;
                    }
                    else if (hero.Father.IsNoble)
                    {
                        origin = hero.Father.CharacterObject;
                    }

                    _originCharacterInfo.SetValue(hero.CharacterObject, origin);
                }
            }
        }

        private void FixAdoptBastard(Hero hero)
        {
            if (!hero.IsNoble && !hero.Mother.IsNoble && hero.Father.IsNoble)
            {
                hero.IsNoble = true;
                hero.Clan = hero.Father.Clan;
            }
            else if (!hero.IsNoble && hero.Mother.IsNoble && !hero.Father.IsNoble)
            {
                hero.IsNoble = true;
                hero.Clan = hero.Mother.Clan;
            }
        }
    } // FixBugsOfRepopulateCalradia
}
