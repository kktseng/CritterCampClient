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
        public string Text = "Button";
        public Vector2 Position = Vector2.Zero;

        public event EventHandler<EventArgs> Tapped;

        /// <summary>
        /// Creates a new Button.
        /// </summary>
        /// <param name="text">The text to display in the button.</param>
        public Button(GameScreen screen, string text) {
            Text = text;
        }

        /// <summary>
        /// Invokes the Tapped event and allows subclasses to perform actions when tapped.
        /// </summary>
        protected virtual void OnTapped() {
            if(Tapped != null)
                Tapped(this, EventArgs.Empty);
        }

        /// <summary>
        /// Passes a tap location to the button for handling.
        /// </summary>
        /// <param name="tap">The location of the tap.</param>
        /// <returns>True if the button was tapped, false otherwise.</returns>
        public bool HandleTap(Vector2 tap) {
            Vector2 size = new Vector2(600, 160);
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

            // Load the button image if not loaded
            if(!screen.ScreenManager.Textures.ContainsKey("button")) {
                ContentManager cm = screen.ScreenManager.Game.Content;
                screen.ScreenManager.Textures.Add("button", cm.Load<Texture2D>("button600"));
            }
            Texture2D button = screen.ScreenManager.Textures["button"];

            // Draw the button
            
            sd.Draw(button, Position, 0, new Vector2(600, 160));
            sd.DrawString(font, Text, Position); 
        }
    }
}
