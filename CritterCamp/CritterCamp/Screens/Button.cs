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
        public string Text = ""; // test to display in the button
        public string Caption = ""; // text to display on the right of the button
        public string image;
        public int textureIndex;
        public Vector2 size;
        public Vector2 Position;
        public ButtonArgs buttonArgs;

        public event EventHandler<ButtonArgs> Tapped;

        /// <summary>
        /// Creates a new Button.
        /// </summary>
        /// <param name="text">The text to display in the button.</param>
        public Button(GameScreen screen, string text) {
            Text = text;
            size = new Vector2(600, 160);
            image = "button";
            this.textureIndex = 0;
            Position = Vector2.Zero;
            buttonArgs = new ButtonArgs();
        }

        /// <summary>
        /// Creates a new Button.
        /// </summary>
        /// <param name="image">The name of the texture to display in the button.</param>
        /// <param name="size">The size of the button.</param>
        public Button(GameScreen screen, string image, Vector2 size) {
            this.image = image;
            this.size = size;
            this.textureIndex = 0;
            Position = Vector2.Zero;
            buttonArgs = new ButtonArgs();
        }

        /// <summary>
        /// Creates a new Button.
        /// </summary>
        /// <param name="image">The name of the texture to display in the button.</param>
        /// <param name="textureIndex">The index of the texture.</param>
        public Button(GameScreen screen, string image, int textureIndex, Vector2 size) {
            this.image = image;
            this.size = size;
            this.textureIndex = textureIndex;
            Position = Vector2.Zero;
            buttonArgs = new ButtonArgs();
        }

        /// <summary>
        /// Invokes the Tapped event and allows subclasses to perform actions when tapped.
        /// </summary>
        protected virtual void OnTapped() {
            if(Tapped != null)
                Tapped(this, buttonArgs);
        }

        /// <summary>
        /// Passes a tap location to the button for handling.
        /// </summary>
        /// <param name="tap">The location of the tap.</param>
        /// <returns>True if the button was tapped, false otherwise.</returns>
        public bool HandleTap(Vector2 tap) {
            if(tap.X >= Position.X - size.X / 2 &&
                tap.Y >= Position.Y - size.Y / 2 &&
                tap.X <= Position.X + size.X / 2 &&
                tap.Y <= Position.Y + size.Y / 2) {
                OnTapped();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Draws the button
        /// </summary>
        /// <param name="screen">The screen drawing the button</param>
        public void Draw(GameScreen screen) {
            // Grab some common items from the ScreenManager
            SpriteBatch spriteBatch = screen.ScreenManager.SpriteBatch;
            SpriteFont font = screen.ScreenManager.Font;
            SpriteDrawer sd = (SpriteDrawer)screen.ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));

            // Draw the button           
            sd.Draw(screen.ScreenManager.Textures[image], Position, textureIndex, size);
            sd.DrawString(font, Text, Position);
            sd.DrawString(font, Caption, new Vector2(Position.X + size.X + 15, Position.Y - size.Y/2), Color.Black, false); 
        }
    }

    class ButtonArgs : EventArgs {
        public string arg;

        public ButtonArgs() 
            : base() {
            arg = "";
        }

        public ButtonArgs(string arg)
            : base() {
            this.arg = arg;
        }
    }
}
