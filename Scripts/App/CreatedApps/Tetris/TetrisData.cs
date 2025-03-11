namespace Revistone.Apps.Tetris;

public static class TetrisData
{
    public enum BlockType { I, L, J, S, T, Z, O, Null }

    static ConsoleColor[] blockColours = [
        ConsoleColor.Black,
        ConsoleColor.Cyan,
        ConsoleColor.DarkGreen,
        ConsoleColor.Blue,
        ConsoleColor.Green,
        ConsoleColor.Magenta,
        ConsoleColor.Red,
        ConsoleColor.Yellow,
        ConsoleColor.Gray,
    ];

    public static ConsoleColor GetBlockColour(int blockType)
    {
        return blockColours[blockType];
    }

    static int[][,] IRotData = [
        new int[,] { {0, 1, 0, 0}, {0, 1, 0, 0}, {0, 1, 0, 0}, {0, 1, 0, 0} },
        new int[,] { {0, 0, 0, 0}, {1, 1, 1, 1}, {0, 0, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {0, 0, 1, 0}, {0, 0, 1, 0}, {0, 0, 1, 0}, {0, 0, 1, 0} },
        new int[,] { {0, 0, 0, 0}, {0, 0, 0, 0}, {1, 1, 1, 1}, {0, 0, 0, 0} },
    ];

    static int[][,] LRotData = [
        new int[,] { {0, 1, 0, 0}, {0, 1, 0, 0}, {1, 1, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {1, 0, 0, 0}, {1, 1, 1, 0}, {0, 0, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {0, 1, 1, 0}, {0, 1, 0, 0}, {0, 1, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {0, 0, 0, 0}, {1, 1, 1, 0}, {0, 0, 1, 0}, {0, 0, 0, 0} },

    ];

    static int[][,] JRotData = [

        new int[,] { {1, 1, 0, 0}, {0, 1, 0, 0}, {0, 1, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {0, 0, 1, 0}, {1, 1, 1, 0}, {0, 0, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {0, 1, 0, 0}, {0, 1, 0, 0}, {0, 1, 1, 0}, {0, 0, 0, 0} },
        new int[,] { {0, 0, 0, 0}, {1, 1, 1, 0}, {1, 0, 0, 0}, {0, 0, 0, 0} },
    ];

    static int[][,] SRotData = [
        new int[,] { {1, 0, 0, 0}, {1, 1, 0, 0}, {0, 1, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {0, 1, 1, 0}, {1, 1, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {1, 0, 0, 0}, {1, 1, 0, 0}, {0, 1, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {0, 1, 1, 0}, {1, 1, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0} },

    ];

    static int[][,] TRotData = [
        new int[,] { {0, 1, 0, 0}, {1, 1, 0, 0}, {0, 1, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {0, 1, 0, 0}, {1, 1, 1, 0}, {0, 0, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {0, 1, 0, 0}, {0, 1, 1, 0}, {0, 1, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {0, 0, 0, 0}, {1, 1, 1, 0}, {0, 1, 0, 0}, {0, 0, 0, 0} },
    ];

    static int[][,] ZRotData = [
        new int[,] { {0, 1, 0, 0}, {1, 1, 0, 0}, {1, 0, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {1, 1, 0, 0}, {0, 1, 1, 0}, {0, 0, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {0, 1, 0, 0}, {1, 1, 0, 0}, {1, 0, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {1, 1, 0, 0}, {0, 1, 1, 0}, {0, 0, 0, 0}, {0, 0, 0, 0} },

    ];

    static int[][,] ORotData = [
        new int[,] { {1, 1, 0, 0}, {1, 1, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {1, 1, 0, 0}, {1, 1, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {1, 1, 0, 0}, {1, 1, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0} },
        new int[,] { {1, 1, 0, 0}, {1, 1, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0} },

    ];

    public static int[,] GetBlockData(BlockType type, int rotation)
    {
        return type switch
        {
            BlockType.I => IRotData[rotation],
            BlockType.L => LRotData[rotation],
            BlockType.J => JRotData[rotation],
            BlockType.S => SRotData[rotation],
            BlockType.T => TRotData[rotation],
            BlockType.Z => ZRotData[rotation],
            BlockType.O => ORotData[rotation],
            _ => new int[4, 4],
        };
    }
}