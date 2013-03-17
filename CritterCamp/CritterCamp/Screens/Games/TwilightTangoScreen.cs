using CritterCamp.Screens.Games.Lib;
using CritterCamp.Screens.Games.TwilightTango;
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

    class TwilightTangoScreen : BaseGameScreen {
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

        protected int rounds = 0; // Number of rounds that have been played
        protected int commandNum = COMMAND_INIT; // Number of commands to display
        protected int currentMove; // Current dance move
        protected double timerBar = 0;
        private TimeSpan start;
        private TimeSpan timer;

        // temp until I can figure out how to deal with fonts
        protected SpriteFont arial;

        public TwilightTangoScreen(List<PlayerData> playerData) : base(playerData) {
            currentRank = playerData.Count;
            for(int i = 0; i < playerData.Count; i++) {
                players[playerData[i].username] = new Player(this, new Vector2(100 + 650 * i, 800));
            }

            // Enable flick gestures
            EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            textureList["twilight"] = cm.Load<Texture2D>("twilightTextures");
            textureList["map"] = cm.Load<Texture2D>("mapTextures");
            textureList["doodads"] = cm.Load<Texture2D>("doodads");
            textureList["effects"] = cm.Load<Texture2D>("effects");
            textureList["pig"] = cm.Load<Texture2D>("pig");
            arial = cm.Load<SpriteFont>("Fonts/menufont");
            setMap();
        }

        public void setMap() {
            tileMap = new TileMap(textureList["map"]);
            int[,] map = new int[,] {
                {  3,  1,  2,  3,  0,  1,  2,  3,  0,  1,  2,  3,  0,  1,  2,  3,  0,  1,  2,  3 },
                {  1,  2,  1,  3,  3,  2,  1,  3,  2,  0,  1,  2,  3,  3,  1,  1,  3,  0,  1,  2 },
                {  2,  2,  0,  1,  2,  3,  0,  1,  2,  3,  2,  2,  3,  2,  2,  1,  1,  3,  2,  1 },
                {  1,  2,  3,  2,  1,  2,  2,  3,  0,  2,  3,  0,  1,  3,  1,  0,  1,  1,  3,  0 },
                {  2,  1,  2,  3,  0,  2,  0,  3,  2,  1,  1,  2,  3,  2,  2,  3,  1,  1,  2,  1 },
                {  3,  0,  1,  1,  3,  3,  2,  2,  3,  0,  3,  2,  3,  0,  1,  2,  2,  0,  1,  2 },
                { 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16 },
                { 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17 },
                {  9,  6,  4,  4,  5,  6,  4, 10, 11,  9,  4,  5,  4, 10, 11,  9,  4,  5,  4,  4 },
                { 14,  4,  4,  6, 10,  9,  4,  8, 15, 14,  4,  4, 10, 15, 12, 12,  9, 10,  9,  4 },
                {  7,  4, 10, 11, 12,  7,  4,  5,  8,  7, 10, 11, 15,  7,  6,  4, 13, 15, 14,  6 },
                {  4,  4,  8,  7,  4,  4, 10, 11,  9,  4,  8, 15, 14,  4,  4,  5,  8, 15,  7,  4 }
            };
            tileMap.setMap(map);
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
                                commandList.Add(new Arrow(this, (Direction)(int)a[i], textureList["twilight"], new Vector2(200 + 250 * (i % 7), 200 + 250 * (i / 7)), 1.4f));
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
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            sd.Begin();
            tileMap.draw(sd);
            DrawActors(sd);
            if(banner != null) {
                // Draw banner
                banner.Draw(new Vector2(Constants.BUFFER_WIDTH / 2, 300));
            }
            // Draw timer bar
            if(timerBar > 0) {
                for(int i = 0; i < (timerBar * 18) - 1; i++) {
                    sd.Draw(textureList["twilight"], new Vector2(Constants.BUFFER_SPRITE_DIM * 1.5f + Constants.BUFFER_SPRITE_DIM * i, 64), (int)TextureData.twilightTexture.timer);
                }
                bool rocketBool = (((int)(timerBar * 15d) % 2) == 0);
                int rocket = rocketBool ? (int)TextureData.twilightTexture.rocket1 : (int)TextureData.twilightTexture.rocket2;
                float rocketOffset = rocketBool ? 0 : 8;
                // Final bar, draw the partial bar
                sd.Draw(textureList["twilight"], new Vector2(Constants.BUFFER_SPRITE_DIM * 1.5f + Constants.BUFFER_SPRITE_DIM * (int)Math.Floor(timerBar * 18), 64), (int)TextureData.twilightTexture.timer, new Rectangle(0, 0, (int)(timerBar % (1d / 18d) * 64d * 18d), 64));
                sd.Draw(textureList["twilight"], new Vector2(Constants.BUFFER_SPRITE_DIM + (float)(timerBar * (Constants.BUFFER_WIDTH - 2 * Constants.BUFFER_SPRITE_DIM)), 60 + rocketOffset), rocket);
            }
            sd.End();
            base.Draw(gameTime);
        }

        protected override void MessageReceived(string message, bool error, TCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "game" && (string)o["name"] == "twilight_tango") {
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

        protected void processCommand(Direction command) {
            if(phase == Phase.Input) {
                // Only add commands if player hasn't exceeded the number of commands
                int numArrows = players[playerName].input.Count();
                if(numArrows < commandNum) {
                    players[playerName].input.Add(command);
                    inputArrows.Add(new Arrow(this, command, textureList["twilight"], new Vector2(150 + 120 * numArrows, 150), 1f));

                    // Send the command information to the server
                    JObject packet = new JObject(
                        new JProperty("action", "game"),
                        new JProperty("name", "twilight_tango"),
                        new JProperty("data", new JObject(
                            new JProperty("action", "command"),
                            new JProperty("commands", players[playerName].input.Cast<int>().ToList())
                        ))
                    );
                    conn.SendMessage(packet.ToString());
                }
            }
        }
    }
}
