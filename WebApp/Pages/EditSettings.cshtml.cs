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
    public class EditSettingsModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditSettingsModel(AppDbContext context)
        {
            _context = context;
        }
        
        [BindProperty]
        public GameState GameState { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GameState = await _context.GameStates.FirstOrDefaultAsync(g => g.GameStateId == id);

            if (GameState == null)
            {
                return NotFound();
            }
            return Page();
        }
       
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            Game game = new Game(GameState);
            GameState.BoardStateJson = JsonConvert.SerializeObject(game.GetBoard());
            GameState.GameSaveName = "Default";
            GameState.Player1Name = "Player 1";
            GameState.Player2Name = "AI";

            _context.Attach(GameState).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GameStateExists(GameState.GameStateId))
                {
                    return NotFound();
                }
                else throw;
            }

            return RedirectToPage("/Index");
        }
        
        private bool GameStateExists(int id)
        {
            return _context.GameStates.Any(g => g.GameStateId == id);
        }
    }
}