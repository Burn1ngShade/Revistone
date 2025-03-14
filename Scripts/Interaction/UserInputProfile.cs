using Revistone.Console;
using Revistone.Functions;
using Revistone.App;

using static Revistone.Functions.StringFunctions;
using OpenAI.Assistants;

namespace Revistone.Interaction;

/// <summary> The configuration of the requirements of user input. </summary>
public class UserInputProfile //really happy with this script atm
{
    public enum InputType { FullText, PartialText, DateOnly, Int, Float } //type of input
    public enum OutputFormat { NoOutput, Standard, Dump } //type of output [will add more at some point]

    //--- REQS ---

    public string inputFormat = "";
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
    public UserInputProfile(InputType[] validTypes, string inputFormat = "", string bannedChars = "", CapitalCasing caseRequirements = CapitalCasing.None,
    int charCount = -1, int wordCount = -1, bool canBeEmpty = false, CapitalCasing caseSettings = CapitalCasing.None,
    bool removeWhitespace = false, bool removeLeadingWhitespace = false, bool removeTrailingWhitespace = false,
    float numericMin = float.NegativeInfinity, float numericMax = float.PositiveInfinity, OutputFormat outputFormat = OutputFormat.Standard)
    {
        this.validInputTypes = validTypes;
        this.inputFormat = inputFormat;
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

    /// <summary> The configuration of the requirements of user input. </summary>
    public UserInputProfile(InputType validType, string inputFormat = "", string bannedChars = "", CapitalCasing caseRequirements = CapitalCasing.None,
    int charCount = -1, int wordCount = -1, bool canBeEmpty = false, CapitalCasing caseSettings = CapitalCasing.None,
    bool removeWhitespace = false, bool removeLeadingWhitespace = false, bool removeTrailingWhitespace = false,
    float numericMin = float.NegativeInfinity, float numericMax = float.PositiveInfinity, OutputFormat outputFormat = OutputFormat.Standard) :
    this([validType], inputFormat, bannedChars, caseRequirements, charCount, wordCount, canBeEmpty, caseSettings, removeWhitespace, removeLeadingWhitespace, removeTrailingWhitespace, numericMin, numericMax, outputFormat)
    { }

    /// <summary> The configuration of the requirements of user input. </summary>
    public UserInputProfile(string inputFormat = "", string bannedChars = "", CapitalCasing caseRequirements = CapitalCasing.None,
    int charCount = -1, int wordCount = -1, bool canBeEmpty = false, CapitalCasing caseSettings = CapitalCasing.None,
    bool removeWhitespace = false, bool removeLeadingWhitespace = false, bool removeTrailingWhitespace = false,
    float numericMin = float.NegativeInfinity, float numericMax = float.PositiveInfinity, OutputFormat outputFormat = OutputFormat.Standard) :
    this([], inputFormat, bannedChars, caseRequirements, charCount, wordCount, canBeEmpty, caseSettings, removeWhitespace, removeLeadingWhitespace, removeTrailingWhitespace, numericMin, numericMax, outputFormat)
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

        if (inputFormat != "" && !StringFunctions.InFormat(modInput, inputFormat)) errors.Add($"Input Does Not Meet Required Format: {inputFormat}!");
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

        ConsoleAction.SendConsoleMessage(new ConsoleLine("--- Input Invalid --- ", AppRegistry.activeApp.colourScheme.secondaryColour.Extend(22)),
        ConsoleAnimatedLine.AppTheme);
        for (int i = 0; i < errors.Count; i++)
        {
            ConsoleAction.SendConsoleMessage(new ConsoleLine($"{i + 1}. {errors[i]}", AppRegistry.activeApp.colourScheme.primaryColour));
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
