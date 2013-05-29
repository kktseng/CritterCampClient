using CritterCamp.Screens.Games.Lib;
using CritterCamp.Screens.Games.TwilightTango;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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

namespace CritterCamp.Screens.Games {
    public enum Direction {
        Left, Up, Right, Down
    }

    class TwilightTangoScreen : BaseGameScreen {
        public static int MAX_ROUNDS = 11;
        public static int COMMAND_INCR = 1;
        public static int COMMAND_INIT = 4;

        public static TimeSpan COMMAND_TIME = new TimeSpan(0, 0, 4);
        public static TimeSpan COMMAND_TIME_INCR = new TimeSpan(0, 0, 1);
        public static TimeSpan INPUT_TIME = new TimeSpan(0, 0, 3);
        public static TimeSpan INPUT_INCR = new TimeSpan(0, 0, 1);
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

        public TwilightTangoScreen(Dictionary<string, PlayerData> playerData, bool singlePlayer)
            : base(playerData, singlePlayer) {
            currentRank = playerData.Count;
            for(int i = 0; i < playerData.Values.Count; i++) {
                PlayerData data = playerData.Values.ElementAt(i);
                players[data.username] = new Player(this, new Vector2(150 + 450 * i, 800), data);
            }

            // Enable flick gestures
            EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            AddTextures("twilight", "map", "doodads", "effects");
            AddSounds("ding", "dong", "chime1", "chime2", "chime3", "chime4", "puff");
            SetMap();
        }

