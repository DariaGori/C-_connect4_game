using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLL;
using ConsoleUI;
using DAL;
using Domain;
using MenuSystem;
using Newtonsoft.Json;

namespace ConsoleApp
{
    class Program
    {
        private static GameState _state = new GameState();
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings { TypeNameAssemblyFormatHandling = 
            TypeNameAssemblyFormatHandling.Full };
        
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            SetDefaultState();
            
            Console.Clear();
            Console.WriteLine($"Welcome to the {_state.GameName} game!");
            Console.WriteLine();

            var savedGamesMenu = new Menu(1)
            {
                MenuTitle = $"Choose the {_state.GameName} game to continue with",
                MenuItemsDictionary = new Dictionary<string, MenuItem>()
            };
            
            List<string> savedGames = new List<string>();
            
            using (var ctx = new AppDbContext())
            {
                foreach (var gameSave in ctx.GameStates
                    .Where(g => g.GameStateId != 1))
                {
                    if (gameSave.GameSaveName != null) {savedGames.Add(gameSave.GameSaveName);}
                }
            };
            
            int i = 1;
            
            foreach (var save in savedGames)
            {
                savedGamesMenu.MenuItemsDictionary.Add($"{i}", new MenuItem() { Description = save, CommandToExecute = () 
                        =>
                    {
                        _state = LoadSavedGame(save);
                        return GameRun(_state);
                    }
                });
                i++;
            }
            
            var gameOptionMenu = new Menu(1)
            {
                MenuTitle = $"Choose a {_state.GameName} game mode",
                MenuItemsDictionary = new Dictionary<string, MenuItem>()
                {
                    {
                        "1", new MenuItem()
                        {
                            Description = "Computer starts",
                            CommandToExecute = () =>
                            {
                                _state.HumanPlayerCount = 1;
                                _state.ComputerMove = true;
                                return SetUsernames();
                            }
                        }
                    },
                    {
                        "2", new MenuItem()
                        {
                            Description = "Human starts",
                            CommandToExecute = () =>
                            {
                                _state.HumanPlayerCount = 1;
                                _state.ComputerMove = false;
                                return SetUsernames();
                            }
                        }
                    },
                    {
                        "3", new MenuItem()
                        {
                            Description = "Human vs. Human",
                            CommandToExecute = () =>
                            {
                                _state.HumanPlayerCount = 2;
                                _state.ComputerMove = false;
                                return SetUsernames();
                            }
                        }
                    },
                }
            };
            
            var mainMenu = new Menu(0)
            {
                MenuTitle = $"{_state.GameName} - Main Menu",
                MenuItemsDictionary = new Dictionary<string, MenuItem>()
                {
                    {
                        "1", new MenuItem()
                        {
                            Description = "Start a new game",
                            CommandToExecute = gameOptionMenu.Run
                        }
                    },
                    {
                        "2", new MenuItem()
                        {
                            Description = "Continue with a saved game",
                            CommandToExecute = savedGamesMenu.Run
                        }
                    },
                    {
                        "3", new MenuItem()
                        {
                            Description = "Add custom game settings",
                            CommandToExecute = SaveSettings
                        }
                    }
                }
            };
            
            mainMenu.Run();
        }

        // Get player name(s) from user(s) and swap the order, depending on who starts the game
        static string SetUsernames()
        {
            var exit = false;

            do
            {
                bool userCancelled;
                    
                (_state.Player1Name, userCancelled) =
                    Game.GetUserStringInput($"Please input the name for the player 1: ", 0);
                if (userCancelled) return "";

                if(_state.HumanPlayerCount == 2) {
                    (_state.Player2Name, userCancelled) = Game.GetUserStringInput($"Please input the name for the player 2: ", 
                        0);
                }
                if (userCancelled) return "";
                
                Game.SwapUserNames(_state);
                exit = true;
            } while (!exit);

            return GameRun(_state);
        }
        
        // For the very 1st run, put default game settings into DB
        // Load default settings into _state
       static void SetDefaultState()
        {
            using var context = new AppDbContext();
            // If there is no default settings in the DB, add
            if (!context.GameStates.Any(item => item.GameStateId == 1))
            {
                context.Add(Game.GetDefaultState());
                context.SaveChanges();
            }
                
            // Write default settings into _state
            _state = context.GameStates.First(item => item.GameStateId == 1);
        }
        
        // Load the game with the name provided
        public static GameState LoadSavedGame(string gameName)
        {
            GameState res;

            using (var ctx = new AppDbContext())
            {
                // Find game by the name selected
                res = ctx.GameStates.First(item => item.GameSaveName == gameName);
                
                // If nothing found, return new game
                if (res == null)
                {
                    res = ctx.GameStates.First(item => item.GameStateId == 1);
                    res.GameSaveName = $"No game found by the name {gameName}. The new game is started";
                }
            }

            return res;
        }
        
