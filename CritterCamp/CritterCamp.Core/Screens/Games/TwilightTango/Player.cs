using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens.Games.TwilightTango {
    public enum PlayerDanceStates {
        Standing,
        DanceLeft,
        DanceUp,
        DanceRight,
        DanceDown
    }

    public class Player : AnimatedObject<PlayerDanceStates>, IComparable<Player> {
        public int health;
        public int maxHealth;
        public List<Direction> input;
        public int rank;
        public PlayerData player;

        public Player(BaseGameScreen screen, Vector2 coord, PlayerData player)
            : base(screen, "pig", coord) {
            input = new List<Direction>();
            State = PlayerDanceStates.Standing;
            maxHealth = screen.singlePlayer ? 9 : 5;
            health = maxHealth;
            maxCycles = 1;
            this.player = player;
        }

        protected override void SetAnim() {
            setDefaultState(PlayerDanceStates.Standing);
            animation.Add(PlayerDanceStates.Standing, SingleFrame((int)TextureData.PlayerStates.standing));
            animation.Add(PlayerDanceStates.DanceUp, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.jump4, 50),
                new Frame((int)TextureData.PlayerStates.jump1, 50, new Vector2(0, -21)),
                new Frame((int)TextureData.PlayerStates.jump2, 100, new Vector2(0, -37)),
                new Frame((int)TextureData.PlayerStates.jump3, 100, new Vector2(0, -4)),
                new Frame((int)TextureData.PlayerStates.jump4, 50, new Vector2(0, 3))
            });
            animation.Add(PlayerDanceStates.DanceDown, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.pickup1, 100),
                new Frame((int)TextureData.PlayerStates.pickup2, 100),
                new Frame((int)TextureData.PlayerStates.pickup3, 150)
            });
            SetLeftRight(new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.punchRight1, 75),
                new Frame((int)TextureData.PlayerStates.punchRight2, 75, new Vector2(-13, 0)),
                new Frame((int)TextureData.PlayerStates.punchRight3, 150, new Vector2(-13, 0))
            }, PlayerDanceStates.DanceRight, PlayerDanceStates.DanceLeft);
        }

        public override void Draw(SpriteDrawer sd) {
            for(int i = 0; i < health; i++) {
                sd.DrawPlayer(screen, player, Coord + new Vector2((100 * (i % (maxHealth / 2 + 1)) + (i / (maxHealth / 2 + 1)) * 50), (i / (maxHealth / 2 + 1)) * 65), GetNum(), spriteEffect: GetFrame().Value.effect);
            }
        }

        public int CompareTo(Player p) {
            return p.health.CompareTo(health);
        }
    }
}
