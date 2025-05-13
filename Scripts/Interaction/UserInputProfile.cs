using Revistone.Console;
using Revistone.Functions;
using Revistone.App;

using static Revistone.Functions.StringFunctions;

namespace Revistone.Interaction;

/// <summary> The configuration of the requirements of user input. </summary>
public class UserInputProfile
{
    public enum InputType { FullText, PartialText, DateOnly, Int, Float } //type of input
    public enum OutputFormat { NoOutput, Standard, Dump } //type of output

    //--- REQS ---

    public string[] validInputFormats = [];
    public string bannedChars = "";

    public InputType[] validInputTypes; //type that input can be
    public CapitalCasing caseRequirements; //must match caps

    public int charCount;
    public int wordCount;

    public float numericMin;
    public float numericMax;

    public bool canBeEmpty;

    //--- MODIFICATIONS ---

    public CapitalCasing caseSettings; //changes caps to this

    public bool removeWhitespace; //remove spaces
    public bool removeLeadingWhitespace; //removes whitespace at start of word
    public bool removeTrailingWhitespace; //removes whitespace at start of word

    //--- OUTPUT ---

    public OutputFormat outputFormat; //prints mismatches

    //--- CONSTRUCTORS ---

    /// <summary> The configuration of the requirements of user input. </summary>
    public UserInputProfile(InputType[] validInputTypes, string[] validInputFormats, string bannedChars = "", CapitalCasing caseRequirements = CapitalCasing.None,
    int charCount = -1, int wordCount = -1, bool canBeEmpty = false, CapitalCasing caseSettings = CapitalCasing.None,
    bool removeWhitespace = false, bool removeLeadingWhitespace = false, bool removeTrailingWhitespace = false,
    float numericMin = float.NegativeInfinity, float numericMax = float.PositiveInfinity, OutputFormat outputFormat = OutputFormat.Standard)
    {
        this.validInputTypes = validInputTypes;
        this.validInputFormats = validInputFormats;
        this.bannedChars = bannedChars;
        this.caseRequirements = caseRequirements;
        this.charCount = charCount;
        this.wordCount = wordCount;
        this.canBeEmpty = canBeEmpty;
        this.caseSettings = caseSettings;
        this.removeWhitespace = removeWhitespace;
        this.removeLeadingWhitespace = removeLeadingWhitespace;
        this.removeTrailingWhitespace = removeTrailingWhitespace;
        this.numericMin = numericMin;
        this.numericMax = numericMax;
        this.outputFormat = outputFormat;
    }

    public UserInputProfile(InputType[] validInputTypes, string validInputFormat = "", string bannedChars = "", CapitalCasing caseRequirements = CapitalCasing.None,
    int charCount = -1, int wordCount = -1, bool canBeEmpty = false, CapitalCasing caseSettings = CapitalCasing.None,
    bool removeWhitespace = false, bool removeLeadingWhitespace = false, bool removeTrailingWhitespace = false,
    float numericMin = float.NegativeInfinity, float numericMax = float.PositiveInfinity, OutputFormat outputFormat = OutputFormat.Standard) :
    this(validInputTypes, validInputFormat.Length == 0 ? [] : [validInputFormat], bannedChars, caseRequirements, charCount, wordCount, canBeEmpty, caseSettings, removeWhitespace, removeLeadingWhitespace, removeTrailingWhitespace, numericMin, numericMax, outputFormat)
    { }

    /// <summary> The configuration of the requirements of user input. </summary>
    public UserInputProfile(InputType validType, string validInputFormat = "", string bannedChars = "", CapitalCasing caseRequirements = CapitalCasing.None,
    int charCount = -1, int wordCount = -1, bool canBeEmpty = false, CapitalCasing caseSettings = CapitalCasing.None,
    bool removeWhitespace = false, bool removeLeadingWhitespace = false, bool removeTrailingWhitespace = false,
    float numericMin = float.NegativeInfinity, float numericMax = float.PositiveInfinity, OutputFormat outputFormat = OutputFormat.Standard) :
    this([validType], validInputFormat.Length == 0 ? [] : [validInputFormat], bannedChars, caseRequirements, charCount, wordCount, canBeEmpty, caseSettings, removeWhitespace, removeLeadingWhitespace, removeTrailingWhitespace, numericMin, numericMax, outputFormat)
    { }

    /// <summary> The configuration of the requirements of user input. </summary>
    public UserInputProfile(string validInputFormat = "", string bannedChars = "", CapitalCasing caseRequirements = CapitalCasing.None,
    int charCount = -1, int wordCount = -1, bool canBeEmpty = false, CapitalCasing caseSettings = CapitalCasing.None,
    bool removeWhitespace = false, bool removeLeadingWhitespace = false, bool removeTrailingWhitespace = false,
    float numericMin = float.NegativeInfinity, float numericMax = float.PositiveInfinity, OutputFormat outputFormat = OutputFormat.Standard) :
    this([], validInputFormat.Length == 0 ? [] : [validInputFormat], bannedChars, caseRequirements, charCount, wordCount, canBeEmpty, caseSettings, removeWhitespace, removeLeadingWhitespace, removeTrailingWhitespace, numericMin, numericMax, outputFormat)
    { }

