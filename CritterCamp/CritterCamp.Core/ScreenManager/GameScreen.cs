#region File Description
//-----------------------------------------------------------------------------
// GameScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens;
using CritterCamp.Core.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System;
namespace GameStateManagement {
    /// <summary>
    /// Enum describes the screen transition state.
    /// </summary>
    public enum ScreenState {
        TransitionOn,
        Active,
        TransitionOff,
        Hidden,
    }

    public delegate void SyncAction(JArray data, double rand); 

    /// <summary>
    /// A screen is a single layer that has update and draw logic, and which
    /// can be combined with other layers to build up a complex menu system.
    /// For instance the main menu, the options menu, the "are you sure you
    /// want to quit" message box, and the main game itself are all implemented
    /// as screens.
    /// </summary>
    public abstract class GameScreen {
        protected ITCPConnection conn;
        protected bool online;

        public GameScreen(bool online) {
            this.online = online;
        }

        private SyncAction syncAction;

        protected virtual void MessageReceived(string message, bool error, ITCPConnection connection) {
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "group" && (string)o["type"] == "synced") {
                if(syncAction != null) {
                    syncAction((JArray)o["data"], (double)o["rand"]);
                    CancelSync();
                }
            }   
        }

        protected void Sync(SyncAction syncAction) {
            this.syncAction = syncAction;
            Helpers.Sync();
        }

        protected void Sync(SyncAction syncAction, string data) {
            this.syncAction = syncAction;
            Helpers.Sync(data);
        }

        protected void Sync(SyncAction syncAction, string data, int timeout) {
            this.syncAction = syncAction;
            Helpers.Sync(data, timeout);
        }

        protected void CancelSync() {
            syncAction = null;
        }

        public void SetConn(ITCPConnection conn) {
            RemoveConn(); // remove the old connection first

            if (conn != null) { // set the new conenction
                this.conn = conn;
                conn.pMessageReceivedEvent += MessageReceived;
            }
        }

        public void RemoveConn() {
            if (conn != null) {
                conn.pMessageReceivedEvent -= MessageReceived; // remove the method from the old connection
                conn = null;
            }
        }

        protected virtual void SwitchScreen(Type screen) {
            LoadingScreen.Load(ScreenManager, false, null, Helpers.GetScreenFactory(this).CreateScreen(screen));
        }

        protected Vector2 coordScale, backBuffer;
        /// <summary>
        /// Normally when one screen is brought up over the top of another,
        /// the first screen will transition off to make room for the new
        /// one. This property indicates whether the screen is only a small
        /// popup, in which case screens underneath it do not need to bother
        /// transitioning off.
        /// </summary>
        public virtual bool IsPopup {
            get { return isPopup; }
            protected set { isPopup = value; }
        }

        bool isPopup = false;


        /// <summary>
        /// Indicates how long the screen takes to
        /// transition on when it is activated.
        /// </summary>
        public TimeSpan TransitionOnTime {
            get { return transitionOnTime; }
            protected set { transitionOnTime = value; }
        }

        TimeSpan transitionOnTime = TimeSpan.Zero;


        /// <summary>
        /// Indicates how long the screen takes to
        /// transition off when it is deactivated.
        /// </summary>
        public TimeSpan TransitionOffTime {
            get { return transitionOffTime; }
            protected set { transitionOffTime = value; }
        }

        TimeSpan transitionOffTime = TimeSpan.Zero;


        /// <summary>
        /// Gets the current position of the screen transition, ranging
        /// from zero (fully active, no transition) to one (transitioned
        /// fully off to nothing).
        /// </summary>
        public float TransitionPosition {
            get { return transitionPosition; }
            protected set { transitionPosition = value; }
        }

        float transitionPosition = 1;


        /// <summary>
        /// Gets the current alpha of the screen transition, ranging
        /// from 1 (fully active, no transition) to 0 (transitioned
        /// fully off to nothing).
        /// </summary>
        public float TransitionAlpha {
            get { return 1f - TransitionPosition; }
        }


        /// <summary>
        /// Gets the current screen transition state.
        /// </summary>
        public ScreenState ScreenState {
            get { return screenState; }
            protected set { screenState = value; }
        }

        ScreenState screenState = ScreenState.TransitionOn;


        /// <summary>
        /// There are two possible reasons why a screen might be transitioning
        /// off. It could be temporarily going away to make room for another
        /// screen that is on top of it, or it could be going away for good.
        /// This property indicates whether the screen is exiting for real:
        /// if set, the screen will automatically remove itself as soon as the
        /// transition finishes.
        /// </summary>
        public bool IsExiting {
            get { return isExiting; }
            protected internal set { isExiting = value; }
        }

        bool isExiting = false;


        /// <summary>
        /// Checks whether this screen is active and can respond to user input.
        /// </summary>
        public bool IsActive {
            get {
                return !otherScreenHasFocus &&
                       (screenState == ScreenState.TransitionOn ||
                        screenState == ScreenState.Active);
            }
        }

        bool otherScreenHasFocus;


        /// <summary>
        /// Gets the manager that this screen belongs to.
        /// </summary>
        public ScreenManager ScreenManager {
            get { return screenManager; }
            internal set { screenManager = value; }
        }

        ScreenManager screenManager;


        /// <summary>
        /// Gets the index of the player who is currently controlling this screen,
        /// or null if it is accepting input from any player. This is used to lock
        /// the game to a specific player profile. The main menu responds to input
        /// from any connected gamepad, but whichever player makes a selection from
        /// this menu is given control over all subsequent screens, so other gamepads
        /// are inactive until the controlling player returns to the main menu.
        /// </summary>
        public PlayerIndex? ControllingPlayer {
            get { return controllingPlayer; }
            internal set { controllingPlayer = value; }
        }

