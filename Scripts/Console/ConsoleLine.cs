using System.Globalization;
using Revistone.App.BaseApps;
using Revistone.Functions;

namespace Revistone.Console;

/// <summary> Class pertaining all info for a line in the console. </summary>
public class ConsoleLine
{
    //--- NORMAL ---
    public string lineText { get { return _lineText; } }
    string _lineText;

    public ConsoleColor[] lineColour { get { return _lineColour; } }
    ConsoleColor[] _lineColour;

    public ConsoleColor[] lineBGColour { get { return _lineBGColour; } }
    ConsoleColor[] _lineBGColour;

    public bool updated { get { return _updated; } }
    bool _updated;

    //--- CONSTRUCTORS ---

    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine(string lineText, ConsoleColor[] lineColour, ConsoleColor[] lineColourBG)
    {
        this._lineText = lineText;
        this._lineColour = lineColour;
        this._lineBGColour = lineColourBG;
        this._updated = false;
    }
    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine() : this("", new ConsoleColor[] { ConsoleColor.White }) { }
    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine(string lineText) : this(lineText, new ConsoleColor[] { ConsoleColor.White }) { }
    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine(string lineText, ConsoleColor lineColour) : this(lineText, new ConsoleColor[] { lineColour }) { }
    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine(ConsoleLine consoleLine) : this(consoleLine.lineText, consoleLine.lineColour, consoleLine.lineBGColour) { }
    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine(string lineText, ConsoleColor[] lineColour) : this(lineText, lineColour, new ConsoleColor[] { ConsoleColor.Black }) { }
    /// <summary> Class pertaining all info for a line in the console. </summary>
    public ConsoleLine(string lineText, ConsoleColor lineColour, ConsoleColor lineColourBG) : this(lineText, new ConsoleColor[] { lineColour }, new ConsoleColor[] { lineColourBG }) { }

    //--- METHODS ---

    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(string lineText, ConsoleColor[] lineColour, ConsoleColor[] lineColourBG)
    {
        // lock (Management.Manager.renderLockObject) { }
        this._lineText = lineText;
        this._lineColour = lineColour;
        this._lineBGColour = lineColourBG;
        this._updated = false;
    }

    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(ConsoleLine lineInfo) { Update(lineInfo.lineText, lineInfo.lineColour, lineInfo.lineBGColour); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(string lineText) { Update(lineText, _lineColour); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(string lineText, ConsoleColor lineColour) { Update(lineText, [lineColour]); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(ConsoleColor[] lineColour) { Update(lineText, lineColour); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(string lineText, ConsoleColor[] lineColour) { Update(lineText, lineColour, [ConsoleColor.Black]); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(string lineText, ConsoleColor lineColour, ConsoleColor lineColourBG) { Update(lineText, [lineColour], [lineColourBG]); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(ConsoleColor[] lineColour, ConsoleColor[] lineColourBG) { Update(lineText, lineColour, lineColourBG); }
    /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
    public void Update(ConsoleColor lineColour, ConsoleColor lineColourBG) { Update(lineText, [lineColour], [lineColourBG]); }

    /// <summary> Marks ConsoleLine as updated (try to avoid). </summary>
    public void MarkAsUpToDate()
    {
        this._updated = true;
    }

    /// <summary> Marks ConsoleLine as not updated. </summary>
    public void MarkForUpdate()
    {
        this._updated = false;
    }

    /// <summary> Checks if lineText has length 0. </summary>
    public bool IsEmpty() { return lineText.Length == 0; }

    ///<summary> Ensures that FG and BG colour arrays are atleast aslong as text. </summary>
    public void Normalise()
    {
        if (_lineText.Length > _lineColour.Length) _lineColour = _lineColour.ExtendEnd(_lineText.Length);
        if (_lineText.Length > _lineBGColour.Length) _lineBGColour = _lineBGColour.ExtendEnd(_lineText.Length);
    } 

    //--- STATIC METHODS ---

    /// <summary> Inserts a ConsoleLine into another, overwritting overlapping chars and colours. </summary>
    public static ConsoleLine Overwrite(ConsoleLine baseLine, ConsoleLine overwriteLine, int overwriteIndex)
    {
        string s = baseLine.lineText;
        s += new string(' ', Math.Clamp(overwriteIndex + overwriteLine.lineText.Length - s.Length, 0, int.MaxValue));
        s = s.ReplaceAt(overwriteIndex, overwriteLine.lineText.Length, overwriteLine.lineText);

        ConsoleColor[] cl = baseLine.lineColour.Extend(ConsoleColor.White, s.Length);
        for (int i = overwriteIndex; i < overwriteIndex + overwriteLine.lineText.Length; i++) cl[i] = overwriteLine.lineColour[i - overwriteIndex];

        ConsoleColor[] clbg = baseLine.lineBGColour.Extend(ConsoleColor.Black, s.Length);
        for (int i = overwriteIndex; i < overwriteIndex + overwriteLine.lineText.Length; i++) clbg[i] = overwriteLine.lineBGColour[i - overwriteIndex];

        return new ConsoleLine(s, cl, clbg);
    }

    /// <summary> Removes invalid charchters, zero and double length charchters from a consoleLine, seperates emojis to prevent joining. </summary>
    public static ConsoleLine Clean(ConsoleLine baseLine)
    {
        string validLineText = "";
        bool allowEmojis = SettingsApp.GetValue("Show Emojis") == "Yes";

        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(baseLine.lineText);
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

        return new ConsoleLine(validLineText, baseLine.lineColour, baseLine.lineBGColour);
    } 
}
