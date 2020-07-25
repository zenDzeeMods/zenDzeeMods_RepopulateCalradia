//#define ENABLE_TESTS
//#define ENABLE_TEST_PREGNANCY

using HarmonyLib;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace zenDzeeMods_RepopulateCalradia_Common
{
    public class SubModule : MBSubModuleBase
    {
        private static Harmony harmony = null;

        protected override void OnSubModuleLoad()
        {
            harmony = new Harmony("zenDzeeMods_RepopulateCalradia_Common");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        protected override void OnSubModuleUnloaded()
        {
            if (harmony != null)
            {
                harmony.UnpatchAll("zenDzeeMods_RepopulateCalradia_Common");
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarter;
                campaignStarter.AddBehavior(new FixBugsOfRepopulateCalradia());
                campaignStarter.AddBehavior(new AgingBehavior());

#if ENABLE_TESTS
#if ENABLE_TEST_PREGNANCY
                campaignStarter.AddModel(new TestPregnancyModel());
#endif // ENABLE_TEST_PREGNANCY
#endif // ENABLE_TESTS
            }
        }
    } // SubModule

#if ENABLE_TESTS

#if ENABLE_TEST_PREGNANCY
    internal class TestPregnancyModel : DefaultPregnancyModel
    {
        public override float PregnancyDurationInDays => 2f;
        public override float GetDailyChanceOfPregnancyForHero(Hero hero)
        {
            if (hero.IsHumanPlayerCharacter)
            {
                return 1f;
            }

            return base.GetDailyChanceOfPregnancyForHero(hero);
        }
    }
#endif // ENABLE_TEST_PREGNANCY

#endif // ENABLE_TESTS
}
