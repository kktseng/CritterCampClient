#region File Description
//-----------------------------------------------------------------------------
// ScreenFactory.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using CritterCamp;
using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens;
using CritterCamp.Core.Screens.Games;
using System;
using System.Collections.Generic;

namespace GameStateManagement {
    /// <summary>
    /// Our game's implementation of IScreenFactory which can handle creating the screens
    /// when resuming from being tombstoned.
    /// </summary>
    public class ScreenFactory : IScreenFactory {
        public GameScreen CreateScreen(Type screenType) {
            if(screenType == typeof(TutorialScreen)) {
                GameData currGame = Storage.Get<GameData>("currentGameData");
                return new TutorialScreen(currGame);
            } else if(typeof(BaseGameScreen).IsAssignableFrom(screenType)) {
                Dictionary<string, PlayerData> playerData = Storage.Get<Dictionary<string, PlayerData>>("player_data");
                bool single = Storage.Get<bool>("singlePlayer");
                return Activator.CreateInstance(screenType, new object[2] { playerData, single }) as GameScreen;
            } else if(typeof(MainScreen).IsAssignableFrom(screenType)) {
                if(Storage.ContainsKey("profileBounce")) {
                    bool bounce = Storage.Get<bool>("profileBounce");
                    Storage.Remove("profileBounce");
                    return Activator.CreateInstance(screenType, new object[1] { bounce }) as GameScreen;
                } else {
                    return Activator.CreateInstance(screenType, new object[1] { true }) as GameScreen;
                }
            } else {
                return Activator.CreateInstance(screenType) as GameScreen;
            }
            // If we had more complex screens that had constructors or needed properties set,
            // we could do that before handing the screen back to the ScreenManager. For example
            // you might have something like this:
            //
            // if (screenType == typeof(MySuperGameScreen))
            // {
            //     bool value = GetFirstParameter();
            //     float value2 = GetSecondParameter();
            //     MySuperGameScreen screen = new MySuperGameScreen(value, value2);
            //     return screen;
            // }
            //
            // This lets you still take advantage of constructor arguments yet participate in the
            // serialization process of the screen manager. Of course you need to save out those
            // values when deactivating and read them back, but that means either IsolatedStorage or
            // using the PhoneApplicationService.Current.State dictionary.
        }
    }
}
