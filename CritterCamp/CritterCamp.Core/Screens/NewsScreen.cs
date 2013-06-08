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

namespace CritterCamp.Core.Screens {
    class NewsScreen : MenuScreen {
        public NewsScreen() : base() { }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            IsPopup = true;

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
                    if (MyScreenManager.Fonts["gillsans"].MeasureString(tryAdd).X < maxSizeScaled) {
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
            if (MyScreenManager.Fonts["gillsans"].MeasureString(tryAdd).X < maxSizeScaled) {
                result = tryAdd;
            } else {
                result += "\n" + wordToAdd;
            }

            return result;
        }
    }
}
