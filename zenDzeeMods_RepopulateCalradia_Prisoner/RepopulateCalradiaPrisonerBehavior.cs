//#define ENABLE_LOGS

using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Localization;
using zenDzeeMods_RepopulateCalradia_Common;

namespace zenDzeeMods_RepopulateCalradia_Prisoner
{
    internal class RepopulateCalradiaPrisonerBehavior : CampaignBehaviorBase
    {
        private static Hero heroToTalk = null;

        public override void SyncData(IDataStore dataStore)
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.CharacterPortraitPopUpOpenedEvent.AddNonSerializedListener(this, OnCharacterPortraitPopUpOpened);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
            CampaignEvents.GameMenuOpened.AddNonSerializedListener(this, OnGameMenuOpened);
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, OnHourlyTickParty);

            CampaignEvents.PrisonerReleased.AddNonSerializedListener(this, OnPrisonerReleased);

#if ENABLE_LOGS
            CampaignEvents.OnChildConceivedEvent.AddNonSerializedListener(this, OnChildConceived);
#endif // ENABLE_LOGS
        }

        private static void OnPrisonerReleased(Hero hero, IFaction arg2, EndCaptivityDetail arg3)
        {
            ZenDzeeRomanceHelper.EndLoverRomances(hero, ZenDzeeRomanceHelper.RomanceLevel_Prisoner);
        }

#if ENABLE_LOGS
        private static void OnChildConceived(Hero mother)
        {
            if (mother.IsPrisoner && mother.StayingInSettlementOfNotable != null)
            {
                Hero captor = ZenDzeeRomanceHelper.GetLover(mother, ZenDzeeRomanceHelper.RomanceLevel_Prisoner);
                if (captor == null)
                {
                    InformationManager.DisplayMessage(new InformationMessage("ERROR: Captor is null: " + mother.Name));
                    return;
                }

                InformationManager.DisplayMessage(new InformationMessage(captor.Name + " impregnates prisoner " + mother.Name));
            }
        }
