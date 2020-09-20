using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Domain
{
    public class GameState
    {
        public int GameStateId { get; set; }
        
        [Display(Name = "Date")]
        public DateTime SaveTime { get; set; }
        
        [MaxLength(255)]
        public string GameName { get; set; } = "Connect 4";
        
        [MaxLength(50, ErrorMessage = "Max Length for {0} is {1}")]
        [MinLength(2, ErrorMessage = "Min Length for {0} is {1}")]
        [Display(Name = "Game Name")]
        public string GameSaveName { get; set; } = default!;
        
        [Display(Name = "Board Width")]
        [Range(4, 20, ErrorMessage = "The width of the board should be in the range {1} - {2}")]
        public int BoardWidth { get; set; }
        [Display(Name = "Board Height")]
        [Range(4, 20, ErrorMessage = "The height of the board should be in the range {1} - {2}")]
        public int BoardHeight { get; set; }
        
        public bool PlayerZeroMove { get; set; }
        
        [Display(Name = "AI Starts")]
        public bool ComputerMove { get; set; }
        
        [Display(Name = "Play Against Human")]
        public int HumanPlayerCount { get; set; }
        
        [Display(Name = "Amount of Balls in the Row to Win")]
        [Range(4, 6, ErrorMessage = "The number should be in the range {1} - {2}")]
        public int WinningConditionSequenceLength { get; set; }
        
        [MaxLength(50, ErrorMessage = "Max Length for {0} is {1}")]
        [MinLength(2, ErrorMessage = "Min Length for {0} is {1}")] 
        [Display(Name = "Player 1 Name")]
        public string Player1Name { get; set; } = default!;
        
        [MaxLength(50, ErrorMessage = "Max Length for {0} is {1}")]
        [MinLength(2, ErrorMessage = "Min Length for {0} is {1}")] 
        [Display(Name = "Player 2 Name")]
        public string? Player2Name { get; set; }
        
        public bool GameOver { get; set; }

        public string BoardStateJson { get; set; } = default!;
    }
}