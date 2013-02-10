using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp {
    public static class TextureData {
        // Max number of sprites allowed in a single row
        public static int spriteSheetWidth = 10;

        // All background sprites for maps
        public enum mapTexture {
            nightSky1, nightSky2, nightSky3, nightSky4,
            grass1, grass2, grass3,
            dirtTL,
            dirtTR,
            dirtBL,
            dirtBR,
            dirtB,
            dirtT,
            dirtR,
            dirtL,
            dirt,
            longGrassNight,
            longGrass1
        }

        public enum PlayerStates {
            standing,
            jump1, jump2, jump3, jump4,
            pickup1, pickup2, pickup3,
            walkRight1, walkRight2, walkRight3, walkRight4,
            punchRight1, punchRight2, punchRight3,
            walkLeft1, walkLeft2, walkLeft3, walkLeft4,
            punchLeft1, punchLeft2, punchLeft3
        }

        public enum Effects {
            smoke1, smoke2, smoke3, smoke4
        }

        public enum Doodads {
            signTopLeft,
            signTopMid,
            signTopRight,
            signBtmLeft,
            signBtmMid,
            signBtmRight
        }

        public enum snTexture {
            arrow1, arrow2, arrow3, arrow4, arrow5, arrow6, arrow7, arrow8, arrow9, arrow10, arrow11,
            greenArrow, redArrow,
            timer,
            rocket1, rocket2
        }

        public enum SmallDoodads {
            heart
        }
    }
}