using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL;
using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
#pragma warning disable 1998

namespace WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty] public GameState GameState { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int toDo)
        {
            if (await _context.GameStates.FirstOrDefaultAsync(g => g.GameStateId == 1) == null)
            {
                GameState = Game.GetDefaultState();
                
                _context.GameStates.Add(GameState);
                await _context.SaveChangesAsync();
            }

            switch (toDo)
            {
                case 1: return RedirectToPage("/Gamestates/Create", new {id = 1});
                case 2: return RedirectToPage("/Gamestates/Index");
                case 3: return RedirectToPage("/EditSettings", new {id = 1});
            }
            return Page();
        }
    }
}