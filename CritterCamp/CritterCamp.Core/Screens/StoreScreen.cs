using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games;
using CritterCamp.Core.Screens.UIElements;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens {
    class StoreScreen : MenuScreen {
        List<UIElement> TransitionAnimations;
        protected Type lastScreen;

        Color selectedTint = new Color(239, 116, 111);
        Color normalTint = new Color(195, 221, 84);
        PlayerData myData;

        Label itemViewLabel;
        View itemView;

        Label itemName;
        Label itemDescription;
        Label currentMoney;
        Button buyItem;

        Button selectedCategory;

        int leftXMiddle = 576;
        int leftYTop = 50;

        int rightXMiddle = 1536;

        bool categorySelected; // whether a category is selected
        bool gameSelected; // whether a game is selected
        bool itemSelected; // whether an item is selected

        public StoreScreen(Type lastScreen) : base() {
            this.lastScreen = lastScreen;

            TransitionOnTime = new TimeSpan(0, 0, 0, 1, 0);
            TransitionOffTime = new TimeSpan(0, 0, 0, 0, 200);
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            myData = Storage.Get<PlayerData>("myPlayerData");
            TransitionAnimations = new List<UIElement>();

            categorySelected = false;
            gameSelected = false;
            itemSelected = false;

            BorderedView storeCategories = new BorderedView(new Vector2(672, 450), new Vector2(1536, 258));
            storeCategories.BorderWidth = 0;
            Label categories = new Label("Categories", new Vector2(rightXMiddle, 125));
            categories.Font = "gillsans";
            categories.Scale = 1.2f;
            Button skins = new SmallButton("Skins");
            skins.Position = new Vector2(rightXMiddle, 250);
            skins.TappedArgs.Arg = "skins";
            skins.Tapped += CategoryButton_Tapped;
            Button upgrades = new SmallButton("Upgrades");
            upgrades.Position = new Vector2(rightXMiddle, 370);
            upgrades.TappedArgs.Arg = "upgrades";
            upgrades.Tapped += CategoryButton_Tapped;
            storeCategories.Disabled = false;
            storeCategories.AddElement(categories, skins, upgrades);
            storeCategories.SetAnimationOffset(new Vector2(1000, 0), Helpers.EaseOutBounceAnimation, false); // set the animation to start 1000 pixels to the right
            storeCategories.UpdateAnimationPosition(0); // start the animation to move the elements out of screen
            TransitionAnimations.Add(storeCategories); // add it to the list of animated objects
            mainView.AddElement(storeCategories);

            BorderedView itemDetails = new BorderedView(new Vector2(672, 552), new Vector2(rightXMiddle, 780));
            itemDetails.BorderWidth = 0;

            itemName = new Label("", new Vector2(rightXMiddle, 580));
            itemName.Font = "gillsans";
            itemName.MaxSize(760);
            itemDescription = new Label("", new Vector2(rightXMiddle, 650));
            itemDescription.MaxSize(760);
            buyItem = new SmallButton("");
            buyItem.Position = new Vector2(1536, 745);
            buyItem.ButtonImage = "buyButton";
            buyItem.Tapped += BuyItem_Tapped;
            ClearItem();

            currentMoney = new Label("$" + myData.money, new Vector2(rightXMiddle, 870));
            currentMoney.Font = "gillsans";
            currentMoney.TextColor = Color.Yellow;
            Button back = new SmallButton("Back");
            back.Position = new Vector2(1536, 978);
            back.Tapped += backButton_Tapped;
            itemDetails.Disabled = false;
            itemDetails.AddElement(itemName, itemDescription, buyItem, currentMoney, back);
            itemDetails.SetAnimationOffset(new Vector2(1000, 0), Helpers.EaseOutBounceAnimation, false); // set the animation to start 1000 pixels to the right
            itemDetails.UpdateAnimationPosition(0); // start the animation to move the elements out of screen
            TransitionAnimations.Add(itemDetails); // add it to the list of animated objects
            mainView.AddElement(itemDetails);

            BorderedView storeLeft = new BorderedView(new Vector2(1152, 1300), new Vector2(leftXMiddle, 540));
            storeLeft.Disabled = false;
            itemView = new View(new Vector2(1152, 1300), new Vector2(leftXMiddle, 540));
            itemView.Disabled = false;
            itemViewLabel = new Label("Select a category at the right", new Vector2(leftXMiddle, leftYTop));
            itemViewLabel.Font = "gillsans";
            storeLeft.AddElement(itemViewLabel, itemView);
            storeLeft.SetAnimationOffset(new Vector2(-1500, 0), Helpers.EaseOutBounceAnimation, false); // set the animation to start 1500 pixels to the left
            storeLeft.UpdateAnimationPosition(0); // start the animation to move the elements out of screen
            TransitionAnimations.Add(storeLeft); // add it to the list of animated objects
            mainView.AddElement(storeLeft);
        }

        private void ClearCategory() {
            itemViewLabel.Text = "Select a category at the right";
            itemView.RemoveAllElements();
            selectedCategory.DrawSelected = false; // unhighlight it
            selectedCategory.Disabled = false; // let it be pressed again
            selectedCategory = null;
            categorySelected = false;
        }

        private void ClearItem() {
            // clears the bottom right view
            itemName.Visible = false;
            itemDescription.Visible = false;
            buyItem.Visible = false;
            itemSelected = false;
        }

        private void CategoryButton_Tapped(object sender, UIElementTappedArgs e) {
            categorySelected = true;

            if (selectedCategory != null) {
                // there was a previous selected category
                selectedCategory.DrawSelected = false; // unhighlight it
                selectedCategory.Disabled = false; // let it be pressed again
            }

            selectedCategory = (Button)e.Element;
            selectedCategory.Disabled = true; // don't let it be pressed again
            selectedCategory.DrawSelected = true; // highlight it to show the user what category were in

            itemDescription.Text = "selected " + e.Arg;
            switch (e.Arg) {
                case "skins":
                    SwitchToSkinsCategory();
                    break;
                case "upgrades":
                    SwitchToUpgradesCategory();
                    break;
            }
        }

        private void SwitchToSkinsCategory() {
            ClearItem();
            itemView.RemoveAllElements(); // remove any elements from the last category
            itemViewLabel.Text = "Profile Skins";

            Label comingSoon = new Label("New skins coming soon!", new Vector2(leftXMiddle, leftYTop + 75));
            itemView.AddElement(comingSoon);
        }

        private void SwitchToUpgradesCategory() {
            ClearItem();
            gameSelected = false;
            itemView.RemoveAllElements(); // remove any elements from the last category
            itemViewLabel.Text = "Single Player Game Upgrades";

            int startX = 192;
            int startY = leftYTop + 150;
            int count = 0;

            // generate the store items and buttons for the game upgrades
            foreach (GameData gd in GameConstants.GAMES) {
                // draw the game icon and name
                Button gameChoice = new Button(gd.GameIconTexture, gd.GameIconIndex);
                gameChoice.Size = new Vector2(128, 128);
                gameChoice.Position = new Vector2(startX, startY);
                gameChoice.TappedArgs.ObjectArg = gd;
                gameChoice.Tapped += Game_Tapped;
                Label line1 = new Label(gd.NameLine1, new Vector2(startX, startY + 140));
                line1.Font = "gillsans";
                line1.Scale = 0.8f;
                Label line2 = new Label(gd.NameLine2, new Vector2(startX, startY + 200));
                line2.Font = "gillsans";
                line2.Scale = 0.8f;

                count++;
                if (count == 4) {
                    count = 0;
                    startX = 192;
                    startY += 356;
                } else {
                    startX += 256;
                }

                itemView.AddElement(gameChoice, line1, line2);
            }
        }

        private void Game_Tapped(object sender, UIElementTappedArgs e) {
            GameData gd = (GameData)e.ObjectArg;
            GenerateUpgradeStore(gd);
        }

        private void GenerateUpgradeStore(GameData gd) {
            gameSelected = true;
            ClearItem();
            itemView.RemoveAllElements(); // remove any elements from the last category
            itemViewLabel.Text = gd.Name + " Upgrades";

            int startY = leftYTop + 150;

            // generate the store items and buttons for the game upgrades
            foreach (GameUpgrade gu in gd.GameUpgrades) {
                GameUpgradeStoreItem gusi = new GameUpgradeStoreItem(gu);

                Button upgrade = new LongButton(gu.Name);
                upgrade.Position = new Vector2(leftXMiddle, startY);
                upgrade.TappedArgs.ObjectArg = gusi;
                upgrade.Tapped += StoreItem_Tapped;
                Label level = new Label("Level: " + gu.Level + "/5", new Vector2(leftXMiddle, startY + 115));
                startY += 275;

                itemView.AddElement(upgrade, level);
            }
        }
        
        private void StoreItem_Tapped(object sender, UIElementTappedArgs e) {
            itemSelected = true;
            StoreItem si = (StoreItem)e.ObjectArg;

            if (si.GetType().IsAssignableFrom(typeof(GameUpgradeStoreItem))) {
                // this is a game upgrade item
                GameUpgradeStoreItem gusi = (GameUpgradeStoreItem)si;
                itemDescription.Text = si.Name;
                itemName.Visible = true;
                itemDescription.Visible = true;

                if (gusi.Level < 5) { // upgrade is not maxed out yet 
                    // allow the user to buy the upgrade
                    itemName.Text = "Level " + (gusi.Level + 1) + " Upgrade";
                    buyItem.TappedArgs.ObjectArg = si;
                    buyItem.Text = si.Price.ToString();
                    buyItem.Visible = true;
                } else {
                    itemName.Text = "Already at max level";
                    buyItem.Visible = false;
                }
            } else {
                // this is a regular upgrade item
                itemName.Text = si.Name;
                itemName.Visible = true;
                itemDescription.Visible = false;

                buyItem.TappedArgs.ObjectArg = si;
                buyItem.Text = si.Price.ToString();
                buyItem.Visible = true;
            }
        }

        private void BuyItem_Tapped(object sender, UIElementTappedArgs e) {
            StoreItem si = (StoreItem)e.ObjectArg;

            if (myData.money < si.Price) {
                // not enough money to buy it
                string line1 = "Not enough money to buy " + si.Name;
                string line2 = "You have $" + myData.money + " and you need $" + si.Price;
                ScreenManager.AddScreen(new MessagePopup(line1, line2), null);
                return;
            }

            // otherwise user has enough money, purchase the item
            if (si.GetType().IsAssignableFrom(typeof(GameUpgradeStoreItem))) {
                // this is a game upgrade item
                GameUpgradeStoreItem gusi = (GameUpgradeStoreItem)si;
                GameUpgrade gu = gusi.MyGameUpgrade;
                gu.Level++; // increase the level by 1
                GenerateUpgradeStore(gu.MyGame);

                // send the information to the server
                JObject packet = new JObject(
                    new JProperty("action", "store"),
                    new JProperty("type", "game"),
                    new JProperty("game", gu.MyGame.ServerName),
                    new JProperty("upgrade_index", gu.Index)
                );
                conn.SendMessage(packet.ToString());
            }

            myData.money -= si.Price;
            Storage.Set("myPlayerData", myData);
            currentMoney.Text = "$" + myData.money;

            // show a confirmation to the user
            string line = "Sucessfully bought " + si.Name;
            ScreenManager.AddScreen(new MessagePopup(line, ""), null);
        }

        public override void OnBackPressed() {
            if (itemSelected) { // user selected an item
                // clear the item selected screen
                ClearItem();
            } else if (gameSelected) { // on the upgrades screen for a specific game
                // go back to the list of games 
                SwitchToUpgradesCategory();
            } else if (categorySelected) { // a category was selected
                // go back to main store page with no category
                ClearCategory();
            } else { // otherwise already on base screen
                SwitchScreen(lastScreen); // go back to the last screen
            }
        }

        private void backButton_Tapped(object sender, UIElementTappedArgs e) {
            OnBackPressed();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if (ScreenState == GameStateManagement.ScreenState.TransitionOn) {
                foreach (UIElement element in TransitionAnimations) {
                    element.UpdateAnimationPosition(1-TransitionPosition);
                }
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

    }
}