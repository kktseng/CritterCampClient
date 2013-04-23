using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using MonoGame.Framework.WindowsPhone;
using CritterCamp.Resources;
using System.IO.IsolatedStorage;
using Windows.ApplicationModel.Core;

namespace CritterCamp {
    public partial class GamePage : PhoneApplicationPage {
        private CritterCampGame _game;

        // Constructor
        public GamePage() {
            InitializeComponent();

            _game = XamlGame<CritterCampGame>.Create("", this);
            CoreApplication.Properties["GamePage"] = this;
            //adDuplexAd.IsTest = true; // use this line to display our own ad for testing

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        public void hideAdduplux() {
            Dispatcher.BeginInvoke(() =>
            {
                adDuplexAd.Visibility = Visibility.Collapsed;
            });
        }

        public void showAdduplux() {
            Dispatcher.BeginInvoke(() => {
                adDuplexAd.Visibility = Visibility.Visible;
            });
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}