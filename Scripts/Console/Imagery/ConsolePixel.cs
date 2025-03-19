namespace Revistone.Console.Image;

///<summary> Pixels are the base structure used by the ConsoleImage and ConsoleVideo classes. </summary>
public class ConsolePixel(char c, ConsoleColor fg = ConsoleColor.White, ConsoleColor bg = ConsoleColor.Black)
{
    public char Character { get; set; } = c;
    public ConsoleColor FGColour { get; set; } = fg;
    public ConsoleColor BGColour { get; set; } = bg;

    public ConsolePixel(ConsolePixel pixel) : this(pixel.Character, pixel.FGColour, pixel.BGColour) { }
    public ConsolePixel() : this(' ') {}
    public ConsolePixel(ConsoleColor fg = ConsoleColor.White, ConsoleColor bg = ConsoleColor.Black) : this(' ', fg, bg) {}

    ///<summary> Set the characteristics of the pixel. </summary>
    public void Set(char character, ConsoleColor fgColour, ConsoleColor bgColour)
    {
        this.Character = character;
        this.FGColour = fgColour;
        this.BGColour = bgColour;
    }

    ///<summary> Set the characteristics of the pixel. </summary>
    public void Set(ConsolePixel pixel) { Set(pixel.Character, pixel.FGColour, pixel.BGColour); }

    ///<summary> Set the character of the pixel. </summary>
    public void SetChar(char character) { this.Character = character; }
    ///<summary> Set the foreground colour of the pixel. </summary>
    public void SetForeground(ConsoleColor fgColour) { this.FGColour = fgColour; }
    ///<summary> Set the background colour of the pixel. </summary>
    public void SetBackground(ConsoleColor bgColour) { this.BGColour = bgColour; }
}