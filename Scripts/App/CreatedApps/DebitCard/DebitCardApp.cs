using Revistone.Console;
using Revistone.Functions;
using static Revistone.Console.ConsoleAction;
using Revistone.Interaction;
using static Revistone.Interaction.UserInput;
using static Revistone.Interaction.UserInputProfile;

namespace Revistone
{
    namespace Apps
    {
        public class DebitCardApp : App
        {
            List<DebitCard> cardInfo = new List<DebitCard>();

            public DebitCardApp(string name, ConsoleColor[] borderColours, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int borderColourSpeed = 5, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base (name, borderColours, appCommands, borderColourSpeed, minAppWidth, minAppHeight, baseCommands) {}

            public override void OnAppInitalisation()
            {
                DebitCard.accountID = int.Parse(File.ReadLines("Scripts/App/CreatedApps/DebitCard/DebitCard.txt").ToArray()[0]);

                LoadDebitCardInfo();
                MainMenu();
            }

            void MainMenu()
            {
                ClearPrimaryConsole();

                string[] title = new string[] {
                    " _   _ _____ _   _  _______   __   ______  ___   _   _  _   __",
                    @"| | | |  _  | \ | ||  ___\ \ / /   | ___ \/ _ \ | \ | || | / /",
                    @"| |_| | | | |  \| || |__  \ V /    | |_/ / /_\ \|  \| || |/ / ",
                    @"|  _  | | | | . ` ||  __|  \ /     | ___ \  _  || . ` ||    \ ",
                    @"| | | \ \_/ / |\  || |___  | |     | |_/ / | | || |\  || |\  \",
                    @"\_| |_/\___/\_| \_/\____/  \_/     \____/\_| |_/\_| \_/\_| \_/"
                };

                for (int i = 1; i < 7; i++)
                {
                    SendConsoleMessage(new ConsoleLine(title[i - 1], ColourFunctions.ColourGradient(ColourFunctions.CyanToDarkMagentaGradient, 62)));
                    UpdateLineAnimation(new ConsoleAnimatedLine(true, ConsoleAnimatedLine.UpdateType.ShiftRight, 10), i);
                }

                ShiftLine();

                int userChoice = CreateOptionMenu($"Options",
                         new string[] { "Access Debit Card", "Create Debit Card",
                         "View All Accounts", "Verify Long Card Number", "Exit" }, true);

                switch (userChoice)
                {
                    case 0:
                        AccessCard();
                        break;
                    case 1:
                        CreateCard();
                        break;
                    case 2:
                        ViewAllCards();
                        break;
                    case 3:
                        ValidateCardNumber();
                        break;
                    case 4:
                        ExitApp();
                        return;
                }

                MainMenu();
            }

            // --- MENU OPTIONS ---

            void CardMenu(DebitCard c)
            {
                ClearPrimaryConsole();
                string s = $"Welcome Back To Your [{c.accountType} Account] {c.holderName}!";
                string funds = $"You Currently Have £{Math.Round(c.funds, 2)}";

                SendConsoleMessage(new ConsoleLine(s, ColourFunctions.ColourWords(s, new int[] { 4, 5 },
        new ConsoleColor[] { DebitCard.accountColourLookup[(int)c.accountType] }, ConsoleColor.White)));
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
                                ShiftLine();
                                WaitForUserInput(ConsoleKey.Enter);
                                ClearLines(3, true);
                            }
                            else
                            {
                                c.funds = c.funds + fundChange;
                                SendConsoleMessage($"Funds Updated, Your New Balance Is £{Math.Round(c.funds, 2)}");
                                funds = $"You Currently Have £{Math.Round(c.funds, 2)}";
                                UpdateConsoleLine(new ConsoleLine(funds), 2);
                                SaveDebitCardInfo();
                                ShiftLine();
                                WaitForUserInput(ConsoleKey.Enter);
                                ClearLines(3, true);
                            }
                            break;
                        case 1:
                            ConsoleColor[][] colourSchemes = new ConsoleColor[][] { new ConsoleColor[] { ConsoleColor.White }, new ConsoleColor[] { ConsoleColor.Black }, new ConsoleColor[] { ConsoleColor.Red, ConsoleColor.DarkRed },
                new ConsoleColor[] { ConsoleColor.Blue, ConsoleColor.DarkBlue }, new ConsoleColor[] { ConsoleColor.Green, ConsoleColor.DarkGreen }, new ConsoleColor[] { ConsoleColor.Magenta, ConsoleColor.DarkMagenta },
                new ConsoleColor[] { ConsoleColor.Cyan, ConsoleColor.DarkCyan, ConsoleColor.Blue, ConsoleColor.DarkBlue }, new ConsoleColor[] {ConsoleColor.Green, ConsoleColor.Blue}};
                            GoToLine(14);
                            int colourScheme = CreateOptionMenu("New Colour Scheme: ", new string[] {
                    "White", "Black", "Red", "Blue", "Green", "Magenta", "Cyan To Blue", "Green And Blue" }, true);
                            c.colours = colourSchemes[colourScheme].ToList();
                            SaveDebitCardInfo();
                            ClearPrimaryConsole();
                            SendConsoleMessage(new ConsoleLine(s, ColourFunctions.ColourWords(s, new int[] { 4, 5 },
                            new ConsoleColor[] { DebitCard.accountColourLookup[(int)c.accountType] }, ConsoleColor.White)));
                            SendConsoleMessage(funds);

                            ShiftLine();
                            c.DisplayCard();
                            ShiftLine();
                            break;
                        case 2:
                            GoToLine(14);
                            int accountType = CreateOptionMenu("New Account Type: ", new string[] { "Standard", "Premier", "Gold", "Ruby" }, true);
                            c.accountType = (DebitCard.AccountType)accountType;
                            SaveDebitCardInfo();
                            ClearPrimaryConsole();
                            s = $"Welcome Back To Your [{c.accountType} Account] {c.holderName}!";
                            SendConsoleMessage(new ConsoleLine(s, ColourFunctions.ColourWords(s, new int[] { 4, 5 },
                            new ConsoleColor[] { DebitCard.accountColourLookup[(int)c.accountType] }, ConsoleColor.White)));
                            SendConsoleMessage(funds);

                            ShiftLine();
                            c.DisplayCard();
                            ShiftLine();
                            break;
                        case 3:
                            if (CreateOptionMenu("Are You Sure You Want To Delete Your Account? ", true))
                            {
                                cardInfo.Remove(c);
                                SaveDebitCardInfo();
                                SendConsoleMessage("Account Deleted!");
                                ShiftLine();
                                WaitForUserInput(ConsoleKey.Enter);
                                return;
                            }
                            else
                            {
                                SendConsoleMessage("Account Deletion Aborted!");
                                ShiftLine();
                                WaitForUserInput(ConsoleKey.Enter);
                            }
                            break;
                        case 4:
                            return;
                    }
                }
            }

