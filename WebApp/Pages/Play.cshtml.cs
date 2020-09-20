using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL;
using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace WebApp.Pages
{
    public class PlayModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings { TypeNameAssemblyFormatHandling = 
            TypeNameAssemblyFormatHandling.Full };

        public PlayModel(AppDbContext context)
        {
            _context = context;
        }

        public Game Game { get; set; } = default!;
        public string? Warning { get; private set; }
        public CellState[,] Board { get; private set; } = default!;
        public int MoveResult { get; set; }
        [BindProperty] public GameState GameState { get; set; } = default!;
        
        List<int> fullCols = new List<int>();

        public async Task<IActionResult> OnGetAsync(int? gameId, string? warning, bool? gameOver)
        {
            GameState = await _context.GameStates.FirstOrDefaultAsync(g => g.GameStateId == gameId) ??
                        await _context.GameStates.FirstOrDefaultAsync(g => g.GameStateId == 1);
            
            Board = JsonConvert.DeserializeAnonymousType(GameState.BoardStateJson.Replace("[]", "[,]"), Board, _settings);
            Warning = warning;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? gameId, int? xIndex, int? toDo )
        {
            bool moveOver = false;
            string warning = "";
            
            GameState = await _context.GameStates.FirstOrDefaultAsync(g => g.GameStateId == gameId) ??
                        await _context.GameStates.FirstOrDefaultAsync(g => g.GameStateId == 1);
            Game = new Game(GameState);

            if (toDo == null)
            {
                if(xIndex == null) return RedirectToPage("./Play", new {gameId = GameState.GameStateId});
                
                do
                {
                    if (GameState.ComputerMove && GameState.HumanPlayerCount == 1)
                    {
                        xIndex = Game.GetAiMove(GameState, fullCols);
                        MoveResult = Game.Move(xIndex.Value - 1, GameState.WinningConditionSequenceLength,
                            GameState.PlayerZeroMove);
                    } else if (!GameState.ComputerMove)
                    {
                        MoveResult = Game.Move(xIndex.Value, GameState.WinningConditionSequenceLength,
                            GameState.PlayerZeroMove);
                    }

                    switch (MoveResult)
                    {
                        case 0:
                            Game.SwitchMove(GameState);
                            if(!GameState.ComputerMove) moveOver = true;
                            break;
                        case 1:
                            ((IList) fullCols).Add(xIndex.Value);
                            if (!GameState.ComputerMove)
                            {
                                warning = "This column is full! Please choose another one.";
                            }
                            break;
                        case 4: case 6:
                            warning = " won!";
                            moveOver = true;
                            GameState.GameOver = true;
                            break;
                        default:
                            warning = "The board is full. Game Over!";
                            moveOver = true;
                            GameState.GameOver = true;
                            break;
                    }
                } while (!moveOver);
            }

            GameState.BoardStateJson = JsonConvert.SerializeObject(Game.GetBoard(), _settings);
            _context.Attach(GameState).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GameStateExists(GameState.GameStateId)) return NotFound();
                throw;
            }

            if (toDo != null && GameState.GameOver)
            {
                _context.GameStates.Remove(GameState);
                await _context.SaveChangesAsync();
            }

            if (toDo != null) return toDo.Value switch
            {
                1 => RedirectToPage("Gamestates/Create"),
                2 => RedirectToPage("Gamestates/Index"),
                3 => RedirectToPage("Index")
            };
            
            return RedirectToPage("./Play", new {gameId = GameState.GameStateId, warning});
        }
        
        private bool GameStateExists(int id)
        {
            return _context.GameStates.Any(g => g.GameStateId == id);
        }
    }
}