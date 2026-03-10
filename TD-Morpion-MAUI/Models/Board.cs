namespace TD_Morpion_MAUI.Models;

public class Board
{
    private char[,] _cells;
    private int[][] _winningCombinations = new int[][]
    {
        new int[] { 0, 1, 2 }, // Ligne 1
        new int[] { 3, 4, 5 }, // Ligne 2
        new int[] { 6, 7, 8 }, // Ligne 3
        new int[] { 0, 3, 6 }, // Colonne 1
        new int[] { 1, 4, 7 }, // Colonne 2
        new int[] { 2, 5, 8 }, // Colonne 3
        new int[] { 0, 4, 8 }, // Diagonale \
        new int[] { 2, 4, 6 }  // Diagonale /
    };

    public Board()
    {
        _cells = new char[3, 3];
        Initialize();
    }

    public void Initialize()
    {
        for (int line = 0; line < 3; line++)
        {
            for (int column = 0; column < 3; column++)
            {
                _cells[line, column] = ' ';
            }
        }
    }

    public (int line, int column)? IsValidMove(int position)
    {
        if (position < 1 || position > 9) return null;

        int line = (position - 1) / 3;
        int column = (position - 1) % 3;

        char cell = _cells[line, column];
        if (cell == 'X' || cell == 'O') return null;

        return (line, column);
    }

    public void PlayMove(int line, int column, char player)
    {
        _cells[line, column] = player;
    }

    public bool CheckWin(char player)
    {
        return _winningCombinations.Any(combination =>
            combination.All(position => GetCellAt(position) == player)
        );
    }

    private char GetCellAt(int position)
    {
        int line = position / 3;
        int column = position % 3;
        return _cells[line, column];
    }

    public bool IsFull()
    {
        return _cells.Cast<char>().All(cell => cell == 'X' || cell == 'O');
    }
}