            void AccessCard()
            {
                string accountNumber = GetValidUserInput(new ConsoleLine("Account Number [Last 4 Digits Of The Card Number]: ", ConsoleColor.DarkBlue), new UserInputProfile(InputType.Int, charCount: 4));
                for (int i = 0; i < cardInfo.Count; i++)
                {
                    if (accountNumber == cardInfo[i].accountNumber.Substring(12))
                    {
                        SendConsoleMessage(new ConsoleLine($"Account With The Number {accountNumber} Found, Owner {cardInfo[i].initals}!", ConsoleColor.DarkBlue));
                        ShiftLine();
                        WaitForUserInput(ConsoleKey.Enter);

                        ClearPrimaryConsole();

                        DateOnly d = DateOnly.Parse(GetValidUserInput(new ConsoleLine("DOB [dd/mm/yyyy]: ", ConsoleColor.DarkBlue), new UserInputProfile(InputType.DateOnly)));
                        if (d != cardInfo[i].dob)
                        {
                            SendConsoleMessage("DOB Incorrect!");
                            ShiftLine();
                            WaitForUserInput(ConsoleKey.Enter);
                            return;
                        }

                        int p = int.Parse(GetValidUserInput(new ConsoleLine("Pin: ", ConsoleColor.DarkBlue), new UserInputProfile(InputType.Int, charCount: 4)));
                        if (p != cardInfo[i].DecryptPin())
                        {
                            SendConsoleMessage("Pin Incorrect!");
                            ShiftLine();
                            WaitForUserInput(ConsoleKey.Enter);
                            return;
                        }

                        CardMenu(cardInfo[i]);
                        ClearPrimaryConsole();
                        return;
                    }
                }

                SendConsoleMessage($"No Account With The Number {accountNumber} Found!");
                ShiftLine();
                WaitForUserInput(ConsoleKey.Enter);
                return;
            }

