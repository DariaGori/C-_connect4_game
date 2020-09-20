using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL;
using Domain;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

#pragma warning disable 1998

namespace WebApp.Pages_GameStates
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }
        
        [BindProperty]
        public GameState GameState { get; set; } = default!;
        [BindProperty] 
        public bool PlayAgainstHuman { get; set; }
        public GameState NewSave { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }
       
        public async Task<IActionResult> OnPostAsync()
        {
            NewSave = await _context.GameStates.FirstOrDefaultAsync(g => g.GameStateId == 1);
            NewSave.SaveTime = DateTime.Now;
            NewSave.GameStateId = _context.GameStates.OrderByDescending(g => g.GameStateId).First().GameStateId + 1;
            NewSave.GameSaveName = GameState.GameSaveName;
            NewSave.HumanPlayerCount = PlayAgainstHuman ? 2 : 1;
            NewSave.ComputerMove = GameState.ComputerMove;
            NewSave.Player1Name = GameState.Player1Name;
            NewSave.Player2Name = GameState.Player2Name;
            NewSave = Game.SwapUserNames(NewSave);

            if (NewSave.ComputerMove && NewSave.HumanPlayerCount != 2)
            {
                List<int> fullCols = new List<int>();
                Game game = new Game(NewSave);
                int xIndex = game.GetAiMove(NewSave, fullCols);
                int moveResult = game.Move(xIndex - 1, NewSave.WinningConditionSequenceLength, NewSave.PlayerZeroMove);
                NewSave = game.SwitchMove(NewSave);
                NewSave.BoardStateJson = JsonConvert.SerializeObject(game.GetBoard());
            }
            
            _context.GameStates.Add(NewSave);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Play", new {gameId = NewSave.GameStateId});
        }
    }
}
