using System;

namespace CritterCamp.Core.Lib {
    public static class TextureData {
        // Max number of sprites allowed in a single row
        public static int spriteSheetWidth = 10;

        // Gutter size in pixels
        public static int spriteSheetGutter = 2;

        // All background sprites for maps
        public enum mapTexture {
            // too lazy
        }

        public enum ButtonSquare {
            main,
            profile,
            profile2,
            store
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
            signTopLeft, signTopMid, signTopRight, signBtmLeft, signBtmMid, signBtmRight, // 0
            jetPack1, jetPack2, jetPack3, jetPack4, // 6
            sideJet1, sideJet2, sideJet3, sideJet4, // 10
            jetFlame1, jetFlame2, // 14
            fenceTopEnd, fenceBottomEnd, fenceTopLeft, fenceLeftEnd, fence3Way, fence2Way, fenceTopRight, fenceRightEnd, // 16
            fishingPole1, fishingPole2, // 24
            smallSign, // 26
            flag, // 27
            greenBaton1, redBaton1, redBaton2, // 28
            skull
        }

        public enum twilightTexture {
            arrow1, arrow2, arrow3, arrow4, arrow5, arrow6, arrow7, arrow8, arrow9, arrow10, arrow11,
            greenArrow, redArrow,
            timer, rocket
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
            boat1, boat2, boat3, boat4, boat5, boat6, boat7, boat8, boat9,
            shark1, shark2, shark3, shark4, shark5, shark6,
            whale1, whale2, whale3, whale4, whale5, whale6, whale7, whale8
        }

        public enum colorTextures {
            splatter1, //splatter1_2, splatter1_3, splatter1_4,
            //splatter2_1, splatter2_2, splatter2_3, splatter2_4,
            //splatter3_1, splatter3_2, splatter3_3, splatter3_4,
            //splatter4_1, splatter4_2, splatter4_3, splatter4_4,
            paintBall,
            crosshair,
            frameCorner, frameSide, canvas
        }

        public enum Fish {
            small,
            medium,
            largeBlue1, largeBlue2,
            largeOrange1, largeOrange2,
            shiny1, shiny2, shiny3
        }

        public enum games {
            glow,
            twilightTango,
            jetpackJamboree,
            fishingFrenzy,
            colorClash
        }
    }
}