        public void SetMap() {
            tileMap = new TileMap(textureList["map"]);
            int[,] map = new int[,] {
                {  3,  1,  2,  3,  0,  1,  2,  3,  0,  1,  2,  3,  0,  1,  2,  3,  0,  1,  2,  3 },
                {  1,  2,  1,  3,  3,  2,  1,  3,  2,  0,  1,  2,  3,  3,  1,  1,  3,  0,  1,  2 },
                {  2,  2,  0,  1,  2,  3,  0,  1,  2,  3,  2,  2,  3,  2,  2,  1,  1,  3,  2,  1 },
                {  1,  2,  3,  2,  1,  2,  2,  3,  0,  2,  3,  0,  1,  3,  1,  0,  1,  1,  3,  0 },
                {  2,  1,  2,  3,  0,  2,  0,  3,  2,  1,  1,  2,  3,  2,  2,  3,  1,  1,  2,  1 },
                {  3,  0,  1,  1,  3,  3,  2,  2,  3,  0,  3,  2,  3,  0,  1,  2,  2,  0,  1,  2 },
                { 20, 20, 21, 19, 20, 20, 21, 16, 18, 18, 19, 20, 20, 20, 21, 19, 20, 20, 20, 20 },
                { 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17 },
                {  9,  6,  4,  4,  5,  6,  4, 10, 11,  9,  4,  5,  4, 10, 11,  9,  4,  5,  4,  4 },
                { 14,  4,  4,  6, 10,  9,  4,  8, 15, 14,  4,  4, 10, 15, 12, 12,  9, 10,  9,  4 },
                {  7,  4, 10, 11, 12,  7,  4,  5,  8,  7, 10, 11, 15,  7,  6,  4, 13, 15, 14,  6 },
                {  4,  4,  8,  7,  4,  4, 10, 11,  9,  4,  8, 15, 14,  4,  4,  5,  8, 15,  7,  4 }
            };
            tileMap.SetMap(map);
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

        public override void RemovePlayer(string user) {
            // Don't have to do anything special since game ends without input from others
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
                    banner = new TextBanner(this, "WATCH THE STARS!");
                }

                if((gameTime.TotalGameTime - start) > BANNER_TIME) {
                    // Sync all players with server before beginning
                    if(!syncing) {
                        Random rand = new Random();
                        List<int> commands = new List<int>();
                        for(int i = 0; i < 20; i++) {
                            commands.Add(rand.Next(0, 4));
                        }
                        if(singlePlayer) {
                            RemoveActor(commandList);
                            for(int i = 0; i < commandNum; i++) {
                                commandList.Add(new Arrow(this, (Direction)(int)commands[i], textureList["twilight"], new Vector2(200 + 250 * (i % 7), 200 + 250 * (i / 7)), 1.4f));
                                commandList[commandList.Count - 1].Visible = false;
                            }
                            start = gameTime.TotalGameTime;
                            phase = Phase.Commands;
                        } else {
                            // Send ready packet to sync before starting
                            Sync((JArray data, double random) => {
                                RemoveActor(commandList);
                                //foreach(JToken commandArray in data) {
                                JToken commandArray = data[0]; // Temporary hack
                                JArray a = JArray.Parse((string)commandArray);
                                for(int i = 0; i < commandNum; i++) {
                                    commandList.Add(new Arrow(this, (Direction)(int)a[i], textureList["twilight"], new Vector2(200 + 250 * (i % 7), 200 + 250 * (i / 7)), 1.4f));
                                    commandList[commandList.Count - 1].Visible = false;

                                }
                                //}
                                start = gameTime.TotalGameTime;
                                phase = Phase.Commands;
                            }, JsonConvert.SerializeObject(commands));
                        }
                        syncing = true;
                    }
                }
            } else if(phase == Phase.Commands) {
                syncing = false;
                banner = null;
                TimeSpan newTime = COMMAND_TIME + (new TimeSpan(0, 0, (int)COMMAND_TIME_INCR.TotalSeconds * rounds));
                timer = newTime - gameTime.TotalGameTime + start;
                timerBar = timer.TotalMilliseconds / newTime.TotalMilliseconds;
                if(timer.TotalMilliseconds <= 0) {
                    foreach(Arrow a in commandList) {
                        a.State = ArrowStates.FadeOut;
                    }
                    start = gameTime.TotalGameTime;
                    phase = Phase.Input;
                    return;
                } else {
                    // Draw commands every 500ms
                    for(int i = 0; i < (gameTime.TotalGameTime - start).TotalMilliseconds / 500; i++) {
                        if(i < commandList.Count) {
                            commandList[i].Visible = true;
                        }
                    }
                }
            } else if(phase == Phase.Input) {
                TimeSpan newTime = INPUT_TIME + (new TimeSpan(0, 0, (int)INPUT_INCR.TotalSeconds * rounds));
                timer = newTime - gameTime.TotalGameTime + start;
                timerBar = timer.TotalMilliseconds / newTime.TotalMilliseconds;
                if(banner == null && timer > (newTime - BANNER_TIME)) {
                    banner = new TextBanner(this, "SWIPE!");
                } else if(timer <= (newTime - BANNER_TIME)) {
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
                    banner = new TextBanner(this, "TIME'S UP!");
                }

                // Switch to dancing phase when finished waiting for slow packets
                if((gameTime.TotalGameTime - start) > BANNER_TIME) {
                    currentMove = 0;
                    banner = null;
                    phase = Phase.Dancing;
                    return;
                }
            } else if(phase == Phase.Dancing) {
                // Check for LMS
                if(currentRank == 1 && !singlePlayer) {
                    foreach(Player p in players.Values) {
                        if(p.health > 0)
                            p.rank = 1;
                    }
                    phase = Phase.GameOver;
                    return;
                }

                // Animate each character dance
                if(currentMove == 0 || (gameTime.TotalGameTime - start) > MOVE_TIME) {
                    if(currentMove >= commandList.Count()) {
                        // Switch to show winner when finished
                        foreach(Player p in new List<Player>(players.Values)) {
                            p.State = PlayerDanceStates.Standing;
                        }

                        // Clear input list
                        foreach(Player p in new List<Player>(players.Values)) {
                            p.input.Clear();
                        }
                        RemoveActor(inputArrows);

                        phase = Phase.Close;
                        return;
                    }
                    List<Player> tieCheck = new List<Player>();
                    foreach(string s in new List<string>(players.Keys)) {
                        Player p = players[s];
                        bool error = false;
                        if(p.input.Count > currentMove) {
                            if(p.input[currentMove] != commandList[currentMove].dir) {
                                error = true;
                                if(s == playerName) {
                                    soundList["dong"].Play();
                                    inputArrows.ElementAt(currentMove).State = ArrowStates.Red;
                                }
                            } else if(s == playerName) {
                                soundList["ding"].Play();
                                score++;
                                inputArrows.ElementAt(currentMove).State = ArrowStates.Green;
                            }
                            p.State = PlayerDanceStates.DanceLeft + (int)p.input[currentMove];
                        } else {
                            error = true;
                            p.State = PlayerDanceStates.Standing;
                        }
                        if(error && p.health > 0) {
                            soundList["puff"].Play();
                            p.health--;
                            new Smoke(this, p.Coord + new Vector2((100 * (p.health % 3)) + ((p.health / 3) * 50), (p.health / 3) * 65));               
                            if(p.health <= 0) {
                                p.rank = currentRank;
                                foreach(Player tiedPlayer in tieCheck) {
                                    tiedPlayer.rank--;
                                }
                                tieCheck.Add(p);
                                if(--currentRank <= 0) {
                                    phase = Phase.GameOver;
                                }
                            }

                        }

                    }
                    currentMove++;
                    start = gameTime.TotalGameTime; // Reset time for next move
                }
            } else if(phase == Phase.Close) {
                // Exit game when max number of rounds have been reached or a winner has been reached
                start = gameTime.TotalGameTime;
                if(++rounds >= MAX_ROUNDS) {
                    if(singlePlayer) {
                        rounds--;
                        phase = Phase.Sync;
                    } else {
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
                    }
                } else {
                    commandNum += COMMAND_INCR;
                    phase = Phase.Sync;
                }
            } else if(phase == Phase.GameOver) {
                if(banner == null) {
                    string bannerText;
                    if(singlePlayer)
                        bannerText = "SCORE: " + score;
                    else
                        bannerText = (players[playerName].rank == 1) ? "YOU WIN!" : "GAME OVER";
                    banner = new TextBanner(this, bannerText);
                }
                if((gameTime.TotalGameTime - start) > BANNER_TIME) {
                    if(singlePlayer) {
                        scoreReceived = true;
                    } else {
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
                    }
                    phase = Phase.Sleep;
                }
            } else if(phase == Phase.Sleep) {
                // Do nothing
            }
        }

        public override void Draw(GameTime gameTime) {
            SpriteDrawer sd = Helpers.GetSpriteDrawer(this);
            sd.Begin();
            tileMap.Draw(sd);
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
            if(phase == Phase.Input && players[playerName].health > 0) {
                // Only add commands if player hasn't exceeded the number of commands
                int numArrows = players[playerName].input.Count();
                if(numArrows < commandNum) {
                    players[playerName].input.Add(command);
                    inputArrows.Add(new Arrow(this, command, textureList["twilight"], new Vector2(150 + 120 * numArrows, 150), 1f));

                    if(!singlePlayer) {
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
}