        // Run console gameflow
        public static string GameRun(GameState gameState)
        {
            var game = new Game(gameState);
            List<int> fullCols = new List<int>();
            string playerName = gameState.Player1Name;
            int moveResult = 0;

            var done = false;
            do
            {
                Console.Clear();
                GameUI.PrintBoard(game);

                var xIndexInput = 0;
                var menuCall = false;

                switch (moveResult, gameState.ComputerMove)
                {
                    case (1, false): Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine();
                        Console.WriteLine("This column is already full! Please try again.");
                        Console.ResetColor();
                        moveResult = 0;
                        break;
                    case (1, true): xIndexInput = game.GenerateRandomAiMove(fullCols);
                        break;
                    case(0, true): if (gameState.HumanPlayerCount == 1)
                        {
                            xIndexInput = game.GetAiMove(gameState, fullCols);
                        }
                        break;
                    default: (xIndexInput, done, menuCall) = Game.GetUserIntInput($"{playerName}'s turn: Enter X coordinate:", 
                        1, game.BoardWidth, 0, "S");
                        break;
                }

                if (done) break;

                if (menuCall)
                {
                    _state = gameState;
                    SaveGame(game);
                }
                else
                {
                    // 0 = all good, +1 = full column, +2 = full board, +4 = player won
                    moveResult = 
                        game.Move(xIndexInput - 1, gameState.WinningConditionSequenceLength, gameState.PlayerZeroMove);

                    switch (moveResult)
                    {
                        case 0: playerName = playerName == _state.Player1Name! ? _state.Player2Name! : _state.Player1Name!;
                            game.SwitchMove(gameState);
                            break;
                        case 1: ((IList) fullCols).Add(xIndexInput);
                            break;
                        default: 
                            // If the game is over, output the final board and set _state to default settings
                            Console.Clear();
                            GameUI.PrintBoard(game);
                            gameState.GameOver = true;
                            done = true;
                            break;
                    }
                }
            } while (!done);
            
            // Set _state to default settings
            SetDefaultState();
            
            Console.WriteLine("**************************");
            Console.WriteLine();
            Console.WriteLine(moveResult == 4 || moveResult == 6 ? $"{ playerName } won!" : 
                "        Game Over!        ");
            Console.WriteLine("**************************");
            Console.WriteLine();
            return "";
        }

        // Get the name for the game to save and put record into DB
        static void SaveGame(Game game)
        {
            Console.Clear();
            
            bool userCancelled;
            string gameName;
                
            (gameName, userCancelled) =
                Game.GetUserStringInput("Please input the name for the game to be saved: ", 0);

            if (userCancelled) return;
                
            var jsonBoard = JsonConvert.SerializeObject(game.GetBoard(), Settings);

            var newSave = new GameState()
            {
                BoardStateJson = jsonBoard,
                GameName = _state.GameName,
                SaveTime = DateTime.Now,
                GameSaveName = gameName,
                ComputerMove = _state.ComputerMove,
                PlayerZeroMove = _state.PlayerZeroMove,
                HumanPlayerCount = _state.HumanPlayerCount,
                BoardHeight = _state.BoardHeight,
                BoardWidth = _state.BoardWidth,
                Player1Name = _state.Player1Name,
                Player2Name = _state.Player2Name,
                WinningConditionSequenceLength = _state.WinningConditionSequenceLength
            };

            _state = newSave;

            using var ctx = new AppDbContext();
            InsertDataToDb(ctx);
        }
        
        // Get custom settings and update the default settings record in the DB
        static string SaveSettings()
        {
            Console.Clear();
                
            int boardWidth, boardHeight, winSequence;
            bool userCanceled, menuCall;
                
            (boardWidth, userCanceled, menuCall) = Game.GetUserIntInput("Enter board width", 4, 20, 0);
            if (userCanceled || menuCall) return "";

            (boardHeight, userCanceled, menuCall) = Game.GetUserIntInput("Enter board height", 4, 20, 0);
            if (userCanceled || menuCall) return "";
            
            (winSequence, userCanceled, menuCall) = Game.GetUserIntInput("Enter winning sequence length", 4, 6, 0);
            if (userCanceled || menuCall) return "";

            _state.GameStateId = 1;
            _state.BoardWidth = boardWidth;
            _state.BoardHeight = boardHeight;
            _state.WinningConditionSequenceLength = winSequence;
            _state.BoardStateJson = JsonConvert.SerializeObject(new CellState[_state.BoardHeight, _state.BoardWidth], Settings);

            using var ctx = new AppDbContext();
            InsertDataToDb(ctx);

            return "";
        }
        
        // If ID = 1, update default settings, else - add new DB record
        static void InsertDataToDb(AppDbContext ctx)
        {
            if (_state.GameStateId == 1)
            {
                var defSettings = ctx.GameStates.First(item => item.GameStateId == 1);
                defSettings.BoardHeight = _state.BoardHeight;
                defSettings.BoardWidth = _state.BoardWidth;
                defSettings.WinningConditionSequenceLength = _state.WinningConditionSequenceLength;
                defSettings.BoardStateJson = _state.BoardStateJson;
                ctx.GameStates.Update(defSettings);
            }
            else
            {
                ctx.GameStates.Add(_state);
            }
            
            ctx.SaveChanges();
        }
    }
}