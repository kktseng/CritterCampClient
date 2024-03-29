﻿using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens;
using CritterCamp.Core.Screens.UIScreens;
using GameStateManagement;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Media;
using MonoGame.Framework.WindowsPhone;
using System;
using System.Windows;

namespace CritterCamp {
    public partial class GamePage : PhoneApplicationPage, IOfflineScreen {
        private CritterCampGame _game;
        OfflineScreenCore offlineScreenCore;

        // Constructor
        public GamePage() {
            InitializeComponent();

            _game = XamlGame<CritterCampGame>.Create("", this);
            Storage.Set("scaleFactor", App.Current.Host.Content.ScaleFactor);

            //TryMediaPlay();
            //adDuplexAd.IsTest = true; // use this line to display our own ad for testing

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();

            offlineScreenCore = new OfflineScreenCore(this);
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e) {
            if (e.Orientation == PageOrientation.LandscapeLeft) {
                base.OnOrientationChanged(e);
            }
        }

        public void ShowControls(bool show) {
            Dispatcher.BeginInvoke(() => {
                if (show) {
                    LayoutRoot.Visibility = Visibility.Visible;
                } else {
                    LayoutRoot.Visibility = Visibility.Collapsed;
                }
            });
        }

        public void MediaPlayerMuted(bool muted) {
            Dispatcher.BeginInvoke(() => {
                MediaPlayer.IsMuted = muted;
            });
        }

        public void AppendStatusText(string text) {
            Dispatcher.BeginInvoke(() => {
                Status.Text += text;
            });
        }

        public void UpdateStatusText(string text) {
            Dispatcher.BeginInvoke(() => {
                Status.Text = text;
            });
        }

        public void ShowUserInput(bool show) {
            Dispatcher.BeginInvoke(() => {
                if (show) {
                    UserInput.Visibility = Visibility.Visible;
                } else {
                    UserInput.Visibility = Visibility.Collapsed;
                }
            });
        }

        public void ShowResume(bool show) {
            Dispatcher.BeginInvoke(() => {
                if (show) {
                    ResumeButton.Visibility = Visibility.Visible;
                } else {
                    ResumeButton.Visibility = Visibility.Collapsed;
                }
            });
        }

        public void ShowAdDuplex(bool show) {
            Dispatcher.BeginInvoke(() => {
                if (show) {
                    adDuplexAd.Visibility = Visibility.Visible;
                } else {
                    adDuplexAd.Visibility = Visibility.Collapsed;
                }
            });
        }

        public void GoToNextScreen(Type screen) {
            ScreenFactory sf = (ScreenFactory)_game.screenManager.Game.Services.GetService(typeof(IScreenFactory));
            LoadingScreen.Load(_game.screenManager, true, null, sf.CreateScreen(screen));
        }

        private void Register_Click(object sender, RoutedEventArgs e) {
            Username.Text = Username.Text.Trim();
            string username = Username.Text.Trim();
            string password = PasswordBox.Password;

            offlineScreenCore.Register(username, password);
        }

        private void Login_Click(object sender, RoutedEventArgs e) {
            Username.Text = Username.Text.Trim();
            string username = Username.Text.Trim();
            string password = PasswordBox.Password;

            offlineScreenCore.Login(username, password);
        }

        private void Play_Click(object sender, RoutedEventArgs e) {
            offlineScreenCore.Resume();
        }
    }
}