            void ViewAllCards()
            {
                if (cardInfo.Count == 0) return;

                int pointer = 0;

                while (true)
                {
                    ClearPrimaryConsole();

                    DebitCard d = cardInfo[pointer];

                    SendConsoleMessage($"Account [{pointer + 1} / {cardInfo.Count}]");
                    ShiftLine();

                    cardInfo[pointer].DisplayCard(true);

                    ShiftLine();

                    int option = CreateOptionMenu("Options: ", new string[] { "Next Card", "Previous Card", "Exit" });

                    switch (option)
                    {
                        case 0:
                            pointer = pointer < cardInfo.Count - 1 ? pointer + 1 : 0;
                            break;
                        case 1:
                            pointer = pointer > 0 ? pointer - 1 : cardInfo.Count - 1;
                            break;
                        case 2:
                            return;
                    }
                }
            }

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
                    ShiftLine();
                    WaitForUserInput(ConsoleKey.Enter);
                    return;
                }

                string name = GetValidUserInput(new ConsoleLine("Enter Your Full Name: ", ConsoleColor.DarkBlue), new UserInputProfile(InputType.FullText, wordCount: 2));
                int pin = int.Parse(GetValidUserInput(new ConsoleLine("Create A Pin [4 Digits]: ", ConsoleColor.DarkBlue), new UserInputProfile(InputType.Int, charCount: 4)));
                bool masterCard = CreateOptionMenu("Card Type: ", new string[] { "Visa", "Mastercard" }, true) == 0 ? false : true;
                bool contactless = CreateOptionMenu("Contactless: ", true);

                ConsoleColor[][] c = new ConsoleColor[][] { new ConsoleColor[] { ConsoleColor.White }, new ConsoleColor[] { ConsoleColor.Black }, new ConsoleColor[] { ConsoleColor.Red, ConsoleColor.DarkRed },
                new ConsoleColor[] { ConsoleColor.Blue, ConsoleColor.DarkBlue }, new ConsoleColor[] { ConsoleColor.Green, ConsoleColor.DarkGreen }, new ConsoleColor[] { ConsoleColor.Magenta, ConsoleColor.DarkMagenta },
                new ConsoleColor[] { ConsoleColor.Cyan, ConsoleColor.DarkCyan, ConsoleColor.Blue, ConsoleColor.DarkBlue }, new ConsoleColor[] {ConsoleColor.Green, ConsoleColor.Blue}};

                int colourScheme = CreateOptionMenu("Colour Scheme: ", new string[] {
                    "White", "Black", "Red", "Blue", "Green", "Magenta", "Cyan To Blue", "Green And Blue" }, true);
                List<ConsoleColor> colours = c[colourScheme].ToList();

                int accountType = CreateOptionMenu("Account Type: ", new string[] { "Standard", "Premier", "Gold", "Ruby" }, true);

                DebitCard d = new DebitCard(name, DateOnly.Parse(dob), dobCheck, pin, colours, masterCard, contactless, (DebitCard.AccountType)accountType);
                AddCard(d);

                string s = $"Congrats On Your New Honeycomb [{(DebitCard.AccountType)accountType} Account] {d.holderName}!";
                SendConsoleMessage(new ConsoleLine(s, ColourFunctions.ColourWords(s, new int[] { 5, 6 },
                new ConsoleColor[] { DebitCard.accountColourLookup[(int)accountType] }, ConsoleColor.White)));
                ShiftLine();
                d.DisplayCard();
                ShiftLine();
                WaitForUserInput(ConsoleKey.Enter);
            }

            void ExitApp()
            {
                SetActiveApp("Revistone");
                ResetConsole();
                return;
            }

            void ValidateCardNumber()
            {
                string card = GetValidUserInput(new ConsoleLine("Enter A Card Number", ConsoleColor.DarkBlue), new UserInputProfile(InputType.Int, charCount: 16, removeWhitespace: true));

                card = card.Replace(" ", "");
                int[] digits = new int[16];
                for (int i = 0; i < 16; i++)
                {
                    int.TryParse(card[i].ToString(), out digits[i]);
                }
                if (IsValidLuhn(digits)) SendConsoleMessage(new ConsoleLine("Debit Card Number Is Valid!", ConsoleColor.Green), new ConsoleLineUpdate());
                else SendConsoleMessage(new ConsoleLine("Debit Card Number Is NOT Valid!", ConsoleColor.Red), new ConsoleLineUpdate());

                ShiftLine();
                WaitForUserInput(ConsoleKey.Enter, true);
            }

            // --- CARD DATA ---
            void AddCard(DebitCard d)
            {
                cardInfo.Add(d);
                SaveDebitCardInfo();
            }

            /// <summary> Gets DebitCard info from text file </summary>
            void LoadDebitCardInfo()
            {
                string[] lines = File.ReadAllLines("Scripts/App/CreatedApps/DebitCard/DebitCard.txt");

                for (int i = 1; i < lines.Length; i += 9)
                {
                    cardInfo.Add(new DebitCard(lines.Skip(i).Take(9).ToArray()));
                }
            }

            void SaveDebitCardInfo()
            {
                string s = $"{DebitCard.accountID}\n";
                for (int i = 0; i < cardInfo.Count; i++)
                {
                    s += cardInfo[i].ToString();
                }

                File.WriteAllText("Scripts/App/CreatedApps/DebitCard/DebitCard.txt", s);
            }

            // --- EXTRA FUNCTIONS ---

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

            class DebitCard
            {
                public static ConsoleColor[] accountColourLookup = new ConsoleColor[] { ConsoleColor.White, ConsoleColor.DarkBlue, ConsoleColor.Yellow, ConsoleColor.Red };

                public enum AccountType { Standard, Premier, Gold, Ruby }
                public AccountType accountType;

                public float funds = 1000;

                public string holderName;
                public string initals;

                public string accountNumber;
                int pin;

                public DateOnly dob;
                public DateOnly expiryDate;

                public List<ConsoleColor> colours;

                public bool masterCard; //false means visa
                public bool contactless;

                public DebitCard(string holderName, DateOnly dob, DateOnly expiryDate, int pin, List<ConsoleColor> colours, bool masterCard, bool contactless, AccountType accountType)
                {
                    this.holderName = holderName;
                    char[] firstChars = this.holderName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]).ToArray();
                    this.initals = $"{firstChars[0]}. {firstChars[1]}".ToUpper();

                    this.funds = 1000;

                    this.dob = dob;
                    this.expiryDate = expiryDate;
                    SetPin(pin);
                    this.accountNumber = GenerateAccountNumber();
                    this.colours = colours;
                    this.masterCard = masterCard;
                    this.contactless = contactless;

                    this.accountType = accountType;
                }

                public DebitCard(string[] cardString)
                {
                    this.holderName = cardString[0];
                    char[] firstChars = this.holderName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]).ToArray();
                    this.initals = $"{firstChars[0]}. {firstChars[1]}".ToUpper();

                    this.dob = DateOnly.Parse(cardString[1]);
                    this.expiryDate = DateOnly.Parse(cardString[1]).AddYears(30);
                    this.pin = int.Parse(cardString[2]);
                    this.masterCard = bool.Parse(cardString[3]);
                    this.contactless = bool.Parse(cardString[4]);

                    this.colours = new List<ConsoleColor>();
                    string[] c = cardString[5].Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in c)
                    {
                        colours.Add((ConsoleColor)int.Parse(s));
                    }

                    this.accountNumber = cardString[6];
                    this.accountType = Enum.Parse<AccountType>(cardString[7]);
                    this.funds = float.Parse(cardString[8]);
                }

                public override string ToString()
                {
                    string s = $"{holderName}\n{dob}\n{pin}\n{masterCard}\n{contactless}\n";
                    string cs = "";
                    foreach (ConsoleColor c in colours) cs += $"{(int)c},";
                    s += $"{cs.Substring(0, cs.Length - 1)}\n";
                    s += $"{accountNumber}\n{accountType}\n{funds}\n";

                    return s;
                }

                public void DisplayCard(bool censored = false)
                {
                    ConsoleColor[] c = ColourFunctions.AlternatingColours(colours.ToArray(), 60, 2);

                    string s = $"{(masterCard ? "Mastercard" : "Visa")} {(contactless ? "- Contactless" : "")}";
                    string s2 = $"Honeycomb Bank [{(censored ? "******" : accountType)} Account]  |";

                    string[] cardLines = new string[] {
                        new string('-', 60),
                        $"|" + new string(' ', 58) + "|",
                        $"|  {s}" + new string(' ', 34 - s.Length) + $"Account Number: {accountNumber.ToString().Substring(12)}  |",
                        $"|{new string(' ', 58)}|",
                        $"|  {(censored ? "**** **** **** ****" : FormattedAccountNumber())}" + new string(' ', 25) + $"{(censored ? "**/**/****" : expiryDate)}  |",
                        $"|" + new string(' ', 58) + "|",
                        $"|  {initals}" + new string(' ', 53 - s2.Length) + s2,
                        $"|" + new string(' ', 58) + "|",
                        new string('-', 60)
                    };
                    int currentLine = GetConsoleLineIndex();

                    for (int i = 0; i < cardLines.Length; i++)
                    {
                        SendConsoleMessage(new ConsoleLine(cardLines[i], c), new ConsoleLineUpdate());
                        UpdateLineAnimation(new ConsoleAnimatedLine(true, ConsoleAnimatedLine.UpdateType.ShiftRight, 10), currentLine + i);
                    }
                }

                public static int accountID = 0;

                public static string GenerateAccountNumber()
                {
                    Random rnd = new Random();
                    string s = rnd.NextInt64(100000000000, 999999999999).ToString() + accountID.ToString("0000");
                    accountID++;
                    return s;
                }

                public void SetPin(int pin)
                {
                    this.pin = (pin * pin) + 1452342;
                }

                public int DecryptPin()
                {
                    return (int)Math.Sqrt(pin - 1452342);
                }

                public string FormattedAccountNumber()
                {
                    return $"{accountNumber.ToString().Substring(0, 4)} {accountNumber.ToString().Substring(4, 4)} {accountNumber.ToString().Substring(8, 4)} {accountNumber.ToString().Substring(12, 4)}";
                }
            }
        }
    }
}