using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.UIElements;
using Microsoft.Xna.Framework;

#if WINDOWS_PHONE
using Microsoft.Phone.Tasks;
#endif

namespace CritterCamp.Core.Screens {
    class AboutScreen : MenuScreen {
        BorderedView aboutPage;

        public AboutScreen() : base() { }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            IsPopup = true;

            aboutPage = new BorderedView(new Vector2(1150, 902), new Vector2(1920 / 2, 1080 / 2 - 75));
            aboutPage.Disabled = false;

            int startX = 445;
            int startY = 120;
            Label about = new Label("About", new Vector2(startX, startY));
            about.CenterX = false;
            about.Font = "tahoma";
            Label version = new Label("Version " + Configuration.VERSION, new Vector2(startX, startY + 70));
            version.CenterX = false;
            version.Scale = 0.8f;

            Label email1 = new Label("Email any issues to: ", new Vector2(startX + 670, startY - 20));
            email1.CenterX = false;
            email1.Scale = 0.8f;
            Label email2 = new Label("CritterCampGame@gmail.com", new Vector2(startX + 340, startY + 30));
            email2.CenterX = false;

            Button rate = new Button("Rate Us");
            rate.Position = new Vector2(startX + 200, startY + 177);
            rate.Tapped += rateButton_Tapped;

            Image fbIcon = new Image("fbIcon", 0, new Vector2(100, 100), new Vector2(startX + 75, startY + 350));
            Label fb = new Label("fb.me/CritterCampGame", new Vector2(startX + 180, startY + 350));
            fb.CenterX = false;
            Image twIcon = new Image("twitterIcon", 0, new Vector2(100, 100), new Vector2(startX + 75, startY + 515));
            Label tw = new Label("@CritterCampGame", new Vector2(startX + 180, startY + 515));
            tw.CenterX = false;

            Label music1 = new Label("Music", new Vector2(startX, startY + 655));
            music1.CenterX = false;
            music1.Scale = 0.8f;
            Label music2 = new Label("Call to Adventure by Kevin Macleod", new Vector2(startX, startY + 715));
            music2.CenterX = false;

            aboutPage.AddElement(about);
            aboutPage.AddElement(version);
            aboutPage.AddElement(email1);
            aboutPage.AddElement(email2);
            aboutPage.AddElement(rate);
            aboutPage.AddElement(fbIcon);
            aboutPage.AddElement(fb);
            aboutPage.AddElement(twIcon);
            aboutPage.AddElement(tw);
            aboutPage.AddElement(music1);
            aboutPage.AddElement(music2);
            mainView.AddElement(aboutPage);
        }

        private void rateButton_Tapped(object sender, UIElementTappedArgs e) {
#if WINDOWS_PHONE
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
            marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;
            marketplaceDetailTask.Show();
#endif
        }
    }
}
