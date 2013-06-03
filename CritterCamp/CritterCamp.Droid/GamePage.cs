using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using CritterCamp.Core.Screens;
using CritterCamp.Screens.UIScreens;
using System;

namespace CritterCamp.Droid {
    [Activity(Label = "CritterCamp.Droid"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.SensorLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class GamePage : Microsoft.Xna.Framework.AndroidGameActivity, IOfflineScreen {
        OfflineScreenCore offlineScreenCore;
        View LoginView;
        View UserInput;
        TextView Status;
        View ResumeView;
        Game1 g;

        TextView Username;
        TextView Password;

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.OffineScreen);
            LoginView = FindViewById<View>(Resource.Id.LoginView);
            UserInput = FindViewById<View>(Resource.Id.UserInput);
            Status = FindViewById<TextView>(Resource.Id.Status);
            ResumeView = FindViewById<View>(Resource.Id.ResumeButton);

            Username = FindViewById<TextView>(Resource.Id.Username);
            Password = FindViewById<TextView>(Resource.Id.Password);

            Button Login = FindViewById<Button>(Resource.Id.Login);
            Login.Click += Login_Click;
            Button Register = FindViewById<Button>(Resource.Id.Register);
            Register.Click += Register_Click;
            ResumeView.Click += Play_Click;

            Game1.Activity = this;            
            g = new Game1();
            AddContentView(g.Window, new FrameLayout.LayoutParams(FrameLayout.LayoutParams.FillParent, FrameLayout.LayoutParams.FillParent));
            g.Run();

            offlineScreenCore = new OfflineScreenCore(this);
        }


        public void ShowControls(bool show) {
            if (show) {
                LoginView.Visibility = ViewStates.Visible;
            } else {
                LoginView.Visibility = ViewStates.Gone;
            }
        }

        public void AppendStatusText(string text) {
            Status.Text += text;
        }

        public void UpdateStatusText(string text) {
            Status.Text = text;
        }

        public void ShowUserInput(bool show) {
            if (show) {
                UserInput.Visibility = ViewStates.Visible;
            } else {
                UserInput.Visibility = ViewStates.Gone;
            }
        }

        public void ShowResume(bool show) {
            if (show) {
                ResumeView.Visibility = ViewStates.Visible;
            } else {
                ResumeView.Visibility = ViewStates.Gone;
            }
        }

        public void ShowAdDuplex(bool show) {
            // no adduplex for android :(
        }

        public void GoToNextScreen(Type screen) {
            /*
            ScreenFactory sf = (ScreenFactory)g.screenManager.Game.Services.GetService(typeof(IScreenFactory));
            LoadingScreen.Load(g.screenManager, true, null, sf.CreateScreen(screen));
             */
        }

        private void Register_Click(object sender, EventArgs e) {
            Username.Text = Username.Text.Trim();
            string username = Username.Text.Trim();
            string password = Password.Text;

            offlineScreenCore.Register(username, password);
        }

        private void Login_Click(object sender, EventArgs e) {
            Username.Text = Username.Text.Trim();
            string username = Username.Text.Trim();
            string password = Password.Text;

            offlineScreenCore.Login(username, password);
        }

        private void Play_Click(object sender, EventArgs e) {
            offlineScreenCore.Resume();
        }
    }
}

