namespace Revistone.Console.Image;

///<summary> Pixels are the base structure used by the ConsoleImage and ConsoleVideo classes. </summary>
public class ConsolePixel
{
    public char Character { get; set; }
    public ConsoleColour FGColour { get; set; }
    public ConsoleColour BGColour { get; set; }

    public ConsolePixel(char c, ConsoleColour fg, ConsoleColour bg)
    {
        Character = c;
        FGColour = fg;
        BGColour = bg;
    }

    public ConsolePixel(ConsolePixel pixel) : this(pixel.Character, pixel.FGColour, pixel.BGColour) { }
    public ConsolePixel() : this(' ', ConsoleColour.White, ConsoleColour.Black) {}
    public ConsolePixel(ConsoleColour fg, ConsoleColour bg) : this(' ', fg, bg) {}

    ///<summary> Set the characteristics of the pixel. </summary>
    public void Set(char character, ConsoleColour fgColour, ConsoleColour bgColour)
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
    public void SetForeground(ConsoleColour fgColour) { this.FGColour = fgColour; }
    ///<summary> Set the background colour of the pixel. </summary>
    public void SetBackground(ConsoleColour bgColour) { this.BGColour = bgColour; }
}