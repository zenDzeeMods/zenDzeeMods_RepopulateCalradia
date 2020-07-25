using System;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace zenDzeeMods_RepopulateCalradia_Common
{
    public class ZenDzeeRomanceHelper
    {
        public static float LoversAgeMin { get => 30f; }
        public static float PregnancyAgeMax { get => 45f; }

        public const int RomanceLevel_Lovers = 69;
        public const int RomanceLevel_Prisoner = -69;

        public static Hero GetLover(Hero hero, int level = RomanceLevel_Lovers)
        {
            return GetRomanticLover(hero, Romance.RomanticStateList.FirstOrDefault(rs => GetRomanticLover(hero, rs, level) != null), level);
        }

        private static Hero GetRomanticLover(Hero hero, Romance.RomanticState rs, int level = RomanceLevel_Lovers)
        {
            if (hero == null || rs == null) return null;

            if (rs.Person1 == hero && (int)rs.Level == level) return rs.Person2;
            if (rs.Person2 == hero && (int)rs.Level == level) return rs.Person1;

            return null;
        }

        public static void EndLoversRomance(Hero hero1, Hero hero2, int level = RomanceLevel_Lovers)
        {
            if (hero1 == null || hero2 == null)
            {
                return;
            }

            Romance.RomanticState state = Romance.GetRomanticState(hero1, hero2);
            if (state != null && (int)state.Level == level)
            {
                state.Level = Romance.RomanceLevelEnum.Ended;
            }
        }

        public static void EndLoverRomances(Hero hero, int level = RomanceLevel_Lovers)
        {
            if (hero == null)
            {
                return;
            }

            Romance.RomanticState state;
            while ((state = Romance.RomanticStateList.FirstOrDefault(rs => (rs.Person1 == hero || rs.Person2 == hero)
                && (int)rs.Level == level)) != null)
            {
                state.Level = Romance.RomanceLevelEnum.Ended;
            }
        }


        internal static Hero fakeSpouse1 = null;
        internal static Hero fakeSpouse2 = null;
        internal static Hero fakeNobleOccupation = null;

        public static void FakeSpouses(Hero hero1, Hero hero2)
        {
            fakeSpouse1 = hero1;
            fakeSpouse2 = hero2;
        }

        public static void ClearFakes()
        {
            fakeSpouse1 = null;
            fakeSpouse2 = null;
            fakeNobleOccupation = null;
        }

        public static void FakeNoble(Hero hero)
        {
            fakeNobleOccupation = hero;
        }
    }
}
