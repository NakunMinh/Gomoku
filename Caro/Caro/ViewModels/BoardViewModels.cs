using Caro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caro.ViewModels
{
    class BoardViewModels
    {
        public Board CurrentBoard { get; set; }
        public BoardViewModels()
        {
            CurrentBoard = new Board();
        }
    }
}
