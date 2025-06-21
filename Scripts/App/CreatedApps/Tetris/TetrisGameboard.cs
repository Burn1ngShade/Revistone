using Revistone.Console;
using Revistone.Console.Image;
using Revistone.Functions;
using static Revistone.App.BaseApps.Tetris.TetrisData;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;

namespace Revistone.App.BaseApps.Tetris;

public class TetrisGameboard
{
    int[,] data = new int[10, 20];

    public PieceData currentPiece;
    BlockType heldBlock = BlockType.Null;
    bool canUseHeldBlock = true;
    List<BlockType> pieceQueue = [];

    public int startTick = -1;
    public bool paused = false;
    public bool gameOver = false;
    public bool displayPreviewPiece = true;

    public bool running => !paused && !gameOver;


    int linesCleared = 0;
    int score = 0;

    public TetrisGameboard()
    {
        currentPiece = new PieceData(BlockType.I);
        GeneratePieceQueue();
        SpawnNextPiece();
        canUseHeldBlock = true;
    }

    public void Tick()
    {
        if (PieceIsColliding(currentPiece.type, currentPiece.rotation, currentPiece.x, currentPiece.y + 1))
        {
            AddCurrentPieceToBoard();
            RemoveLines();
            SpawnNextPiece();
            canUseHeldBlock = true;
        }
        else
        {
            currentPiece.y++;
        }
        Output();
    }

