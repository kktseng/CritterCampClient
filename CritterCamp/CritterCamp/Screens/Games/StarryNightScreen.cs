using CritterCamp.Screens.Games.Lib;
using CritterCamp.Screens.Games.StarryNight;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens.Games {
    public enum Direction {
        Left, Up, Right, Down
    }

    public enum PlayerDanceStates {
        Standing,
        DanceLeft, DanceUp, DanceRight, DanceDown
    }

    public enum ArrowStates {
        FadeIn, FadeOut,
        Green, Red
    }

    class StarryNightScreen : BaseGameScreen {
        public static int MAX_ROUNDS = 5;
        public static int COMMAND_INCR = 2;
        public static int COMMAND_INIT = 6;

        public static TimeSpan COMMAND_TIME = new TimeSpan(0, 0, 3);
        public static TimeSpan INPUT_TIME = new TimeSpan(0, 0, 5);
        public static TimeSpan TIMEOUT_TIME = new TimeSpan(0, 0, 1);
        public static TimeSpan MOVE_TIME = new TimeSpan(0, 0, 0, 0, 600);
        public static TimeSpan BANNER_TIME = new TimeSpan(0, 0, 0, 1, 500);

        protected TileMap tileMap;
        protected TextBanner banner;
        protected bool syncing;
        protected int currentRank;

        protected Vector2 currentGesture = new Vector2(0);

        protected Phase phase = Phase.Initialize;
        protected enum Phase {
            Initialize,
            Sync,
            Commands,
            Input,
            Timeup, // Exists mainly to handle slow packets
            Dancing,
            Close,
            GameOver,
            Sleep
        }

        protected Dictionary<string, Player> players = new Dictionary<string, Player>();
        protected List<Arrow> commandList = new List<Arrow>();
        protected List<Arrow> inputArrows = new List<Arrow>();
        protected List<string> usernames;

        protected int rounds = 0; // Number of rounds that have been played
        protected int commandNum = COMMAND_INIT; // Number of commands to display
        protected int currentMove; // Current dance move
        protected double timerBar = 0;
        protected string playerName = (string)CoreApplication.Properties["username"];
        private TimeSpan start;
        private TimeSpan timer;

        // temp until I can figure out how to deal with fonts
        protected SpriteFont arial;

        public StarryNightScreen(List<string> usernames, List<string> pictures) : base() {
            this.usernames = usernames;
            currentRank = usernames.Count;
            for(int i = 0; i < usernames.Count; i++) {
                players[usernames[i]] = new Player(this, new Vector2(100 + 650 * i, 800));
            }

            // Enable flick gestures
            EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            textureList["sn"] = cm.Load<Texture2D>("snTextures");
            textureList["map"] = cm.Load<Texture2D>("mapTextures");
            textureList["doodads"] = cm.Load<Texture2D>("doodads");
            textureList["effects"] = cm.Load<Texture2D>("effects");
            textureList["pig"] = cm.Load<Texture2D>("pig");
            arial = cm.Load<SpriteFont>("menufont");
            setMap();
        }

        public void setMap() {
            tileMap = new TileMap(textureList["map"]);
            TextureData.mapTexture ns1 = TextureData.mapTexture.nightSky1;
            TextureData.mapTexture ns2 = TextureData.mapTexture.nightSky2;
            TextureData.mapTexture ns3 = TextureData.mapTexture.nightSky3;
            TextureData.mapTexture ns4 = TextureData.mapTexture.nightSky4;
            TextureData.mapTexture lgn = TextureData.mapTexture.longGrassNight;
            TextureData.mapTexture lg1 = TextureData.mapTexture.longGrass1;

            TextureData.mapTexture gr1 = TextureData.mapTexture.grass1;
            TextureData.mapTexture gr2 = TextureData.mapTexture.grass2;
            TextureData.mapTexture gr3 = TextureData.mapTexture.grass3;
            TextureData.mapTexture dbl = TextureData.mapTexture.dirtBL;
            TextureData.mapTexture drt = TextureData.mapTexture.dirt;
            TextureData.mapTexture dib = TextureData.mapTexture.dirtB;
            TextureData.mapTexture dbr = TextureData.mapTexture.dirtBR;
            TextureData.mapTexture dil = TextureData.mapTexture.dirtL;
            TextureData.mapTexture dir = TextureData.mapTexture.dirtR;
            TextureData.mapTexture dit = TextureData.mapTexture.dirtT;
            TextureData.mapTexture dtl = TextureData.mapTexture.dirtTL;
            TextureData.mapTexture dtr = TextureData.mapTexture.dirtTR;

            TextureData.mapTexture[,] map = new TextureData.mapTexture[,] {
                { ns4, ns2, ns3, ns4, ns1, ns2, ns3, ns4, ns1, ns2, ns3, ns4, ns1, ns2, ns3, ns4, ns1, ns2, ns3, ns4 },
                { ns2, ns3, ns2, ns4, ns4, ns3, ns2, ns4, ns3, ns1, ns2, ns3, ns4, ns4, ns2, ns2, ns4, ns1, ns2, ns3 },
                { ns3, ns3, ns1, ns2, ns3, ns4, ns1, ns2, ns3, ns4, ns3, ns3, ns4, ns3, ns3, ns2, ns2, ns4, ns3, ns2 },
                { ns2, ns3, ns4, ns3, ns2, ns3, ns3, ns4, ns1, ns3, ns4, ns1, ns2, ns4, ns2, ns1, ns2, ns2, ns4, ns1 },
                { ns3, ns2, ns3, ns4, ns1, ns3, ns1, ns4, ns3, ns2, ns2, ns3, ns4, ns3, ns3, ns4, ns2, ns2, ns3, ns2 },
                { ns4, ns1, ns2, ns2, ns4, ns4, ns3, ns3, ns4, ns1, ns4, ns3, ns4, ns1, ns2, ns3, ns3, ns1, ns2, ns3 },
                { lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn, lgn },
                { lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1, lg1 },
                { dbl, gr3, gr1, gr1, gr2, gr3, gr1, dbr, dib, dbl, gr1, gr2, gr1, dbr, dib, dbl, gr1, gr2, gr1, gr1 },
                { dil, gr1, gr1, gr3, dbr, dbl, gr1, dtr, drt, dil, gr1, gr1, dbr, drt, dit, dit, dbl, dbr, dbl, gr1 },
                { dtl, gr1, dbr, dib, dit, dtl, gr1, gr2, dtr, dtl, dbr, dib, drt, dtl, gr3, gr1, dir, drt, dil, gr3 },
                { gr1, gr1, dtr, dtl, gr1, gr1, dbr, dib, dbl, gr1, dtr, drt, dil, gr1, gr1, gr2, dtr, drt, dtl, gr1 }
            };
            tileMap.setMap(map);
        }

        protected override void MessageReceived(string message, bool error, TCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "game" && (string)o["name"] == "starry_night") {
                JObject data = (JObject)o["data"];
                if((string)data["action"] == "update") {
                    // Accept player update packets in both input and timeup phase
                    if(phase == Phase.Input || phase == Phase.Timeup) {
                        string source = (string)data["source"];
                        if(players.ContainsKey(source)) {
                            // If the client side commands are more updates, ignore the update
                            JArray commands = (JArray)data["commands"];
                            if(players[source].input.Count() < commands.Count()) {
                                players[source].input.Clear();
                                foreach(JToken command in commands) {
                                    players[source].input.Add((Direction)(int)command);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            foreach(GestureSample gesture in input.Gestures) {
                if(gesture.GestureType == GestureType.FreeDrag) {
                    currentGesture += gesture.Delta;
                } else if(gesture.GestureType == GestureType.DragComplete) {
                    if(Math.Abs(currentGesture.X) > Math.Abs(currentGesture.Y)) {
                        if(currentGesture.X > 0) {
                            processCommand(Direction.Up);
                        } else {
                            processCommand(Direction.Down);
                        }
                    } else {
                        if(currentGesture.Y > 0) {
                            processCommand(Direction.Right);
                        } else {
                            processCommand(Direction.Left);
                        }
                    }
                    currentGesture = new Vector2(0);
                }
            }

            base.HandleInput(gameTime, input);
        }

        protected void processCommand(Direction command) {
            if(phase == Phase.Input) {
                // Only add commands if player hasn't exceeded the number of commands
                int numArrows = players[playerName].input.Count();
                if(numArrows < commandNum) {
                    players[playerName].input.Add(command);
                    inputArrows.Add(new Arrow(this, command, textureList["sn"], new Vector2(150 + 120 * numArrows, 150), 1f));

                    // Send the command information to the server
                    JObject packet = new JObject(
                        new JProperty("action", "game"),
                        new JProperty("name", "starry_night"),
                        new JProperty("data", new JObject(
                            new JProperty("action", "command"),
                            new JProperty("commands", players[playerName].input.Cast<int>().ToList())
                        ))
                    );
                    conn.SendMessage(packet.ToString());
                }
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update based on phase
            if(phase == Phase.Initialize) {
                syncing = false;
                start = gameTime.TotalGameTime;
                phase = Phase.Sync;
            } else if(phase == Phase.Sync) {
                // Display banner before syncing
                if(banner == null) {
                    banner = new TextBanner(this, "WATCH THE STARS!", arial);
                }

                if((gameTime.TotalGameTime - start) > BANNER_TIME) {
                    // Sync all players with server before beginning
                    if(!syncing) {
                        Random rand = new Random();
                        List<int> commands = new List<int>();
                        for(int i = 0; i < 20; i++) {
                            commands.Add(rand.Next(0, 4));
                        }
                        // Send ready packet to sync before starting
                        Helpers.Sync((JArray data) => {
                            removeActor(commandList);
                            //foreach(JToken commandArray in data) {
                            JToken commandArray = data[0]; // Temporary hack
                            JArray a = JArray.Parse((string)commandArray);
                            for(int i = 0; i < commandNum; i++) {
                                commandList.Add(new Arrow(this, (Direction)(int)a[i], textureList["sn"], new Vector2(200 + 250 * (i % 7), 200 + 250 * (i / 7)), 1.4f));
                                commandList[commandList.Count - 1].setVisibility(false);

                            }
                            // }
                            start = gameTime.TotalGameTime;
                            phase = Phase.Commands;
                        }, JsonConvert.SerializeObject(commands));
                        syncing = true;
                    }
                }
            } else if(phase == Phase.Commands) {
                syncing = false;
                banner = null;
                timer = COMMAND_TIME - gameTime.TotalGameTime + start;
                timerBar = timer.TotalMilliseconds / COMMAND_TIME.TotalMilliseconds;
                if(timer.TotalMilliseconds <= 0) {
                    foreach(Arrow a in commandList) {
                        a.setState(ArrowStates.FadeOut);
                    }
                    start = gameTime.TotalGameTime;
                    phase = Phase.Input;
                    return;
                } else {
                    // Draw commands every 300ms
                    for(int i = 0; i < (gameTime.TotalGameTime - start).TotalMilliseconds / 300; i++) {
                        if(i < commandList.Count) {
                            commandList[i].setVisibility(true);
                        }
                    }
                }
            } else if(phase == Phase.Input) {
                timer = INPUT_TIME - gameTime.TotalGameTime + start;
                timerBar = timer.TotalMilliseconds / INPUT_TIME.TotalMilliseconds;
                if(banner == null && timer > (INPUT_TIME - BANNER_TIME)) {
                    banner = new TextBanner(this, "SWIPE!", arial);
                } else if(timer <= (INPUT_TIME - BANNER_TIME)) {
                    banner = null;
                }
                if(timer.TotalMilliseconds <= 0) {
                    start = gameTime.TotalGameTime;
                    phase = Phase.Timeup;
                    timerBar = 0;
                    return;
                }
            } else if(phase == Phase.Timeup) {
                if(banner == null) {
                    banner = new TextBanner(this, "TIME'S UP!", arial);
                }

                // Switch to dancing phase when finished waiting for slow packets
                if((gameTime.TotalGameTime - start) > BANNER_TIME) {
                    currentMove = 0;
                    banner = null;
                    phase = Phase.Dancing;
                    return;
                }
            } else if(phase == Phase.Dancing) {
                // Animate each character dance
                if(currentMove == 0 || (gameTime.TotalGameTime - start) > MOVE_TIME) {
                    if(currentMove >= commandList.Count()) {
                        // Switch to show winner when finished
                        foreach(Player p in new List<Player>(players.Values)) {
                            p.setState(PlayerDanceStates.Standing);
                        }

                        // Clear input list
                        foreach(Player p in new List<Player>(players.Values)) {
                            p.input.Clear();
                        }
                        removeActor(inputArrows);

                        phase = Phase.Close;
                        return;
                    }
                    foreach(string s in new List<string>(players.Keys)) {
                        Player p = players[s];
                        bool error = false;
                        if(p.input.Count > currentMove) {
                            if(p.input[currentMove] != commandList[currentMove].dir) {
                                error = true;
                                if(s == playerName) {
                                    inputArrows.ElementAt(currentMove).setState(ArrowStates.Red);
                                }
                            } else if(s == playerName) {
                                inputArrows.ElementAt(currentMove).setState(ArrowStates.Green);
                            }
                            p.setState(PlayerDanceStates.DanceLeft + (int)p.input[currentMove]);
                        } else {
                            error = true;
                            p.setState(PlayerDanceStates.Standing);
                        }
                        if(error && p.health > 0) {
                            p.health--;
                            if(p.health <= 0) {
                                p.rank = currentRank;
                                if(--currentRank <= 0) {
                                    phase = Phase.GameOver;
                                }
                            }
                            new Smoke(this, p.getCoord() + new Vector2(100 * p.health, 0));
                        }

                    }
                    currentMove++;
                    start = gameTime.TotalGameTime; // Reset time for next move
                }
            } else if(phase == Phase.Close) {
                // Exit game when max number of rounds have been reached or a winner has been reached
                start = gameTime.TotalGameTime;
                if(++rounds >= MAX_ROUNDS) {
                    List<Player> sortedPlayers = new List<Player>(players.Values);
                    sortedPlayers.Sort();
                    int lastHealth = 0, tieCount = 0;
                    for(int i = 0; i < sortedPlayers.Count; i++) {
                        if(sortedPlayers[i].health > 0) {
                            if(lastHealth == sortedPlayers[i].health) {
                                tieCount++;
                            } else {
                                tieCount = 0;
                            }
                            sortedPlayers[i].rank = i + 1 - tieCount;
                            lastHealth = sortedPlayers[i].health;
                        }
                    }
                    phase = Phase.GameOver;
                } else {
                    commandNum += COMMAND_INCR;
                    phase = Phase.Sync;
                }
            } else if(phase == Phase.GameOver) {
                if(banner == null) {
                    banner = new TextBanner(this, "GAME OVER", arial);
                }
                if((gameTime.TotalGameTime - start) > BANNER_TIME) {
                    // Sync scores
                    JObject packet = new JObject(
                        new JProperty("action", "group"),
                        new JProperty("type", "report_score"),
                        new JProperty("score", new JObject(
                            from username in new List<string>(players.Keys)
                            select new JProperty(username, players[username].rank)
                        ))
                    );
                    conn.SendMessage(packet.ToString());
                    expGained = (5 - players[playerName].rank) * 100;
                    phase = Phase.Sleep;
                }
            } else if(phase == Phase.Sleep) {
                // Do nothing
            }
        }

        public override void Draw(GameTime gameTime) {
            ScreenManager.SpriteBatch.Begin();
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            tileMap.draw(sd);
            DrawActors(gameTime);
            if(banner != null) {
                // Draw banner
                banner.Draw(new Vector2(Constants.BUFFER_WIDTH / 2, 300));
            }
            // Draw timer bar
            if(timerBar > 0) {
                for(int i = 0; i < (timerBar * 18) - 1; i++) {
                    sd.Draw(textureList["sn"], new Vector2(Constants.BUFFER_SPRITE_DIM * 1.5f + Constants.BUFFER_SPRITE_DIM * i, 64), (int)TextureData.snTexture.timer);
                }
                bool rocketBool = (((int)(timerBar * 15d) % 2) == 0);
                int rocket = rocketBool ? (int)TextureData.snTexture.rocket1 : (int)TextureData.snTexture.rocket2;
                float rocketOffset = rocketBool ? 0 : 8;
                // Final bar, draw the partial bar
                sd.Draw(textureList["sn"], new Vector2(Constants.BUFFER_SPRITE_DIM * 1.5f + Constants.BUFFER_SPRITE_DIM * (int)Math.Floor(timerBar * 18), 64), (int)TextureData.snTexture.timer, new Rectangle(0, 0, (int)(timerBar % (1d / 18d) * 64d * 18d), 64));
                sd.Draw(textureList["sn"], new Vector2(Constants.BUFFER_SPRITE_DIM + (float)(timerBar * (Constants.BUFFER_WIDTH - 2 * Constants.BUFFER_SPRITE_DIM)), 60 + rocketOffset), rocket);
            }
            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