#endif // ENABLE_LOGS

        private static void OnHourlyTickParty(MobileParty party)
        {
            Settlement settlement = party.CurrentSettlement;
            if (settlement != null
                && !party.IsMainParty
                && party.IsLordParty
                && party.LeaderHero != null
                && party.LeaderHero.Clan != null
                && party.LeaderHero.Clan == settlement.OwnerClan
                )
            {
                float rnd = MBRandom.RandomFloat;
                if (rnd < 0.5)
                {
                    CharacterObject lover = settlement.Party
                        .PrisonerHeroes()
                        .FirstOrDefault(h => h.Age >= ZenDzeeRomanceHelper.LoversAgeMin
                            && h.HeroObject.StayingInSettlementOfNotable == null
                            && h.IsFemale != party.LeaderHero.IsFemale);
                    if (lover != null)
                    {
                        LordHaveFunWIthPrisoner(settlement, party, lover.HeroObject);
                    }
                }
            }
        }

        private static void LordHaveFunWIthPrisoner(Settlement settlement, MobileParty captorParty, Hero prisoner)
        {
            PartyBase settlementParty = settlement.Party;
            if (settlementParty == null) return;

            prisoner.StayingInSettlementOfNotable = settlement;

            if (Romance.GetRomanticLevel(captorParty.LeaderHero, prisoner) <= Romance.RomanceLevelEnum.Untested)
            {
                ChangeRomanticStateAction.Apply(captorParty.LeaderHero, prisoner, (Romance.RomanceLevelEnum)ZenDzeeRomanceHelper.RomanceLevel_Prisoner);
            }

            if (prisoner.IsHumanPlayerCharacter)
            {
                GameMenu.SwitchToMenu("zendzee_settlement_prisoner_wait");
            }

#if ENABLE_LOGS
            InformationManager.DisplayMessage(new InformationMessage(captorParty.LeaderHero.Name + " having fun with the prisoner " + prisoner.Name));
#endif // ENABLE_LOGS
        }

        private static void LordLeavesAlonePrisoners(Settlement settlement, MobileParty captorParty)
        {
            Hero prisoner;
            while ((prisoner = settlement.HeroesWithoutParty.FirstOrDefault(p => PrisonerIsOwnedByHero(p, captorParty.LeaderHero))) != null)
            {
                prisoner.StayingInSettlementOfNotable = null;

                if (prisoner.IsHumanPlayerCharacter)
                {
                    GameMenu.SwitchToMenu("settlement_wait");
                }

#if ENABLE_LOGS
                InformationManager.DisplayMessage(new InformationMessage(captorParty.LeaderHero.Name + " leaves alone prisoner " + prisoner.Name));
#endif // ENABLE_LOGS

                ZenDzeeRomanceHelper.EndLoversRomance(captorParty.LeaderHero, prisoner, ZenDzeeRomanceHelper.RomanceLevel_Prisoner);
            }
        }

        private static bool PrisonerIsOwnedByHero(Hero prisoner, Hero captor)
        {
            if (prisoner.IsPrisoner)
            {
                int level = (int)Romance.GetRomanticLevel(captor, prisoner);

                return level == ZenDzeeRomanceHelper.RomanceLevel_Prisoner
                    || level == ZenDzeeRomanceHelper.RomanceLevel_Lovers;
            }

            return false;
        }

        private static void OnGameMenuOpened(MenuCallbackArgs menuCallbackArgs)
        {
            MenuContext menuContext = menuCallbackArgs.MenuContext;
            GameOverlays.MenuOverlayType menuOverlayType = Campaign.Current.GameMenuManager.GetMenuOverlayType(menuContext);

            if (menuOverlayType == GameOverlays.MenuOverlayType.SettlementWithCharacters)
            {
                Location lordshall = LocationComplex.Current.GetLocationWithId("lordshall");
                if (lordshall == null) return;

                Settlement settlement = Hero.MainHero.CurrentSettlement;
                if (settlement == null) return;

                IEnumerable<Hero> list = settlement.HeroesWithoutParty.Where(h => h.IsPrisoner);
                if (list == null) return;

                foreach (Hero hero in list)
                {
                    Location current = LocationComplex.Current.GetLocationOfCharacter(hero);
                    if (current == lordshall) continue;

                    if (current == null)
                    {
                        Tuple<string, Monster> actionSetAndMonster = new Tuple<string, Monster>("as_human_lady", Campaign.Current.HumanMonsterSettlement);
                        AgentData agentData = new AgentData(
                            new SimpleAgentOrigin(
                                hero.CharacterObject,
                                -1,
                                null,
                                default(UniqueTroopDescriptor))).Monster(actionSetAndMonster.Item2).NoHorses(true);
                        lordshall.AddCharacter(
                            new LocationCharacter(agentData,
                                new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors),
                                null,
                                true,
                                LocationCharacter.CharacterRelations.Neutral,
                                actionSetAndMonster.Item1,
                                true,
                                true,
                                null,
                                false,
                                false,
                                true));
                    }
                    else
                    {
                        LocationComplex.Current.ChangeLocation(LocationComplex.Current.GetLocationCharacterOfHero(hero),
                            current, lordshall);
                    }
                }
            }
        }

        /*
         * When castle is captures and enemy nobles was there,
         * they may stay as HeroesWithoutParty and Prisoners.
         * This function supposed to fix this issue.
         */
        private static void OnSettlementEntered(MobileParty party, Settlement settlement, Hero notused)
        {
            if (party == null || settlement == null || party.LeaderHero == null || !party.IsMainParty) return;

            Hero hero;
            PartyBase settlementParty = settlement.Party;
            while ((hero = settlement.HeroesWithoutParty.FirstOrDefault(h => h.IsPrisoner && h.PartyBelongedToAsPrisoner == settlementParty)) != null)
            {
                hero.StayingInSettlementOfNotable = null;
            }
        }

        /*
         * When the lord leaves the settlement, return to dungeon all the prisoners
         * whom he borrowed for entertainment.
         */
        private static void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (party == null || settlement == null || party.LeaderHero == null) return;

            LordLeavesAlonePrisoners(settlement, party);
        }

        private static void OnCharacterPortraitPopUpOpened(CharacterObject characterObject)
        {
            Location locationOfCharacter = LocationComplex.Current.GetLocationOfCharacter(characterObject.HeroObject);

            if (!characterObject.HeroObject.IsPrisoner) return;

            heroToTalk = characterObject.HeroObject;

            MenuContext menuContext = Campaign.Current.CurrentMenuContext;
            if (menuContext == null)
            {
                return;
            }

            GameOverlays.MenuOverlayType menuOverlayType = Campaign.Current.GameMenuManager.GetMenuOverlayType(menuContext);

            if (menuOverlayType == GameOverlays.MenuOverlayType.SettlementWithCharacters)
            {
                // get menu overlay layer, see GauntletMenuOverlayBase;
                ScreenBase topScreen = ScreenManager.TopScreen;
                if (topScreen == null)
                {
                    return;
                }
                GauntletLayer gauntletLayer = topScreen.FindLayer<ScreenLayer>("BasicLayer") as GauntletLayer;
                if (gauntletLayer == null)
                {
                    return;
                }

                // get _moviesAndDatasources, its public
                // and there will be ViewModel for SettlementMenuOverlayVM;
                var movieAndDatasource = gauntletLayer._moviesAndDatasources.FirstOrDefault(p => p.Item2 is SettlementMenuOverlayVM);
                if (movieAndDatasource == null)
                {
                    return;
                }
                SettlementMenuOverlayVM dataSource = movieAndDatasource.Item2 as SettlementMenuOverlayVM;

                // it has prop ContextList;
                // you should add new items there, example ExecuteOnSetAsActiveContextMenuItem;
                GameMenuOverlayActionVM overlayItem = new GameMenuOverlayActionVM(
                    OnTalkToPrisonerAction,
                    GameTexts.FindText("str_menu_overlay_context_list", "Conversation").ToString(),
                    true,
                    1, // ID for Conversation
                    "");
                dataSource.ContextList.Add(overlayItem);
            }
        }

        private static void OnTalkToPrisonerAction(object id)
        {
            if((int)id == 1)
            {
                Location locationOfCharacter = LocationComplex.Current.GetLocationOfCharacter(heroToTalk);
                CampaignEventDispatcher.Instance.OnPlayerStartTalkFromMenu(heroToTalk);
                PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(locationOfCharacter, null, heroToTalk.CharacterObject, null);

                heroToTalk = null;
            }
        }

        private static void OnSessionLaunched(CampaignGameStarter campaignStarter)
        {
            campaignStarter.AddDialogLine("start_wanderer_unmet", "start", "zendzee_repopulate_prisoner0",
                "{=BaeqKlQ6}I am not allowed to talk with you.[rb:very_negative]",
                ConditionTalkToPrisonerInCell,
                null,
                120, null);
            campaignStarter.AddDialogLine("start_wanderer_unmet", "lord_introduction", "zendzee_repopulate_prisoner0",
                "{=BaeqKlQ6}I am not allowed to talk with you.[rb:very_negative]",
                ConditionTalkToPrisonerInCell,
                null,
                120, null);

            campaignStarter.AddPlayerLine("zendzee_repopulate_prisoner0_0", "zendzee_repopulate_prisoner0", "close_window",
                "{=zee427439A1}Go to my chambers.",
                null,
                ConsequenceGoToChambers,
                100, null);
            campaignStarter.AddPlayerLine("zendzee_repopulate_prisoner0_1", "zendzee_repopulate_prisoner0", "close_window",
                "{=mdNRYlfS}Nevermind.",
                null,
                null,
                100, null);

            campaignStarter.AddWaitGameMenu("zendzee_settlement_prisoner_wait",
                "{CAPTIVITY_TEXT}\nWaiting in captivity!!!",
                new OnInitDelegate(settlement_wait_on_init),
                new OnConditionDelegate(args => true),
                null,
                new OnTickDelegate(wait_menu_settlement_wait_on_tick),
                GameMenu.MenuAndOptionType.WaitMenuHideProgressAndHoursOption,
                GameOverlays.MenuOverlayType.None,
                0f,
                GameMenu.MenuFlags.none,
                null);
        }

        private static void ConsequenceGoToChambers()
        {
            if (Hero.OneToOneConversationHero == null
                || Hero.OneToOneConversationHero.CurrentSettlement == null)
            {
                return;
            }

            LordHaveFunWIthPrisoner(Hero.OneToOneConversationHero.CurrentSettlement, MobileParty.MainParty, Hero.OneToOneConversationHero);
        }

        private static bool ConditionTalkToPrisonerInCell()
        {
            if (Hero.OneToOneConversationHero == null
                || Hero.OneToOneConversationHero.CurrentSettlement == null)
            {
                return false;
            }

            return Hero.OneToOneConversationHero.IsPrisoner
                && Hero.OneToOneConversationHero.StayingInSettlementOfNotable == null;
        }


        private static void settlement_wait_on_init(MenuCallbackArgs args)
        {
            args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            TextObject text = args.MenuContext.GameMenu.GetText();

            TextObject variable = null;
            if (Hero.MainHero.IsPrisoner && Hero.MainHero.PartyBelongedToAsPrisoner.Settlement != null)
            {
                variable = Hero.MainHero.PartyBelongedToAsPrisoner.Settlement.Name;
            }

            if (variable == null)
            {
                variable = Settlement.CurrentSettlement.Name;
            }

            int captiveTimeInDays = PlayerCaptivity.CaptiveTimeInDays;
            TextObject textObject;
            if (captiveTimeInDays == 0)
            {
                textObject = GameTexts.FindText("str_prisoner_of_settlement_menu_text", null);
            }
            else
            {
                textObject = GameTexts.FindText("str_prisoner_of_settlement_for_days_menu_text", null);
                textObject.SetTextVariable("NUMBER_OF_DAYS", captiveTimeInDays);
                textObject.SetTextVariable("PLURAL", (captiveTimeInDays > 1) ? 1 : 0);
            }
            textObject.SetTextVariable("SETTLEMENT_NAME", variable);
            text.SetTextVariable("CAPTIVITY_TEXT", textObject);
        }

        public static void wait_menu_settlement_wait_on_tick(MenuCallbackArgs args, CampaignTime dt)
        {
            int captiveTimeInDays = PlayerCaptivity.CaptiveTimeInDays;
            if (captiveTimeInDays == 0)
            {
                return;
            }
            
            TextObject variable = null;
            if (Hero.MainHero.IsPrisoner && Hero.MainHero.PartyBelongedToAsPrisoner.Settlement != null)
            {
                variable = Hero.MainHero.PartyBelongedToAsPrisoner.Settlement.Name;
            }

            if (variable == null)
            {
                variable = Settlement.CurrentSettlement.Name;
            }

            TextObject text = args.MenuContext.GameMenu.GetText();
            TextObject textObject = GameTexts.FindText("str_prisoner_of_settlement_for_days_menu_text", null);
            textObject.SetTextVariable("NUMBER_OF_DAYS", captiveTimeInDays);
            textObject.SetTextVariable("PLURAL", (captiveTimeInDays > 1) ? 1 : 0);
            textObject.SetTextVariable("SETTLEMENT_NAME", variable);
            text.SetTextVariable("CAPTIVITY_TEXT", textObject);
        }
    }
}