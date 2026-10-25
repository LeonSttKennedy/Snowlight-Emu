using Snowlight.Game.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Misc
{
    public class UserFigureHelper
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

        public static string AddLookPiecePart(string ItemClothe, List<string> UserClothes)
        {
            return ReplaceLookParts(ItemClothe, UserClothes, true, true, true, true);
        }

        public static string ReplaceHeadPart(string ItemClothe, List<string> UserClothes)
        {
            return ReplaceLookParts(new List<string> { ItemClothe }, UserClothes, false, true, true);
        }

        public static string ReplaceLookParts(string ItemClothe, List<string> UserClothes, bool IgnoreHead = true,
            bool IgnoreHair = true, bool PreserveUserClothes = false, bool AddClothesToUser = false)
        {
            return ReplaceLookParts(new List<string> { ItemClothe }, UserClothes,
                IgnoreHead, IgnoreHair, PreserveUserClothes, AddClothesToUser);
        }

        public static string ReplaceLookParts(string[] ItemClothes, List<string> UserClothes, bool IgnoreHead = true,
            bool IgnoreHair = true, bool PreserveUserClothes = false, bool AddClothesToUser = false)
        {
            return ReplaceLookParts(ItemClothes.ToList(), UserClothes,
                IgnoreHead, IgnoreHair, PreserveUserClothes, AddClothesToUser);
        }

        public static string ReplaceLookParts(List<string> ItemClothes, List<string> UserClothes, bool IgnoreHead = true,
            bool IgnoreHair = true, bool PreserveUserClothes = false, bool AddClothesToUser = false)
        {
            if (ItemClothes == null ||  UserClothes == null)
            {
                return string.Empty;
            }

            for (int i = UserClothes.Count - 1; i >= 0; i--)
            {
                string UserClothePiece = UserClothes[i];

                // Ignores any empty string texts
                if (string.IsNullOrEmpty(UserClothePiece) || UserClothePiece.Length < 2) 
                {
                    continue;
                }

                // Ignores any hair styles or head string texts
                bool IsHead = UserClothePiece.StartsWith("hd");
                bool IsHair = UserClothePiece.StartsWith("hr");

                if ((IgnoreHead && IsHead) || (IgnoreHair && IsHair)) 
                {
                    continue;
                }

                string ClothePieceCategory = UserClothePiece.Substring(0, 2);
                string MatchingClothePiece = ItemClothes.FirstOrDefault(x => x.StartsWith(ClothePieceCategory));

                if (MatchingClothePiece == null)
                {
                    if (!PreserveUserClothes)
                    {
                        UserClothes.RemoveAt(i);
                    }

                    if (AddClothesToUser && ItemClothes.Count > 0)
                    {
                        UserClothes.Add(ItemClothes[ItemClothes.Count - 1]);
                    }

                    continue;
                }

                // Preserve the user's skin color when replacing the head.
                if (!IgnoreHead && IsHead)
                {
                    int HeadStyle = UserClothePiece.IndexOf('-');
                    int SkinColour = HeadStyle >= 0 ? UserClothePiece.IndexOf('-', HeadStyle + 1) : -1;

                    if (SkinColour >= 0)
                    {
                        UserClothes[i] = MatchingClothePiece.TrimEnd('-') + UserClothePiece.Substring(SkinColour);
                    }
                    else
                    {
                        UserClothes[i] = MatchingClothePiece;
                    }
                }
                else
                {
                    UserClothes[i] = MatchingClothePiece;
                }
            }

            return string.Join(".", UserClothes);
        }
    }
}