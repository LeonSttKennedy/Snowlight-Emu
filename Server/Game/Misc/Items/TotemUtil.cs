using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Misc
{
    public enum TotemType
    {
        None = 0,
        Troll = 1,
        Octopus = 2,
        Bird = 3
    }

    public enum TotemColor
    {
        None = 0,
        Red = 1,
        Yellow = 2,
        Blue = 3
    }

    public enum TotemPlanet
    {
        Moon = 0,
        Sun = 1,
        Earth = 2
    }

    public static class TotemUtil
    {
        public static TotemType GetLegTypeFromInt(int ItemFlags)
        {
            int Number = (int)Math.Ceiling((ItemFlags + 1) / 4.0f);

            return (TotemType)Number;
        }

        public static TotemColor GetLegColorFromInt(int ItemFlags)
        {
            int Number = ItemFlags - (4 * ((int)GetLegTypeFromInt(ItemFlags) - 1));

            return (TotemColor)Number;
        }

        public static TotemType GetHeadTypeFromInt(int ItemFlags)
        {
            int Number = ItemFlags < 3 ? ItemFlags + 1 : (int)Math.Ceiling((ItemFlags - 2) / 4.0f);

            return (TotemType)Number;
        }

        public static TotemColor GetHeadColorFromInt(int ItemFlags)
        {
            int Number = ItemFlags < 3 ? 0 : ItemFlags - 3 - (4 * ((int)GetHeadTypeFromInt(ItemFlags) - 1));

            return (TotemColor)Number;
        }

        public static TotemPlanet GetPlanetFromInt(int ItemFlags)
        {
            return (TotemPlanet)ItemFlags;
        }

        public static int GetEffectFromCombination(TotemPlanet Planet, TotemType HeadType, TotemType LegType, TotemColor Color)
        {
            int EffectId = 0;

            if (Planet.Equals(TotemPlanet.Earth) && HeadType.Equals(TotemType.Troll) && LegType.Equals(TotemType.Troll) && Color.Equals(TotemColor.Yellow))
            {
                EffectId = 23;
            }
            else if (Planet.Equals(TotemPlanet.Moon) && HeadType.Equals(TotemType.Octopus) && LegType.Equals(TotemType.Octopus) && Color.Equals(TotemColor.Blue))
            {
                EffectId = 24;
            }
            else if (Planet.Equals(TotemPlanet.Sun) && HeadType.Equals(TotemType.Bird) && LegType.Equals(TotemType.Bird) && Color.Equals(TotemColor.Red))
            {
                EffectId = 25;
            }
            else if (Planet.Equals(TotemPlanet.Earth) && HeadType.Equals(TotemType.Octopus) && LegType.Equals(TotemType.Bird) && Color.Equals(TotemColor.Yellow))
            {
                EffectId = 26;
            }
            
            return EffectId;
        }
    }
}
