using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Domain;
using Newtonsoft.Json;

namespace BLL
{
    public class Game
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings { TypeNameAssemblyFormatHandling = 
            TypeNameAssemblyFormatHandling.Full };
        public string GameName { get; set; }
        
        public int BoardWidth { get; }
        public int BoardHeight { get; }
        
        private CellState[,] Board { get;  set; }

        private bool _playerZeroMove;

        public Game(GameState state)
        {
            if (state.BoardHeight < 4 || state.BoardWidth < 4)
            {
                throw new ArgumentException("Board size has to be at least 4x4!");
            }
            
            BoardHeight = state.BoardHeight;
            BoardWidth = state.BoardWidth;

            GameName = state.GameName ?? "";
            
            Board = (state.BoardStateJson != null)? JsonConvert.DeserializeObject<CellState[,]>(state.BoardStateJson, _settings) : 
                new CellState[state.BoardHeight, state.BoardWidth];
        }
        
        public CellState[,] GetBoard()
        {
            var result = new CellState[BoardHeight, BoardWidth];
            Array.Copy(Board, result, Board.Length);
            return result;
        }

        // Make a move and check the move result
        // Check for full column: if full = 1, if no = 0
        // Add 4, if user won, add 2 to indicate, that the board is full
        public int Move(int posX, int winningSequence, bool playerZeroMove)
        {
            _playerZeroMove = playerZeroMove;
            int posY;
            var sum = (Board[0, posX] != CellState.Empty)? 1 : 0;

            for (posY = BoardHeight - 1; posY >= 0; posY--)
            {
                if (Board[posY, posX] == CellState.Empty)
                {
                    Board[posY, posX] = _playerZeroMove ? CellState.WhiteBall : CellState.BlackBall;
                    if (CheckForWin(posX, posY, Board[posY, posX], winningSequence-1))
                    {
                        sum += 4;
                        return sum;
                    }
                    break;
                }
            }
            
            sum += 2;
            
            for (posX = 0; posX < BoardWidth; posX++)
            {
                if (Board[0, posX] == CellState.Empty)
                {
                    // If any column isn't full, subtract 2 => the board isn't full
                    sum -= 2;
                    break;
                }
            }

            return sum;
        }