    /// <summary> The configuration of the requirements of user input. </summary>
    public UserInputProfile(string[] validInputFormats, string bannedChars = "", CapitalCasing caseRequirements = CapitalCasing.None,
    int charCount = -1, int wordCount = -1, bool canBeEmpty = false, CapitalCasing caseSettings = CapitalCasing.None,
    bool removeWhitespace = false, bool removeLeadingWhitespace = false, bool removeTrailingWhitespace = false,
    float numericMin = float.NegativeInfinity, float numericMax = float.PositiveInfinity, OutputFormat outputFormat = OutputFormat.Standard) :
    this([], validInputFormats, bannedChars, caseRequirements, charCount, wordCount, canBeEmpty, caseSettings, removeWhitespace, removeLeadingWhitespace, removeTrailingWhitespace, numericMin, numericMax, outputFormat)
    { }

    //--- FUNCTIONS ---

    /// <summary> Returns whether the given input under given modifications meets all requirements. </summary>
    public bool InputValid(string input)
    {
        List<string> errors = new List<string>();

        // Modifications

        string modInput = input;

        modInput = StringFunctions.AdjustCapitalisation(modInput, caseSettings);
        if (removeLeadingWhitespace) modInput = modInput.TrimStart();
        if (removeTrailingWhitespace) modInput = modInput.TrimEnd();
        if (removeWhitespace) modInput = modInput.Replace(" ", "");

        // Requirements

        InputType inputType = GetInputType(modInput);
        int words = modInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        if (validInputFormats.Length != 0)
        {
            bool foundFormat = false;
            foreach (string format in validInputFormats) {
                if (StringFunctions.InFormat(modInput, format)) { foundFormat = true; break; }
            }
            if (!foundFormat) errors.Add($"Input Does Not Meet Required Format: {validInputFormats.ToElementString()}!");
        }
        // if (validInputFormats != "" && !StringFunctions.InFormat(modInput, validInputFormats)) errors.Add($"Input Does Not Meet Required Format: {validInputFormats}!");
        char[] bannedChar = modInput.Where(c => bannedChars.Contains(c.ToString())).ToArray();
        if (bannedChar.Length > 0) errors.Add($"Input Can Not Have Characters: {bannedChar.ToElementString()}!");
        if (validInputTypes.Length != 0 && !validInputTypes.Contains(inputType)) errors.Add($"Input Recognised As [{inputType}], Should Be {validInputTypes.ToElementString()}!");
        if (!StringFunctions.MatchesCapitalisation(modInput, caseRequirements)) errors.Add($"Capitalisation Does Not Match Format {caseRequirements}!");
        if (!canBeEmpty && modInput.Length == 0) errors.Add("Input Can Not Be Empty!");
        if (charCount > 0 && modInput.Length != charCount) errors.Add($"Input To {(charCount > modInput.Length ? "Short" : "Long")} [{modInput.Length}], Expected Length [{charCount}]!");
        if (wordCount > 0 && words != wordCount) errors.Add($"Input Contans [{words}] Words, Expected Word Count [{wordCount}]!");
        if (numericMin != float.NegativeInfinity || numericMax != float.PositiveInfinity)
        {
            if (float.TryParse(modInput, out float numericValue))
            {
                if (numericMin > numericValue || numericMax < numericValue) errors.Add($"Input [{numericValue}] Is Not Within Range {numericMin} - {numericMax}!");
            }
            else
            {
                errors.Add($"Input Must Range From {numericMin} - {numericMax}, But Is Not Recognised As A Number!");
            }
        }

        // Output

        if (errors.Count == 0 || outputFormat == OutputFormat.NoOutput) return errors.Count == 0;

        ConsoleAction.SendConsoleMessage(new ConsoleLine("--- Input Invalid --- ", AppRegistry.SecondaryCol.Extend(22)));
        for (int i = 0; i < errors.Count; i++)
        {
            ConsoleAction.SendConsoleMessage(new ConsoleLine($"{i + 1}. {errors[i]}", AppRegistry.PrimaryCol));
        }

        if (outputFormat == OutputFormat.Standard)
        {
            UserInput.WaitForUserInput(space: true);
            ConsoleAction.ClearLines(errors.Count + 2, true);
        }

        return errors.Count == 0;
    }

    /// <summary> Returns type of given string. </summary>
    public static InputType GetInputType(string input) //not sure if to put this here or UserInput, here for now...
    {
        if (long.TryParse(input, out long r)) return InputType.Int;
        if (float.TryParse(input, out float r2)) return InputType.Float;

        if (DateOnly.TryParse(input, out DateOnly r4)) return InputType.DateOnly;

        for (int i = 0; i < input.Length; i++)
        {
            if (int.TryParse(input[i].ToString(), out int r3)) return InputType.PartialText;
        }

        return InputType.FullText;
    }
}
