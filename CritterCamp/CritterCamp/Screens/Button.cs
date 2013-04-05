using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens {
    /// <summary>
    /// Represents a touchable button.
    /// </summary>
    class Button {
        public string Text; // text to display in the button
        public string Caption1; // text to display on the bottom of the button
        public string Caption2; // text to display on the bottom of the button
        public string image;
        public int textureIndex;
        public Vector2 size;
        public Vector2 Position;
        public ButtonArgs buttonArgs;
        public bool highlight;
        public bool visible; // bool if this button is visible (if not visible button is not pressable)
        public bool disabled; // bool if the button is disabled (visible, but not pressable. has a gray overlay)
        private bool selected; // bool if the button is pressed down (has a green overlay)


        public event EventHandler<ButtonArgs> Tapped;

        /// <summary>
        /// Creates a new Button.
        /// </summary>
        /// <param name="text">The text to display in the button.</param>
        public Button(GameScreen screen) {
            Text = "";
            Caption1 = "";
            Caption2 = "";
            size = new Vector2(290, 90);
            image = "buttonMint";
            this.textureIndex = 0;
            Position = Vector2.Zero;
            buttonArgs = new ButtonArgs(this);
            highlight = false;
            visible = true;
            disabled = false;
            selected = false;
        }

        /// <summary>
        /// Creates a new Button.
        /// </summary>
        /// <param name="text">The text to display in the button.</param>
        public Button(GameScreen screen, string text) : this(screen) {
            Text = text;
        }

        /// <summary>
        /// Creates a new Button.
        /// </summary>
        /// <param name="image">The name of the texture to display in the button.</param>
        /// <param name="size">The size of the button.</param>
        public Button(GameScreen screen, string image, Vector2 size) : this(screen) {
            this.image = image;
            this.size = size;
        }

        /// <summary>
        /// Creates a new Button.
        /// </summary>
        /// <param name="image">The name of the texture to display in the button.</param>
        /// <param name="textureIndex">The index of the texture.</param>
        public Button(GameScreen screen, string image, int textureIndex, Vector2 size) : this(screen, image, size) {
            this.textureIndex = textureIndex;
        }

        /// <summary>
        /// Invokes the Tapped event and allows subclasses to perform actions when tapped.
        /// </summary>
        public void OnTapped() {
            if(Tapped != null)
                Tapped(this, buttonArgs);
        }

        /// <summary>
        /// Passes a touch location to the button for handling.
        /// </summary>
        /// <param name="touch">The location of the touch.</param>
        /// <returns>True if the button was touched, false otherwise.</returns>
        public bool HandleTouch(Vector2 touch) {
            if (!visible || disabled) {
                // if this button is not visible or is disabled
                // dont let the user press it
                return false;
            }

            if (touch.X >= Position.X - size.X / 2 &&
                    touch.Y >= Position.Y - size.Y / 2 &&
                    touch.X <= Position.X + size.X / 2 &&
                    touch.Y <= Position.Y + size.Y / 2) {
                selected = true;
                return true;
            }

            selected = false;
            return false;
        }

        public void ResetSelected() {
            selected = false; // make this button not selected anymore
        }

        /// <summary>
        /// Draws the button
        /// </summary>
        /// <param name="screen">The screen drawing the button</param>
        public void Draw(GameScreen screen) {
            if (!visible) {
                // button is not visible. don't draw it
                return;
            }

            // Grab some common items from the ScreenManager
            SpriteBatch spriteBatch = screen.ScreenManager.SpriteBatch;
            SpriteFont font = screen.ScreenManager.Fonts["buttonFont"];
            SpriteFont captionFont = screen.ScreenManager.Fonts["blueHighway28"];
            SpriteDrawer sd = (SpriteDrawer)screen.ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            Color buttonFontColor = Color.White;

            // Draw the button
            if(selected) {
                // this is selected. draw a green overlay
                sd.Draw(screen.ScreenManager.Textures[image], Position, textureIndex, size, new Rectangle(0, 0, (int)size.X, (int)size.Y), SpriteEffects.None, Color.Green);
                buttonFontColor = Color.Gray;
            } else if (disabled) {
                // button is disabled. draw a grey overlay
                sd.Draw(screen.ScreenManager.Textures[image], Position, textureIndex, size, new Rectangle(0, 0, (int)size.X, (int)size.Y), SpriteEffects.None, Color.Gray);
                buttonFontColor = Color.Gray;        
            } else {
                // otherwise draw it normally
                sd.Draw(screen.ScreenManager.Textures[image], Position, textureIndex, size);
            }
            
            // This is way too hacky. We need to either extend this class or just draw the glow on the votescreen class
            if(highlight) {
                // this button is highlighted. draw a green glow
                sd.Draw(screen.ScreenManager.Textures[image], Position, (int)TextureData.games.glow, size);
            }

            sd.DrawString(font, Text, Position, buttonFontColor);
            sd.DrawString(captionFont, Caption1, new Vector2(Position.X, Position.Y + size.Y + 15), Color.Black);
            sd.DrawString(captionFont, Caption2, new Vector2(Position.X, Position.Y + size.Y + 65), Color.Black); 
        }
    }

    class ButtonArgs : EventArgs {
        public Button button;
        public string arg;
        public GameData gameData;

        public ButtonArgs(Button b) {
            button = b;
            arg = "";
            gameData = null;
        }
    }
}
