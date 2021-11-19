using System;

namespace TrueCraft.Core.Logic.Items
{
    public class DyeItem : ItemProvider
    {
        public enum DyeType
        {
            InkSac = 0,
            RoseRed = 1,
            CactusGreen = 2,
            CocoaBeans = 3,
            LapisLazuli = 4,
            PurpleDye = 5,
            CyanDye = 6,
            LightGrayDye = 7,
            GrayDye = 8,
            PinkDye = 9,
            LimeDye = 10,
            DandelionYellow = 11,
            LightBlueDye = 12,
            MagentaDye = 13,
            BoneMeal = 14
        }

        public static readonly short ItemID = 0x15F;

        public override short ID { get { return 0x15F; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            // TODO: Support additional textures
            return new Tuple<int, int>(14, 4);
        }

        public override string DisplayName { get { return "Dye"; } }
    }
}