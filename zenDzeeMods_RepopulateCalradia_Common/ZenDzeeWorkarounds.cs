using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace zenDzeeMods_RepopulateCalradia_Common
{
    public class ZenDzeeWorkarounds
    {
        public static void PrepareToKillHero(Hero hero)
        {
            RemoveWorkshops(hero);
        }

        private static void RemoveWorkshops(Hero hero)
        {
            List<Workshop> workshops = new List<Workshop>(hero.OwnedWorkshops);
            foreach (Workshop workshop in workshops)
            {
                Hero nextOwner = Campaign.Current.Models.WorkshopModel.SelectNextOwnerForWorkshop(workshop.Settlement.Town, workshop, workshop.Owner, 0);
                if (nextOwner != null)
                {
                    ChangeOwnerOfWorkshopAction.ApplyByDeath(workshop, nextOwner);
                }
            }
        }
    }
}