    private void RemoveLines()
    {
        int scoreMultiplier = 1;
        for (int y = 0; y < 20; y++)
        {
            if (Enumerable.Range(0, 10).All(x => data[x, y] != 0))
            {
                score += 100 * scoreMultiplier;
                scoreMultiplier++;
                linesCleared++;
                for (int y2 = y; y2 > 0; y2--)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        data[x, y2] = data[x, y2 - 1];
                    }
                }
            }
        }


    }

    public bool PieceIsColliding(BlockType type, int rotation, int pieceX, int pieceY)
    {
        int[,] pieceData = GetBlockData(type, rotation);

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                if (pieceData[x, y] == 1)
                {
                    if (pieceX + x < 0 || pieceX + x > 9 || pieceY + y < 0 || pieceY + y > 19 || data[pieceX + x, pieceY + y] != 0)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void ToggleHeldPiece()
    {
        if (!canUseHeldBlock) return;

        canUseHeldBlock = false;

        if (heldBlock == BlockType.Null)
        {
            heldBlock = currentPiece.type;
            SpawnNextPiece();
        }
        else
        {
            BlockType temp = heldBlock;
            heldBlock = currentPiece.type;
            currentPiece = new PieceData(temp);
        }

        Output();
    }

    public void Output()
    {
        GoToLine(11);

        int[,] outputData = (int[,])data.Clone();
        int[,] pieceData = GetBlockData(currentPiece.type, currentPiece.rotation);

        int previewPieceY = currentPiece.y;

        while (!PieceIsColliding(currentPiece.type, currentPiece.rotation, currentPiece.x, previewPieceY + 1))
        {
            previewPieceY++;
        }

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                if (pieceData[x, y] == 1)
                {
                    if (displayPreviewPiece) outputData[currentPiece.x + x, previewPieceY + y] = (int)BlockType.Null + 1;
                    outputData[currentPiece.x + x, currentPiece.y + y] = (int)currentPiece.type + 1;
                }
            }
        }

        (string text, int type)[] lines = [
            gameOver ? ($"{new string(' ', 7)}Game Over!{new string(' ', 29)}", 2) : paused ? ($"{new string(' ', 9)}Paused{new string(' ', 31)}", 2) : (new string(' ', 46), 0),
            ($"{new string(' ', 28)}Held (R):{new string(' ', 9)}", gameOver ? 2 : 0),
            (gameOver ? $"    Press Any Key To{new string(' ', 26)}" : new string(' ', 46), gameOver ? 3 : 1),
            (gameOver ? $"        Continue{new string(' ', 30)}" : new string(' ', 46), gameOver ? 3 : 1),
            (new string(' ', 46), gameOver ? 3 : 1),
            (new string(' ', 46), 1),
            (new string(' ', 46), 0),
            ($"{new string(' ', 28)}Next:{new string(' ', 13)}", 0),
            (new string(' ', 46), 1),
            (new string(' ', 46), 1),
            (new string(' ', 46), 1),
            (new string(' ', 46), 1),
            (new string(' ', 46), 1),
            (new string(' ', 46), 1),
            (new string(' ', 46), 1),
            (new string(' ', 46), 1),
            (new string(' ', 46), 0),
            ($"{new string(' ', 28)}Score: {score}{new string(' ', 11 - $"{score}".Length)}", 0),
            ($"{new string(' ', 28)}Lines: {linesCleared}{new string(' ', 11 - $"{linesCleared}".Length)}", 0),
            (new string(' ', 46), 0),
        ];

        for (int y = 0; y < 20; y++)
        {
            (string text, int type) = lines[y];

            switch (type)
            {
                case 0:
                    SendConsoleMessage(new ConsoleLine(text, ConsoleColour.DarkBlue.ToArray(), BuildArray(ConsoleColour.DarkGray.ToArray(2),
                            Enumerable.Range(0, 10).Select(x => GetBlockColour(outputData[x, y])).ToArray().Stretch(2),
                            ConsoleColour.DarkGray.ToArray(2))));
                    break;
                case 1:
                    int[,] bd = new int[4, 4];
                    if (y >= 3 && y <= 5)
                    {
                        bd = GetBlockData(heldBlock, 0);

                        SendConsoleMessage(new ConsoleLine(text, ConsoleColour.DarkBlue.ToArray(), BuildArray(ConsoleColour.DarkGray.ToArray(2),
                            Enumerable.Range(0, 10).Select(x => GetBlockColour(outputData[x, y])).ToArray().Stretch(2),
                            ConsoleColour.DarkGray.ToArray(6), ConsoleColour.Black.ToArray(2),
                            Enumerable.Range(0, 4).Select(x => bd[x, y - 3] == 1 ? GetBlockColour((int)heldBlock + 1) : ConsoleColour.Black).ToArray().Stretch(2),
                            ConsoleColour.Black.ToArray(2), ConsoleColour.DarkGray.ToArray(4))));
                        break;
                    }

                    if (y >= 9 && y <= 11)
                    {
                        bd = GetBlockData(pieceQueue[0], 0);

                        SendConsoleMessage(new ConsoleLine(text, ConsoleColour.DarkBlue.ToArray(), BuildArray(ConsoleColour.DarkGray.ToArray(2),
                            Enumerable.Range(0, 10).Select(x => GetBlockColour(outputData[x, y])).ToArray().Stretch(2),
                            ConsoleColour.DarkGray.ToArray(6), ConsoleColour.Black.ToArray(2),
                            Enumerable.Range(0, 4).Select(x => bd[x, y - 9] == 1 ? GetBlockColour((int)pieceQueue[0] + 1) : ConsoleColour.Black).ToArray().Stretch(2),
                            ConsoleColour.Black.ToArray(2), ConsoleColour.DarkGray.ToArray(4))));
                        break;
                    }

                    if (y >= 13 && y <= 15)
                    {
                        bd = GetBlockData(pieceQueue[1], 0);

                        SendConsoleMessage(new ConsoleLine(text, ConsoleColour.DarkBlue.ToArray(), BuildArray(ConsoleColour.DarkGray.ToArray(2),
                            Enumerable.Range(0, 10).Select(x => GetBlockColour(outputData[x, y])).ToArray().Stretch(2),
                            ConsoleColour.DarkGray.ToArray(6), ConsoleColour.Black.ToArray(2),
                            Enumerable.Range(0, 4).Select(x => bd[x, y - 13] == 1 ? GetBlockColour((int)pieceQueue[1] + 1) : ConsoleColour.Black).ToArray().Stretch(2),
                            ConsoleColour.Black.ToArray(2), ConsoleColour.DarkGray.ToArray(4))));
                        break;
                    }



                    SendConsoleMessage(new ConsoleLine(text, ConsoleColour.DarkBlue.ToArray(), BuildArray(ConsoleColour.DarkGray.ToArray(2),
                            Enumerable.Range(0, 10).Select(x => GetBlockColour(outputData[x, y])).ToArray().Stretch(2),
                            ConsoleColour.DarkGray.ToArray(6),
                            ConsoleColour.Black.ToArray(12),
                            ConsoleColour.DarkGray.ToArray(6))));
                    break;
                case 2:
                    SendConsoleMessage(new ConsoleLine(text, ConsoleColour.DarkBlue.ToArray(), ConsoleColour.DarkGray.ToArray()));
                    break;
                case 3:
                    bd = GetBlockData(heldBlock, 0);
                    SendConsoleMessage(new ConsoleLine(text, ConsoleColour.DarkBlue.ToArray(), BuildArray(ConsoleColour.DarkGray.ToArray(28),
                    ConsoleColour.Black.ToArray(2), y >= 3 ? Enumerable.Range(0, 4).Select(x => bd[x, y - 3] == 1 ? GetBlockColour((int)heldBlock + 1) : ConsoleColour.Black).ToArray().Stretch(2) : ConsoleColour.Black.ToArray(8),
                    ConsoleColour.Black.ToArray(2), ConsoleColour.DarkGray.ToArray())));
                    break;
            }
        }
        SendConsoleMessage(new ConsoleLine(new string(' ', 46), ConsoleColour.White, ConsoleColour.DarkGray));
    }


    // --- PRIVATE METHODS ---

    void AddCurrentPieceToBoard()
    {
        int[,] pieceData = GetBlockData(currentPiece.type, currentPiece.rotation);

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                if (pieceData[x, y] == 1)
                {
                    data[currentPiece.x + x, currentPiece.y + y] = (int)currentPiece.type + 1;
                }
            }
        }
    }

    void GeneratePieceQueue()
    {
        pieceQueue.AddRange(Enum.GetValues<BlockType>().Take(7).OrderBy(x => Management.Manager.rng.Next()));
    }

    void SpawnNextPiece()
    {
        currentPiece = new PieceData(pieceQueue[0]);

        while (currentPiece.y > 0)
        {
            if (PieceIsColliding(currentPiece.type, currentPiece.rotation, currentPiece.x, currentPiece.y))
            {
                currentPiece.y--;
            }
        }

        if (PieceIsColliding(currentPiece.type, currentPiece.rotation, currentPiece.x, currentPiece.y))
        {
            EndGame();
            return;
        }

        pieceQueue.RemoveAt(0);

        if (pieceQueue.Count == 2) GeneratePieceQueue();
    }

    internal void TogglePause()
    {
        paused = !paused;
        Output();
    }

    internal void EndGame()
    {
        gameOver = true;
        Output();
    }

    // --- STRUCTS ---

    public struct PieceData(BlockType type)
    {
        public BlockType type = type;
        public int x = 3, y = 0;
        public int rotation = 0;

        public int Rotate(bool right)
        {
            int newRotation = rotation + (right ? -1 : 1);
            if (newRotation < 0) newRotation = 3;
            if (newRotation > 3) newRotation = 0;

            return newRotation;
        }
    }
}