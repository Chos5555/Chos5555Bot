using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class CompletedQuests
    {
        public int Id { get; set; }
        // Name of the game
        public string GameName { get; set; }
        // Score of completed quests
        public int QuestCount { get; set; } = 0;
    }
}
