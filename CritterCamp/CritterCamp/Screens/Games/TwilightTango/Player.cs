using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.TwilightTango {
    public enum PlayerDanceStates {
        Standing,
        DanceLeft,
        DanceUp,
        DanceRight,
        DanceDown
    }

    public class Player : AnimatedObject<PlayerDanceStates>, IComparable<Player> {
        public int health;
        public List<Direction> input;
        public int rank;

        public Player(BaseGameScreen screen, Vector2 coord)
            : base(screen, "pig", coord) {
            input = new List<Direction>();
            setState(PlayerDanceStates.Standing);
            health = 4;
            maxCycles = 1;
        }

        protected override void setAnim() {
            setDefaultState(PlayerDanceStates.Standing);
            animation.Add(PlayerDanceStates.Standing, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.standing, 1)
            });
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
            animation.Add(PlayerDanceStates.DanceRight, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.punchRight1, 75),
                new Frame((int)TextureData.PlayerStates.punchRight2, 75, new Vector2(-13, 0)),
                new Frame((int)TextureData.PlayerStates.punchRight3, 150, new Vector2(-13, 0))
            });
            animation.Add(PlayerDanceStates.DanceLeft, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.punchRight1, 75, new Vector2(0, 0), effect: SpriteEffects.FlipHorizontally),
                new Frame((int)TextureData.PlayerStates.punchRight2, 75, new Vector2(13, 0), effect: SpriteEffects.FlipHorizontally),
                new Frame((int)TextureData.PlayerStates.punchRight3, 150, new Vector2(13, 0), effect: SpriteEffects.FlipHorizontally)
            });
        }

        public override void draw(SpriteDrawer sd) {
            for(int j = 0; j < health; j++) {
                sd.Draw(getImg(), getCoord() + new Vector2(100 * j, 0), getNum(), effect: getFrame().Value.effect);
            }
        }

        public int CompareTo(Player p) {
            return p.health.CompareTo(health);
        }
    }
}