        // Check the move that's already made for win 
        public bool CheckForWin(int posX, int posY, CellState symbol, int winSequenceLength)
        {
            // Get starting and ending coordinates for lines to check for win
            int startX = posX - winSequenceLength;
            int endX = posX + winSequenceLength;
            int startY = posY - winSequenceLength;
            int endY = posY + winSequenceLength;

            // Check horizontal line for winning sequence
            int sum = 0;
            for (int i = (startX < 0 ? 0 : startX); i <= (endX > BoardWidth-1 ? BoardWidth-1 : endX); i++)
            {
                sum = (Board[posY, i] == symbol) ? (sum + 1) : 0;
                if (sum == winSequenceLength + 1)
                {
                    return true;
                }
            }

            // Check vertical line for winning sequence
            sum = 0;
            for (int i = startY < 0 ? 0 : startY; i <= (endY > BoardHeight-1 ? BoardHeight-1 : endY); i++)
            {
                sum = (Board[i, posX] == symbol) ? (sum + 1) : 0;
                if (sum == winSequenceLength+1)
                {
                    return true;
                }
            }

            sum = 0;
            // Get correction coefficients for main diagonal
            int correctionStart = Math.Max(GetMinCorrection(startX, 0), GetMinCorrection(startY, 0));
            int correctionEnd = Math.Max(GetMaxCorrection(endX, BoardWidth-1), GetMaxCorrection(endY, BoardHeight-1));

            int diagStartX = startX + correctionStart;
            int diagStartY = startY + correctionStart;
            int diagEndX = endX - correctionEnd;
            int diagEndY = endY - correctionEnd;
            for (int i = diagStartX, j = diagStartY; i <= diagEndX && j <= diagEndY; i++, j++)
            {
                sum = (Board[j, i] == symbol) ? (sum + 1) : 0;
                if (sum == winSequenceLength+1)
                {
                    return true;
                }
            }
            
            sum = 0;
            // Get correction coefficients for antidiagonal
            correctionStart = Math.Max(GetMinCorrection(startX, 0), GetMaxCorrection(endY, BoardHeight-1));
            correctionEnd = Math.Max(GetMaxCorrection(endX, BoardWidth-1), GetMinCorrection(startY, 0));
            startX += correctionStart;
            endY -= correctionStart;
            endX -= correctionEnd;
            startY += correctionEnd;
            for (int i = startX, j = endY; i <= endX && j >= startY; i++, j--)
            {
                sum = (Board[j, i] == symbol) ? (sum + 1) : 0;
                if (sum == winSequenceLength+1)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetAiMove(GameState gameState, List<int> excludedCols)
        {
            for (var i = 0; i < gameState.BoardWidth; i++)
            {
                for (var j = 0; j < gameState.BoardHeight; j++)
                {
                    bool hit;
                    int xIndexInput;
                    
                    (xIndexInput, hit) =
                        CheckForNextAiMove(j, i, gameState.WinningConditionSequenceLength - 1);
                    
                    if (hit) return xIndexInput;
                }
            }
            
            return GenerateRandomAiMove(excludedCols);
        }
        
        // Check for potentially winning move or prevent the one from the opponent
        public (int xIndex, bool putBall) CheckForNextAiMove(int initY, int initX, int winSequenceLength)
        {
            int xIndex;
            bool putBall;
            
            (xIndex, putBall) =
                CheckForNextAiMoveVert(initY, initX, winSequenceLength);
            if (putBall) return (xIndex, true);
            (xIndex, putBall) = 
                CheckForNextAiMoveHoriz(initY, initX, winSequenceLength);
            if (putBall) return (xIndex, true);
            (xIndex, putBall) =
                CheckFoNextAiMoveMainDiag(initY, initX, winSequenceLength);
            if (putBall) return (xIndex, true);
            (xIndex, putBall) =
                CheckForNextAiMoveAntiDiag(initY, initX, winSequenceLength);
            if (putBall) return (xIndex, true);
            return (xIndex, false);
        }

        public (int x, bool hit) CheckForNextAiMoveVert(int initY, int initX, int winSequenceLength)
        {
            int sum = 0;
            int emptyY = 100;
            
            if (initY + winSequenceLength <= BoardHeight - 1)
            {
                for (var i = initY; i <= initY + winSequenceLength; i++)
                {
                    sum += EvaluateCell(Board[i,initX]);
                    emptyY = (EvaluateCell(Board[i, initX]) == 0) ? i : emptyY;
                }
            }

            return CheckCellBelowTarget(emptyY, initX, sum, winSequenceLength);
        }

        public (int x, bool hit) CheckFoNextAiMoveMainDiag(int initY, int initX, int winSequenceLength)
        {
            int sum = 0, emptyX = 0;
            int emptyY = 100;

            if (initX + winSequenceLength <= BoardWidth - 1 && initY + winSequenceLength <= BoardHeight-1)
            {
                for (int i = initX, j = initY; i <= initX + winSequenceLength && j <= initY + winSequenceLength; i++, j++)
                {
                    sum += EvaluateCell(Board[j,i]);
                    if (EvaluateCell(Board[j, i]) == 0) (emptyY, emptyX) = (j, i);
                }
            }
            
            return CheckCellBelowTarget(emptyY, emptyX, sum, winSequenceLength);
        }
        
        public (int x, bool hit) CheckForNextAiMoveAntiDiag(int initY, int initX, int winSequenceLength)
        {
            int sum = 0, emptyX = 0;
            int emptyY = 100;

            if (initX + winSequenceLength <= BoardWidth - 1 && initY - winSequenceLength >= 0)
            {
                for (int i = initX, j = initY; i <= initX + winSequenceLength && j >= initY - winSequenceLength; i++, j--)
                {
                    sum += EvaluateCell(Board[j,i]);
                    if (EvaluateCell(Board[j, i]) == 0)
                    {
                        (emptyY, emptyX) = (j, i);
                    }
                }
            }

            return CheckCellBelowTarget(emptyY, emptyX, sum, winSequenceLength);
        }
        
        public (int x, bool hit) CheckForNextAiMoveHoriz(int initY, int initX, int winSequenceLength)
        {
            int sum = 0, emptyX = 0;

            if (initX + winSequenceLength <= BoardWidth - 1)
            {
                for (int i = initX; i <= initX + winSequenceLength; i++)
                {
                    sum += EvaluateCell(Board[initY,i]);
                    if (EvaluateCell(Board[initY, i]) == 0) emptyX = i;
                }
            }

            return CheckCellBelowTarget(initY, emptyX, sum, winSequenceLength);
        }

        // Check whether the cell under the next AI move is empty or full
        public (int target, bool putBall) CheckCellBelowTarget(int y, int x, int sum, int winSequenceLength)
        {
            if (y + 1 <= BoardHeight - 1)
            {
                if (EvaluateCell(Board[y + 1, x]) != 0)
                {
                    return (sum == winSequenceLength || sum == winSequenceLength * 10) ? (x + 1, true) : (0, false);
                }
                
                return (0, false);
            }
            return (sum == winSequenceLength || sum == winSequenceLength * 10) ? (x + 1, true) : (0, false);
        }
        
        public int EvaluateCell(CellState cellState)
        {
            switch (cellState)
            {
                case CellState.Empty:
                    return 0;
                case CellState.BlackBall:
                    return 1;
                case CellState.WhiteBall:
                    return 10;
                default:
                    throw new InvalidEnumArgumentException("Unknown item!");
            }
        }

        public int GenerateRandomAiMove(List<int> exclude)
        {
            Random random = new Random();
            int col; 
            
            do
            {
                col = random.Next(1, BoardWidth);
            } while (exclude.Contains(col));

            return col;
        }

        // Get correction for the start / end of diagonal lines
        public int GetMaxCorrection(int val, int border)
        {
            return val > border ? val - border : 0;
        }
        
        public int GetMinCorrection(int val, int border)
        {
            return val < border ? border - val : 0;
        }

        // Get and validate numeric input
        public static (int result, bool wasCanceled, bool menuCall) GetUserIntInput(string prompt, int min, int max, 
            int? exitIntValue = null, string? menuCallValue = null )
        {
            do
            {
                Console.WriteLine(prompt);
                
                if (exitIntValue.HasValue || !string.IsNullOrWhiteSpace(menuCallValue))
                {
                    Console.WriteLine($"*** To exit the input prompt press {exitIntValue}" +
                                      $"{ (exitIntValue.HasValue && !string.IsNullOrWhiteSpace(menuCallValue) ? ". To go to the inner menu press " : "") }" +
                                      $"{menuCallValue}");
                }

                Console.Write(">");
                
                var inputLine = Console.ReadLine();

                if (inputLine != null && (menuCallValue != null && inputLine.ToUpper() == menuCallValue)) return (1, false, true);
                
                if (int.TryParse(inputLine, out var userInt))
                {
                    if ((userInt > max || userInt < min) && userInt != exitIntValue)
                    {
                        Console.WriteLine($"{inputLine} is not a valid parameter! Please try again");
                    }
                    else
                    {
                        return userInt == exitIntValue ? (userInt, true, false) : (userInt, false, false);
                    }
                }
                else
                {
                    Console.WriteLine($"'{inputLine}' cannot be converted into a number! Please try again");
                }
            } while (true);
        }

        // Get and validate string input
        public static (string result, bool wasCanceled) GetUserStringInput(string prompt, int exitIntValue)
        {
            do
            {
                Console.WriteLine(prompt);
                
                Console.WriteLine($"*** To exit the input prompt press {exitIntValue}");

                Console.Write(">");
                
                var inputLine = Console.ReadLine();

                if (int.TryParse(inputLine, out var userInt))
                {
                    if (userInt == exitIntValue) return ("", true);
                }

                if (String.IsNullOrWhiteSpace(inputLine))
                {
                    Console.WriteLine("Please input non-empty string!");
                } 
                else if (inputLine.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) != -1)
                {
                    Console.WriteLine("Please try again avoiding the special characters: \\ , / , : , * , \" , < , > , |");
                }
                else
                {
                    return (inputLine, false);
                }
            } while (true);
        }

        public static GameState SwapUserNames(GameState state)
        {
            if (state.HumanPlayerCount == 1)
            {
                (state.Player1Name, state.Player2Name) = (state.ComputerMove) ? ("AI", state.Player1Name) : (state.Player1Name, "AI");
            }
            else
            {
                state.Player2Name = state.Player2Name;
            }

            return state;
        }

        public GameState SwitchMove(GameState state)
        {
            state.ComputerMove = !state.ComputerMove;
            state.PlayerZeroMove = !state.PlayerZeroMove;
            return state;
        }
        
        public string GetSingleState(CellState state)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            
            switch (state)
            {
                case CellState.Empty:
                    return " ";
                case CellState.BlackBall:
                    return "\u25ef";
                case CellState.WhiteBall:
                    return "\u2b24";
                default:
                    throw new InvalidEnumArgumentException("Unknown item!");
            }
        }
        
        // Get default settings for the game
        public static GameState GetDefaultState()
        {
            GameState state = new GameState()
            {
                GameStateId = 1,
                SaveTime = DateTime.Now,
                BoardHeight = 6,
                BoardWidth = 7,
                WinningConditionSequenceLength = 4,
                GameName = "Connect 4",
                GameSaveName = "Default",
                Player1Name = "Player 1",
                BoardStateJson = JsonConvert.SerializeObject(new CellState[6, 7])
            };
            
            return state;
        }
    }
}