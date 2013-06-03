using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents a view element which is a collection of UIElements
    /// </summary>
    class View : UIElement{
        protected List<UIElement> UIElements;
        protected UIElement SelectedUIElement;
        bool OnElement;

        /// <summary>
        /// Creates a new View
        /// </summary>
        public View() {
            Initialize();
        }

        /// <summary>
        /// Creates a new view with the given size, position
        /// </summary>
        public View(Vector2 size, Vector2 position) : base(size, position) {
            Initialize();
        }

        protected new void Initialize() {
            UIElements = new List<UIElement>();
        }

        public void AddElement(params UIElement[] uie) {
            lock (UIElements) {
                for(int i = 0; i < uie.Length; i++)
                    UIElements.Add(uie[i]);
            }
        }

        public void RemoveElement(params UIElement[] uie) {
            lock (UIElements) {
                for(int i = 0; i < uie.Length; i++)
                    UIElements.Remove(uie[i]);
            }
        }

        /// <summary>
        /// Invokes the Tapped event and allows subclasses to perform actions when tapped.
        /// </summary>
        public override void OnTapped() {
            base.OnTapped(); // invoke the tapped event for this view if any

            // pass on the tapped to the whatever UIElement was selected
            if (SelectedUIElement != null) {
                SelectedUIElement.OnTapped();
            }
        }

        /// <summary>
        /// Passes a touch location to this view for handling.
        /// </summary>
        /// <param name="scaledLoc">The location of the touch.</param>
        /// <returns>True if the UIElement was touched, false otherwise.</returns>
        public override bool HandleTouch(Vector2 scaledLoc, TouchLocation rawLocation, InputState input) {
            if (!base.HandleTouch(scaledLoc, rawLocation, input)) {
                return false; // touch wasn't for this view
            }

            if (input.TouchState.Count == 0) { // released our finger
                if (SelectedUIElement != null) {
                    if (OnElement) { // release our finger on the selected ui element
                        OnTapped(); // this counts as pressing the ui element
                    }
                    ResetSelected(); // make the ui element not pressed down anymore
                }
            } else {
                if (SelectedUIElement == null) { // we havn't selected an element yet, try to find one that we can select
                    if (rawLocation.State.HasFlag(TouchLocationState.Pressed)) {
                        // and this touch is the beginning of a touch
                        for (int i = 0; i < UIElements.Count; i++) {
                            UIElement uie = UIElements[UIElements.Count-i - 1];
                            if (uie.HandleTouch(scaledLoc, rawLocation, input)) {
                                // found the uielement that we pressed down on
                                OnElement = true;
                                SelectedUIElement = uie;
                                return true;
                            }
                        }
                    }
                } else {
                    // already have a ui element that we started our press in
                    // pass in the new coordinates to it
                    OnElement = SelectedUIElement.HandleTouch(scaledLoc, rawLocation, input);
                    return true;
                }
            }

            // went through all the elements in the view and couldn't find one that our press touches
            return true;
        }

        public override void ResetSelected() {
            base.ResetSelected();
            if (SelectedUIElement != null) {
                SelectedUIElement.ResetSelected();
                SelectedUIElement = null;
            }
        }

        /// <summary>
        /// Draws the UIElement.
        /// </summary>
        protected override void DrawThis() {
            // draw every UIElement in this view
            lock (UIElements) {
                foreach (UIElement uie in UIElements) {
                    uie.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
                }
            }
        }
    }
}