        PlayerIndex? controllingPlayer;


        /// <summary>
        /// Gets the gestures the screen is interested in. Screens should be as specific
        /// as possible with gestures to increase the accuracy of the gesture engine.
        /// For example, most menus only need Tap or perhaps Tap and VerticalDrag to operate.
        /// These gestures are handled by the ScreenManager when screens change and
        /// all gestures are placed in the InputState passed to the HandleInput method.
        /// </summary>
        public GestureType EnabledGestures {
            get { return enabledGestures; }
            protected set {
                enabledGestures = value;

                // the screen manager handles this during screen changes, but
                // if this screen is active and the gesture types are changing,
                // we have to update the TouchPanel ourself.
                if(ScreenState == ScreenState.Active) {
                    TouchPanel.EnabledGestures = value;
                }
            }
        }

        GestureType enabledGestures = GestureType.None;

        /// <summary>
        /// Gets whether or not this screen is serializable. If this is true,
        /// the screen will be recorded into the screen manager's state and
        /// its Serialize and Deserialize methods will be called as appropriate.
        /// If this is false, the screen will be ignored during serialization.
        /// By default, all screens are assumed to be serializable.
        /// </summary>
        public bool IsSerializable {
            get { return isSerializable; }
            protected set { isSerializable = value; }
        }

        bool isSerializable = true;


        /// <summary>
        /// Activates the screen. Called when the screen is added to the screen manager or if the game resumes
        /// from being paused or tombstoned.
        /// </summary>
        /// <param name="instancePreserved">
        /// True if the game was preserved during deactivation, false if the screen is just being added or if the game was tombstoned.
        /// On Xbox and Windows this will always be false.
        /// </param>
        public virtual void Activate(bool instancePreserved) {
            if(online)
                SetConn(Storage.Get<ITCPConnection>("TCPSocket"));
        }


        /// <summary>
        /// Deactivates the screen. Called when the game is being deactivated due to pausing or tombstoning.
        /// </summary>
        public virtual void Deactivate() { }


        /// <summary>
        /// Unload content for the screen. Called when the screen is removed from the screen manager.
        /// </summary>
        public virtual void Unload() {
            RemoveConn();
            CancelSync();
        }


        /// <summary>
        /// Allows the screen to run logic, such as updating the transition position.
        /// Unlike HandleInput, this method is called regardless of whether the screen
        /// is active, hidden, or in the middle of a transition.
        /// </summary>
        public virtual void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            this.otherScreenHasFocus = otherScreenHasFocus;

            if(isExiting) {
                // If the screen is going away to die, it should transition off.
                screenState = ScreenState.TransitionOff;

                if(!UpdateTransition(gameTime, transitionOffTime, 1)) {
                    // When the transition finishes, remove the screen.
                    ScreenManager.RemoveScreen(this);
                }
            } else if(coveredByOtherScreen) {
                // If the screen is covered by another, it should transition off.
                if(UpdateTransition(gameTime, transitionOffTime, 1)) {
                    // Still busy transitioning.
                    screenState = ScreenState.TransitionOff;
                } else {
                    // Transition finished!
                    screenState = ScreenState.Hidden;
                    FinishedTransitioning(false);
                }
            } else {
                // Otherwise the screen should transition on and become active.
                if(UpdateTransition(gameTime, transitionOnTime, -1)) {
                    // Still busy transitioning.
                    screenState = ScreenState.TransitionOn;
                } else {
                    // Transition finished!
                    screenState = ScreenState.Active;
                    FinishedTransitioning(true);
                }
            }
        }

        /// <summary>
        /// Called when the screen finishes transitioning 
        /// </summary>
        protected virtual void FinishedTransitioning(bool active) { }


        /// <summary>
        /// Helper for updating the screen transition position.
        /// </summary>
        bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction) {
            // How much should we move by?
            float transitionDelta;

            if(time == TimeSpan.Zero)
                transitionDelta = 1;
            else
                transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds);

            // Update the transition position.
            transitionPosition += transitionDelta * direction;

            // Did we reach the end of the transition?
            if(((direction < 0) && (transitionPosition <= 0)) ||
                ((direction > 0) && (transitionPosition >= 1))) {
                transitionPosition = MathHelper.Clamp(transitionPosition, 0, 1);
                return false;
            }

            // Otherwise we are still busy transitioning.
            return true;
        }


        /// <summary>
        /// Allows the screen to handle user input. Unlike Update, this method
        /// is only called when the screen is active, and not when some other
        /// screen has taken the focus.
        /// </summary>
        public virtual void HandleInput(GameTime gameTime, InputState input) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) {
                OnBackPressed();
            }
        }

        public virtual void OnBackPressed() {
            // open up the exit popup
            ScreenManager.AddScreen(new ExitPopupScreen(), null);
        }

        /// <summary>
        /// This is called when the screen should draw itself.
        /// </summary>
        public virtual void Draw(GameTime gameTime) {
            SpriteDrawer sd = Helpers.GetSpriteDrawer(this);
            coordScale = sd.coordScale;
            backBuffer = sd.backBuffer;
        }


        /// <summary>
        /// Tells the screen to go away. Unlike ScreenManager.RemoveScreen, which
        /// instantly kills the screen, this method respects the transition timings
        /// and will give the screen a chance to gradually transition off.
        /// </summary>
        public void ExitScreen() {
            screenState = ScreenState.Hidden;
            if(TransitionOffTime == TimeSpan.Zero) {
                // If the screen has a zero transition time, remove it immediately.
                ScreenManager.RemoveScreen(this);
            } else {
                // Otherwise flag that it should transition off and then exit.
                isExiting = true;
            }
        }
    }
}