using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caro.Properties;

namespace Caro.Models
{
    public class Board
    {
        public int _boardSize { get; set; }
        public CellValues[,] Cells { get; set; }
        public CellValues ActivePlayer { get; set; }
        /// <summary>
        /// ///////////////////////////////////////////////////////////////////////
        /// </summary>
        public event PlayerWinHandler OnPlayerWin;
        /// <summary>
        /// ///////////////////////////////////////////////////////////////////////
        /// </summary>
        public Board()
        {
            _boardSize = Settings.Default.BOARD_SIZE;
            Cells = new CellValues[_boardSize, _boardSize];
            ActivePlayer = CellValues.Player1;
        }
        public void PlayAt(int row, int col)
        {
            Cells[row, col] = ActivePlayer;
            // Check win state
            // Vertiacal check
            if (CountPlayerItem(row, col, 1, 0) >= 5
                || CountPlayerItem(row, col, 0, 1) >= 5
                || CountPlayerItem(row, col, 1, 1) >= 5
                || CountPlayerItem(row, col, 1, -1) >= 5)
            {
                if (OnPlayerWin != null)
                    OnPlayerWin(player: ActivePlayer);
                return;
            }
            if (ActivePlayer == CellValues.Player1)
                ActivePlayer = CellValues.Player2;
            else
            {
                ActivePlayer=CellValues.Player1;
            }

        }

        private bool IsInBoard(int row, int col)
        {
            return row >= 0 && row < _boardSize && col >= 0 && col < _boardSize;
        }

        public void AutoPlay()
        {
            
        }

        private int CountPlayerItem(int row, int col, int drow, int dcol)
        {
            int crow = row + drow;
            int ccol = col + dcol;
            int count = 1;
            while (IsInBoard(crow, ccol) && Cells[crow, ccol] == ActivePlayer)
            {
                count++;
                crow = crow + drow;
                ccol = ccol + dcol;
            }
            crow = row - drow;
            ccol = col - dcol;
            while (IsInBoard(crow, ccol) && Cells[crow, ccol] == ActivePlayer)
            {
                count++;
                crow = crow - drow;
                ccol = ccol - dcol;
            }
            return count;
        }

        
    }
    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// <param name="player"></param>
    public delegate void PlayerWinHandler(CellValues player);
    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////////
    /// </summary>
    public enum CellValues { None = 0, Player1 = 1, Player2 = 2 }
}
