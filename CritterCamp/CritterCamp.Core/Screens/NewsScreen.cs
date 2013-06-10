using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games;
using CritterCamp.Core.Screens.UIElements;
using Microsoft.Xna.Framework;
using System.Linq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using CritterCamp.Core.Screens.Games.Lib;
using GameStateManagement;
using System.Globalization;

namespace CritterCamp.Core.Screens {
    class NewsScreen : MenuScreen {
        protected int startX = 475;
        protected int startY = 110;

        public NewsScreen() : base() { }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            IsPopup = true;

            BorderedView newsPage = new BorderedView(new Vector2(1100, 840), new Vector2(1920 / 2, 1080 / 2 - 75));
            newsPage.Disabled = false;

            Label newsTitle = new Label("Latest News", new Vector2(1920/2, startY));
            newsTitle.Font = "museoslab";
            newsTitle.Scale = 0.8f;

            List<NewsPost> news = Storage.Get<List<NewsPost>>("news");
            if (news.Count != 0) {
                NewsPost firstNews = news.ElementAt(0);
                Label newsDate = new Label(firstNews.TimeStamp.ToString("M", new CultureInfo("en-US")), new Vector2(startX, startY + 75));
                newsDate.CenterX = false;;
                String lineBreaksPost = NewsPost.insertLineBreaks(firstNews.Post, 1050, ScreenManager);
                Label newsPostLabel = new Label(lineBreaksPost, new Vector2(startX, startY + 115));
                newsPostLabel.CenterX = false;
                newsPostLabel.CenterY = false;

                Button close = new SmallButton("Close");
                close.Position = new Vector2(1920 / 2, startY + 700);
                close.Tapped += close_Tapped;

                newsPage.AddElement(newsTitle, newsDate, newsPostLabel, close);
            } else {
                Label noNews = new Label("No new news posts to display", new Vector2(startX, startY + 100));
                noNews.CenterX = false;
                newsPage.AddElement(newsTitle, noNews);
            }
            mainView.AddElement(newsPage);
        }

        void close_Tapped(object sender, UIElementTappedArgs e) {
            OnBackPressed();
        }
    }

    class NewsPost {
        public DateTime TimeStamp;
        public string Post;
        public string Id;

        NewsPost(DateTime timeStamp, string post, string id) {
            TimeStamp = timeStamp;
            Post = post;
            Id = id;
        }

        public static NewsPost createFromJObject(JObject newsPost) {
            if (newsPost["_id"] == null || newsPost["post"] == null || newsPost["date"] == null) {
                return null;
            }

            string id = (string)newsPost["_id"];
            string post = (string)newsPost["post"];
            DateTime timeStamp = (DateTime)newsPost["date"];

            return new NewsPost(timeStamp, post, id);
        }

        public static string insertLineBreaks(string post, float maxSize, ScreenManager MyScreenManager) {
            float maxSizeScaled = maxSize * SpriteDrawer.drawScale.X;
            string result = "";
            string wordToAdd = "";
            string tryAdd;
            foreach (char c in post) {
                if (c == ' ') {
                    // this char is a white space. word to add contains the next word to add 
                    tryAdd = result + (result == "" ? "" : " ") + wordToAdd;
                    if (MyScreenManager.Fonts["tahoma"].MeasureString(tryAdd).X < maxSizeScaled) {
                        result = tryAdd;
                    } else {
                        result += "\n" + wordToAdd;
                    }
                    wordToAdd = "";
                } else {
                    // keep building our word
                    wordToAdd += c;
                }
            }

            // add the last word
            tryAdd = result + (result == "" ? "" : " ") + wordToAdd;
            if (MyScreenManager.Fonts["tahoma"].MeasureString(tryAdd).X < maxSizeScaled) {
                result = tryAdd;
            } else {
                result += "\n" + wordToAdd;
            }

            return result;
        }
    }
}
