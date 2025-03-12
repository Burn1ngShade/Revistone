using Revistone.Console;
using Revistone.Functions;
using Revistone.Interaction;

using static Revistone.Console.ConsoleAction;
using static Revistone.Interaction.UserInput;
using static Revistone.Interaction.UserInputProfile;
using static Revistone.Functions.ColourFunctions;

namespace Revistone.App;

public class DebitCardApp : App
{
    // --- APP BOILER PLATE ---
    public DebitCardApp() : base() { }
    public DebitCardApp(string name, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return new DebitCardApp[] { new DebitCardApp("Debit Card Manager", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Magenta.ToArray(), ConsoleColor.Blue.ToArray(), 10), (Alternate(DarkBlueAndMagenta.Flip(), 6, 3), 5),
                new (UserInputProfile format, Action<string> payload, string summary)[] {}, 94) };
    }

    public override void OnAppInitalisation()
    {
        DebitCard.accountID = int.TryParse(AppPersistentData.LoadFile("DebitCard/Data", 0), out int i) ? i : 0;
        DebitCard.LoadDataBase();
        MainMenu();
    }

    // --- MAIN MENU OPTIONS ---

    /// <summary> Main loop for debit card manager.</summary>
    void MainMenu()
    {
        ClearPrimaryConsole();

        ShiftLine();
        ConsoleLine[] title = TitleFunctions.CreateTitle("Honey Bank", ColourFunctions.Extend(CyanDarkMagentaGradient, 93, true), TitleFunctions.AsciiFont.BigMoneyNW);
        SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, "", 10, true), title.Length).ToArray());

        CreateOptionMenu($"Options", new (string, Action)[] { ("Access Debit Card", AccessCard), ("Create Debit Card", CreateCard),
                ("View All Accounts", ViewAllCards), ("Verify Long Card Number", ValidateCardNumber), ("Exit", ExitApp) }, true);

        if (AppRegistry.activeApp.name != "Debit Card Manager") return;
        MainMenu();
    }

    /// <summary> Menu to access a card.</summary>
    void AccessCard()
    {
        //way to easily stop dynamic line 
        string accountNumber = GetValidUserInput(new ConsoleLine("Account Number [Last 4 Digits Of The Card Number]: ", ConsoleColor.DarkBlue), new UserInputProfile(InputType.Int, charCount: 4));
        for (int i = 0; i < DebitCard.db.Count; i++)
        {
            if (accountNumber != DebitCard.db[i].accountNumber.Substring(12)) continue;

            SendConsoleMessage(new ConsoleLine($"Account With The Number {accountNumber} Found, Owner {DebitCard.db[i].initals}!", ConsoleColor.DarkBlue));

            WaitForUserInput(space: true);

            ClearPrimaryConsole();

            DateOnly d = DateOnly.Parse(GetValidUserInput(new ConsoleLine("DOB [dd/mm/yyyy]: ", ConsoleColor.DarkBlue), new UserInputProfile(InputType.DateOnly)));
            if (d != DebitCard.db[i].dob)
            {
                SendConsoleMessage("DOB Incorrect!");
                WaitForUserInput(space: true);
                return;
            }

            int p = int.Parse(GetValidUserInput(new ConsoleLine("Pin: ", ConsoleColor.DarkBlue), new UserInputProfile(InputType.Int, charCount: 4)));
            if (p != DebitCard.db[i].decryptedPin)
            {
                SendConsoleMessage("Pin Incorrect!");
                WaitForUserInput(space: true);
                return;
            }

            CardMenu(DebitCard.db[i]);
            ClearPrimaryConsole();
            return;
        }

        SendConsoleMessage($"No Account With The Number {accountNumber} Found!"); ;
        WaitForUserInput(space: true);
        return;
    }

    /// <summary> Logic for creating a card.</summary>
    void CreateCard()
    {
        string dob = GetValidUserInput(new ConsoleLine("DOB [dd/mm/yyyy]: ", ConsoleColor.DarkBlue), new UserInputProfile(InputType.DateOnly));
        DateOnly dobCheck = DateOnly.FromDateTime(DateTime.Now).AddYears(-30);
        if (DateOnly.Parse(dob) > dobCheck)
        {
            dobCheck = DateOnly.Parse(dob).AddYears(30);
        }
        else
        {
            SendConsoleMessage(new ConsoleLine("You Are Over 30 Years Old! You Can't Create An Account! ", ConsoleColor.Red), new ConsoleLineUpdate());
            WaitForUserInput(space: true);
            return;
        }

        string name = GetValidUserInput(new ConsoleLine("Enter Your Full Name: ", ConsoleColor.DarkBlue), new UserInputProfile(InputType.FullText, wordCount: 2));
        int pin = int.Parse(GetValidUserInput(new ConsoleLine("Create A Pin [4 Digits]: ", ConsoleColor.DarkBlue), new UserInputProfile(InputType.Int, charCount: 4)));
        bool masterCard = CreateOptionMenu("Card Type: ", new string[] { "Visa", "Mastercard" }, true) == 0 ? false : true;
        bool contactless = CreateTrueFalseOptionMenu("Contactless: ");

        ConsoleColor[][] c = new ConsoleColor[][] {
                ConsoleColor.White.ToArray(), ConsoleColor.Black.ToArray(), RedGradient, BlueGradient, GreenGradient, MagentaGradient,CyanDarkBlueGradient, GreenAndBlue};

        int colourScheme = CreateOptionMenu("Colour Scheme: ", new string[] {
                    "White", "Black", "Red", "Blue", "Green", "Magenta", "Cyan To Blue", "Green And Blue" }, true);
        List<ConsoleColor> colours = c[colourScheme].ToList();

        int accountType = CreateOptionMenu("Account Type: ", new string[] { "Standard", "Premier", "Gold", "Ruby" }, true);

        DebitCard d = new DebitCard(name, DateOnly.Parse(dob), dobCheck, pin, colours, masterCard, contactless, (DebitCard.AccountType)accountType);
        DebitCard.db.Add(d);
        DebitCard.SaveDataBase();

        ClearPrimaryConsole();

        string s = $"Congrats On Your New Honeycomb [{(DebitCard.AccountType)accountType} Account] {d.holderName}!";
        SendConsoleMessage(new ConsoleLine(s, AdvancedHighlight(s, ConsoleColor.White.ToArray(), DebitCard.accountColourLookup[accountType].ToArray(), 5, 6)));
        ShiftLine();
        d.DisplayCard();
        WaitForUserInput(space: true);
    }

    /// <summary> Logic for displaying all cards.</summary>
    void ViewAllCards()
    {
        if (DebitCard.db.Count == 0) return;

        int pointer = 0;

        while (true)
        {
            ClearPrimaryConsole();

            SendConsoleMessage($"Account [{pointer + 1} / {DebitCard.db.Count}]");
            ShiftLine();
            DebitCard.db[pointer].DisplayCard(true);
            ShiftLine();

            int option = CreateOptionMenu("Options: ", new string[] { "Next Card", "Previous Card", "Exit" });

            //use to be switch but this looks more consicse to me and performance is a non issue
            if (option == 0) pointer = pointer < DebitCard.db.Count - 1 ? pointer + 1 : 0;
            else if (option == 1) pointer = pointer > 0 ? pointer - 1 : DebitCard.db.Count - 1;
            else return;
        }
    }

    /// <summary> Takes card number and validates according to Luhn code.</summary>
    void ValidateCardNumber()
    {
        string card = GetValidUserInput(new ConsoleLine("Enter A Card Number", ConsoleColor.DarkBlue), new UserInputProfile(InputType.Int, charCount: 16, removeWhitespace: true));

        card = card.Replace(" ", "");
        int[] digits = card.Select(c => int.TryParse(c.ToString(), out int result) ? result : 0).ToArray();
        if (IsValidLuhn(digits)) SendConsoleMessage(new ConsoleLine("Debit Card Number Is Valid!", ConsoleColor.Green));
        else SendConsoleMessage(new ConsoleLine("Debit Card Number Is NOT Valid!", ConsoleColor.Red));

        WaitForUserInput(ConsoleKey.Enter, true, true);
    }

    // --- MENU OPTIONS ---

    /// <summary> Menu to edit and view specific card.</summary>
    void CardMenu(DebitCard c)
    {
        ClearPrimaryConsole();
        string s = $"Welcome Back To Your [{c.accountType} Account] {c.holderName}!";
        string funds = $"You Currently Have £{Math.Round(c.funds, 2)}";

        SendConsoleMessage(new ConsoleLine(s, AdvancedHighlight(s, ConsoleColor.White.ToArray(), DebitCard.accountColourLookup[(int)c.accountType].ToArray(), 4, 5)));
        SendConsoleMessage(funds);

        ShiftLine();
        c.DisplayCard();
        ShiftLine();

        while (true)
        {
            GoToLine(14);
            int option = CreateOptionMenu("Options", new string[] { "Access Funds", "Edit Colour", "Edit Account Type", "Delete Account", "Exit" }, true);
            switch (option)
            {
                case 0:
                    float fundChange = float.Parse(GetValidUserInput(new ConsoleLine("How Much Would You Like To Deposit Or Withdraw: ", ConsoleColor.DarkBlue),
                    new UserInputProfile(new InputType[] { InputType.Int, InputType.Float }, removeWhitespace: true)));
                    if (c.funds + fundChange < 0)
                    {
                        SendConsoleMessage(new ConsoleLine("Insufficient Funds!", ConsoleColor.Red));
                        WaitForUserInput(space: true);
                        ClearLines(3, true);
                    }
                    else
                    {
                        c.funds += fundChange;
                        SendConsoleMessage($"Funds Updated, Your New Balance Is £{Math.Round(c.funds, 2)}");
                        funds = $"You Currently Have £{Math.Round(c.funds, 2)}";
                        UpdatePrimaryConsoleLine(new ConsoleLine(funds), 2);
                        DebitCard.SaveDataBase();
                        WaitForUserInput(space: true);
                        ClearLines(3, true);
                    }
                    break;
                case 1:
                    ConsoleColor[][] colourSchemes = new ConsoleColor[][] {
                ConsoleColor.White.ToArray(), ConsoleColor.Black.ToArray(), RedGradient, BlueGradient, GreenGradient, MagentaGradient,CyanDarkBlueGradient, GreenAndBlue};
                    GoToLine(14);
                    int colourScheme = CreateOptionMenu("New Colour Scheme: ", new string[] {
                    "White", "Black", "Red", "Blue", "Green", "Magenta", "Cyan To Blue", "Green And Blue" }, true);
                    c.accountColours = colourSchemes[colourScheme].ToList();
                    DebitCard.SaveDataBase();
                    ClearPrimaryConsole();
                    SendConsoleMessage(new ConsoleLine(s, AdvancedHighlight(s, ConsoleColor.White.ToArray(), DebitCard.accountColourLookup[(int)c.accountType].ToArray(), 4, 5)));
                    SendConsoleMessage(funds);

                    ShiftLine();
                    c.DisplayCard();
                    ShiftLine();
                    break;
                case 2:
                    GoToLine(14);
                    int accountType = CreateOptionMenu("New Account Type: ", new string[] { "Standard", "Premier", "Gold", "Ruby" }, true);
                    c.accountType = (DebitCard.AccountType)accountType;
                    DebitCard.SaveDataBase();
                    ClearPrimaryConsole();
                    s = $"Welcome Back To Your [{c.accountType} Account] {c.holderName}!";
                    SendConsoleMessage(new ConsoleLine(s, ColourFunctions.AdvancedHighlight(s, ConsoleColor.White, (DebitCard.accountColourLookup[(int)c.accountType].ToArray(), 4), (DebitCard.accountColourLookup[(int)c.accountType].ToArray(), 5))));
                    SendConsoleMessage(funds);

                    ShiftLine();
                    c.DisplayCard();
                    ShiftLine();
                    break;
                case 3:
                    if (CreateTrueFalseOptionMenu("Are You Sure You Want To Delete Your Account? "))
                    {
                        DebitCard.db.Remove(c);
                        DebitCard.SaveDataBase();
                        SendConsoleMessage("Account Deleted!");
                        WaitForUserInput(space: true);
                        return;
                    }
                    else
                    {
                        SendConsoleMessage("Account Deletion Aborted!");
                        WaitForUserInput(space: true);
                    }
                    break;
                case 4:
                    return;
            }
        }
    }

    /// <summary> Checks if card number valid according to Luhn algorithim.</summary>
    bool IsValidLuhn(int[] digits)
    {
        int checkDigits = 0;
        for (int i = digits.Length - 2; i >= 0; --i)
            checkDigits += ((i & 1) is 0) switch
            {
                true => digits[i] > 4 ? digits[i] * 2 - 9 : digits[i] * 2,
                false => digits[i]
            };

        return 10 - (checkDigits % 10) == digits.Last();
    }

    // --- DEBIT CARD CLASS

    /// <summary> Class for info to do with a debit card.</summary>
    class DebitCard
    {
        //static 
        public static List<DebitCard> db = new List<DebitCard>();
        public static ConsoleColor[] accountColourLookup = new ConsoleColor[] { ConsoleColor.White, ConsoleColor.DarkBlue, ConsoleColor.Yellow, ConsoleColor.Red };
        public static int accountID = 0;

        //account values
        public string holderName;
        public string initals = "";
        public string accountNumber = "";
        public string formattedAccountNumber { get { return $"{accountNumber:D16}".Insert(12, " ").Insert(8, " ").Insert(4, " "); } }

        public enum AccountType { Standard, Premier, Gold, Ruby }
        public AccountType accountType;
        public List<ConsoleColor> accountColours;

        public float funds = 1000;
        int pin;
        public int decryptedPin { get { return (int)Math.Sqrt(pin - 1452342); } }

        public DateOnly dob;
        public DateOnly expiryDate;

        public bool masterCard;
        public bool contactless;

        /// <summary> Class for info to do with a debit card.</summary>
        public DebitCard(string holderName, DateOnly dob, DateOnly expiryDate, int pin, List<ConsoleColor> accountColours, bool masterCard, bool contactless, AccountType accountType)
        {
            this.holderName = holderName;
            this.dob = dob;
            this.expiryDate = expiryDate;
            this.accountColours = accountColours;
            this.masterCard = masterCard;
            this.contactless = contactless;
            this.accountType = accountType;

            CompleteAccountSetup(pin);
        }

        /// <summary> Class for info to do with a debit card.</summary>
        public DebitCard(string[] cardString)
        {
            holderName = cardString[0];
            initals = string.Join(". ", holderName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]).Take(2)).ToUpper();
            dob = DateOnly.Parse(cardString[1]);
            expiryDate = DateOnly.Parse(cardString[1]).AddYears(30);
            pin = int.Parse(cardString[2]);
            masterCard = bool.Parse(cardString[3]);
            contactless = bool.Parse(cardString[4]);
            accountColours = cardString[5].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => (ConsoleColor)int.Parse(s)).ToList();
            accountNumber = cardString[6];
            accountType = Enum.Parse<AccountType>(cardString[7]);
            funds = float.Parse(cardString[8]);
        }

        public override string ToString()
        {
            return $"{holderName}\n{dob}\n{pin}\n{masterCard}\n{contactless}\n{string.Join(",", accountColours.Select(colour => (int)colour))}\n{accountNumber}\n{accountType}\n{funds}\n";
        }

        /// <summary> Displays card in console </summary>
        public void DisplayCard(bool censored = false)
        {
            ConsoleColor[] c = Alternate(accountColours.ToArray(), 60, 2);
            string s = $"{(masterCard ? "Mastercard" : "Visa")} {(contactless ? "- Contactless" : "")}", s2 = $"Honeycomb Bank [{(censored ? "******" : accountType)} Account]  |";

            string[] cardLines = new string[] {
                        $"+{new string('-', 58)}+",
                        $"|{new string(' ', 58)}|",
                        $"|  {s}{new string(' ', 34 - s.Length)}Account Number: {accountNumber.ToString().Substring(12)}  |",
                        $"|{new string(' ', 58)}|",
                        $"|  {(censored ? "**** **** **** ****" : formattedAccountNumber)}{new string(' ', 25)}{(censored ? "**/**/****" : expiryDate)}  |",
                        $"|{new string(' ', 58)}|",
                        $"|  {initals}{new string(' ', 53 - s2.Length)}{s2}",
                        $"|{new string(' ', 58)}|",
                        $"+{new string('-', 58)}+"
                    };

            for (int i = 0; i < cardLines.Length; i++) { SendConsoleMessage(new ConsoleLine(cardLines[i], c), new ConsoleLineUpdate(), new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, "1", 10, true)); }
        }

        /// <summary> Completes additional tasks for card setup </summary>
        public void CompleteAccountSetup(int pin)
        {
            this.pin = (pin * pin) + 1452342;
            accountNumber = new Random().NextInt64(100000000000, 999999999999).ToString() + accountID.ToString("0000");
            accountID++;
            initals = string.Join(". ", this.holderName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]).Take(2)).ToUpper();
            funds = 1000;
        }

        /// <summary> Gets DebitCard info from text file </summary>
        public static void LoadDataBase()
        {
            db.Clear();
            string[][] lines = AppPersistentData.LoadFile("DebitCard/Data", 9, true, 1);
            for (int i = 0; i < lines.Length; i++) { db.Add(new DebitCard(lines[i])); }
        }

        /// <summary> Saves DebitCard info to text file </summary>
        public static void SaveDataBase()
        {
            string s = $"{accountID}\n";
            for (int i = 0; i < db.Count; i++) { s += db[i].ToString(); }
            AppPersistentData.SaveFile("DebitCard/Data", s.Split("\n"));
        }
    }
}