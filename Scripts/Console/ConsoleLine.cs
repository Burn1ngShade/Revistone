using System.Globalization;
using Revistone.App.BaseApps;
using Revistone.Console.Image;
using Revistone.Functions;

namespace Revistone.Console;

/// <summary> Class pertaining all info for a line in the console. </summary>
public class ConsoleLine
{
    //--- NORMAL ---
    public string LineText { get; private set; }
    public ConsoleColour[] LineColour { get; private set; }
    public ConsoleColour[] LineBGColour { get; private set; }
    public bool Updated { get;  private set; } // if true, line needs to be updated on console display

    //--- CONSTRUCTORS ---

    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine(string lineText, ConsoleColour[] lineColour, ConsoleColour[] lineColourBG)
    {
        LineText = lineText;
        LineColour = lineColour;
        LineBGColour = lineColourBG;
        Updated = false;
    }
    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine() : this("", [ConsoleColour.White], [ConsoleColour.Black]) { }
    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine(string lineText) : this(lineText, [ConsoleColour.White], [ConsoleColour.Black]) { }
    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine(string lineText, ConsoleColour lineColour) : this(lineText, [lineColour], [ConsoleColour.Black]) { }
    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine(ConsoleLine consoleLine) : this(consoleLine.LineText, consoleLine.LineColour, consoleLine.LineBGColour) { }
    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine(string lineText, ConsoleColour[] lineColour) : this(lineText, lineColour, [ConsoleColour.Black]) { }
    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine(string lineText, ConsoleColour lineColour, ConsoleColour lineColourBG) : this(lineText, [lineColour], [lineColourBG]) { }

    //--- METHODS ---

    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(string lineText, ConsoleColour[] lineColour, ConsoleColour[] lineColourBG)
    {
        LineText = lineText;
        LineColour = lineColour;
        LineBGColour = lineColourBG;
        Updated = false;
    }

    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(ConsoleLine lineInfo) { Update(lineInfo.LineText, lineInfo.LineColour, lineInfo.LineBGColour); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(string lineText) { Update(lineText, LineColour); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(string lineText, ConsoleColour lineColour) { Update(lineText, [lineColour]); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(ConsoleColour[] lineColour) { Update(LineText, lineColour); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(string lineText, ConsoleColour[] lineColour) { Update(lineText, lineColour, [ConsoleColour.Black]); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(string lineText, ConsoleColour lineColour, ConsoleColour lineColourBG) { Update(lineText, [lineColour], [lineColourBG]); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(ConsoleColour[] lineColour, ConsoleColour[] lineColourBG) { Update(LineText, lineColour, lineColourBG); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(ConsoleColour lineColour, ConsoleColour lineColourBG) { Update(LineText, [lineColour], [lineColourBG]); }

    /// <summary> Marks ConsoleLine as updated (try to avoid). </summary>
    public void MarkAsUpToDate()
    {
        Updated = true;
    }

    /// <summary> Marks ConsoleLine as not updated. </summary>
    public void MarkForUpdate()
    {
        Updated = false;
    }

    /// <summary> Checks if lineText has length 0. </summary>
    public bool IsEmpty() { return LineText.Length == 0; }

    ///<summary> Ensures that FG and BG colour arrays are atleast aslong as text. </summary>
    public void Normalise()
    {
        if (LineText.Length > LineColour.Length) LineColour = LineColour.SetLength(LineText.Length);
        if (LineText.Length > LineBGColour.Length) LineBGColour = LineBGColour.SetLength(LineText.Length);
    } 

    //--- STATIC METHODS ---

    /// <summary> Inserts a ConsoleLine into another, overwritting overlapping chars and colours. </summary>
    public static ConsoleLine Overwrite(ConsoleLine baseLine, ConsoleLine overwriteLine, int overwriteIndex)
    {
        string s = baseLine.LineText;
        s += new string(' ', Math.Clamp(overwriteIndex + overwriteLine.LineText.Length - s.Length, 0, int.MaxValue));
        s = s.ReplaceAt(overwriteIndex, overwriteLine.LineText.Length, overwriteLine.LineText);

        ConsoleColour[] cl = baseLine.LineColour.SetLength(ConsoleColour.White, s.Length);
        for (int i = overwriteIndex; i < overwriteIndex + overwriteLine.LineText.Length; i++) cl[i] = overwriteLine.LineColour[i - overwriteIndex];

        ConsoleColour[] clbg = baseLine.LineBGColour.SetLength(ConsoleColour.Black, s.Length);
        for (int i = overwriteIndex; i < overwriteIndex + overwriteLine.LineText.Length; i++) clbg[i] = overwriteLine.LineBGColour[i - overwriteIndex];

        return new ConsoleLine(s, cl, clbg);
    }

    /// <summary> Removes invalid charchters, zero and double length charchters from a consoleLine, seperates emojis to prevent joining. </summary>
    public static ConsoleLine Clean(ConsoleLine baseLine)
    {
        string validLineText = "";
        bool allowEmojis = SettingsApp.GetValue("Show Emojis") == "Yes";

        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(baseLine.LineText);
        bool lastCharIsEmoji = false;
        while (enumerator.MoveNext())
        {
            string textElement = enumerator.GetTextElement();
            bool isEmoji = StringFunctions.IsEmoji(textElement);

            if (isEmoji && !allowEmojis) continue;

            if (textElement.Length == 1 && char.IsSurrogate(textElement[0]))
            {
                validLineText += enumerator.GetTextElement();
                continue;
            }

            int lineWidth = StringFunctions.GetCharWidth(textElement);

            if (textElement.Length == 2 && char.IsSurrogatePair(textElement[0], textElement[1]))
            {
                if (lineWidth == 2) validLineText += textElement;
                if (isEmoji && lastCharIsEmoji) validLineText += " ";
            }
            else if (lineWidth == 1)
            {
                validLineText += enumerator.GetTextElement();
                if (isEmoji && lastCharIsEmoji) validLineText += " ";
            }

            lastCharIsEmoji = isEmoji;
        }

        validLineText = validLineText.Replace("\n", "").Replace("\r", "");

        return new ConsoleLine(validLineText, baseLine.LineColour, baseLine.LineBGColour);
    } 
}
