using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp {
    public static class TextureData {
        // Max number of sprites allowed in a single row
        public static int spriteSheetWidth = 10;

        // Gutter size in pixels
        public static int spriteSheetGutter = 2;

        // All background sprites for maps
        public enum mapTexture {
            nightSky1, nightSky2, nightSky3, nightSky4, // 0
            grass1, grass2, grass3, // 4
            dirtTL, dirtTR, dirtBL, dirtBR, // 7
            dirtB, dirtT, dirtR, dirtL, // 11
            dirt, // 15
            longGrassNight, longGrass1, // 16
            tree, treeLeft, treeMiddle, treeRight, // 18
            water, // 22
            sandtop, sand1, sand2, sand3, sandstar, sandcrab, // 23
            sunset1, sunset2, sunset3, sunset4 // 29
        }

        public enum PlayerStates {
            standing,
            jump1, jump2, jump3, jump4,
            pickup1, pickup2, pickup3,
            walkRight1, walkRight2, walkRight3, walkRight4,
            punchRight1, punchRight2, punchRight3,
            walkDown1, walkDown2,
            holdUp1, holdUp2
        }
        public static int playerStateCount = (Enum.GetNames(typeof(PlayerStates)).Length + spriteSheetWidth - 1) / spriteSheetWidth * spriteSheetWidth;

        public enum Effects {
            smoke1, smoke2, smoke3, smoke4
        }

        public enum Explosions {
            nuke1, nuke2, nuke3, nuke4, nuke5, nuke6, nuke7, nuke8, nuke9, nuke10, nuke11, nuke12, nuke13, nuke14, nuke15, nuke16
        }

        public enum Doodads {
            signTopLeft, signTopMid, signTopRight, signBtmLeft, signBtmMid, signBtmRight,
            jetPack1, jetPack2, jetPack3, jetPack4,
            sideJet1, sideJet2, sideJet3, sideJet4,
            jetFlame1, jetFlame2,
            fenceTopEnd, fenceBottomEnd, fenceTopLeft, fenceLeftEnd, fence3Way, fence2Way, fenceTopRight, fenceRightEnd,
            crosshair,
            fishingPole1, fishingPole2,
            smallSign,
            flag,
            greenBaton1, redBaton1, redBaton2
        }

        public enum twilightTexture {
            arrow1, arrow2, arrow3, arrow4, arrow5, arrow6, arrow7, arrow8, arrow9, arrow10, arrow11,
            greenArrow, redArrow,
            timer,
            rocket1, rocket2
        }

        public enum jetpackTextures {
            cautionTop, cautionRight, cautionLeft, cautionBottom,
            orangeL, orange_, orangeLCurve, orangeTCurve, orangeCross,
            greenL, green_, greenLCurve, greenTCurve, greenCross,
            blueL, blue_, blueLCurve, blueTCurve, blueCross,
            pinkL, pink_, pinkLCurve, pinkTCurve, pinkCross
        }

        public enum fishingTextures {
            hook, sinker, line,
            bucket,
            wave1, wave2, wave3, wave4, wave5, wave6, wave7, wave8,
            boat1, boat2, boat3, boat4, boat5, boat6, boat7, boat8, boat9
        }

        public enum Fish {
            small,
            medium,
            largeBlue1,
            largeBlue2,
            largeOrange1,
            largeOrange2
        }

        public enum games {
            glow,
            twilightTango,
            jetpackJamboree,
            fishingFrenzy
        }
    }
}