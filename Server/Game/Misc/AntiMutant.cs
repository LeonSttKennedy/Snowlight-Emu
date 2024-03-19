using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Misc
{
    public class AntiMutant
    {
        public static bool ValidateLook(string UserLook, string UserGender)
        {
            /*
             * strings for comparation:
             * 1: ha-1011-62.cc-887-1336.hd-180-1.ch-887-110.hr-678-61.sh-906-105.lg-285-105.ca-1816-62
             * 2: ha-3129-100.cc-3039-100.hd-3092-1.hr-3194-40-31.sh-290-62.lg-270-110.fa-1206-62
            */

            bool HasHead = false;

            if (UserLook.Length < 1)
            {
                return false;
            }

            string[] FigureSets = UserLook.Split('.');

            if (FigureSets.Length < 4)
            {
                return false;
            }

            foreach (string Set in FigureSets)
            {
                string[] Parts = Set.Split('-');

                if (Parts.Length < 3)
                {
                    return false;
                }

                string Name = Parts[0];
                int Type = int.Parse(Parts[1]);
                int Color = int.Parse(Parts[2]);
                int Unknown = Parts.Length > 3 ? int.Parse(Parts[3]) : 0; // ??????

                if (Type <= 0 || Color < 0)
                {
                    return false;
                }

                if (Name.Length != 2)
                {
                    return false;
                }

                if (Name == "hd")
                {
                    HasHead = true;
                }
            }

            if (!HasHead || (UserGender != "m" && UserGender != "f"))
            {
                return false;
            }

            return true;
        }
    }
}