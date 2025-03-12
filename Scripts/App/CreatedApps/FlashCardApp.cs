using Revistone.Functions;
using Revistone.Interaction;
using Revistone.Console;
using Revistone.Management;

using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;

//FCS -> Flash Card Set

namespace Revistone.App;

public class FlashCardApp : App
{
    // --- APP BOILER ---

    public FlashCardApp() : base() { }
    public FlashCardApp(string name, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return new FlashCardApp[] {
                    new FlashCardApp("Flash Card Manager", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.DarkGreen.ToArray(), ConsoleColor.Green.ToArray(), 10), (Alternate(DarkGreenAndDarkBlue, 6, 3), 5), new (UserInputProfile format, Action<string> payload, string summary)[0], 70, 40)
                };
    }

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();

        for (int i = 0; i <= 10; i++) { UpdateLineExceptionStatus(true, i); }

        ShiftLine();
        ConsoleLine[] title = TitleFunctions.CreateTitle("REVISE", AdvancedHighlight(64, ConsoleColor.DarkBlue.ToArray(), (ConsoleColor.DarkGreen.ToArray(), 0, 10), (ConsoleColor.DarkGreen.ToArray(), 32, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1);
        SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), title.Length).ToArray());
        ShiftLine();
        MainMenu();
    }

    // --- MENU OPTIONS ---

    /// <summary> Main menu loop.</summary>
    void MainMenu()
    {
        ClearPrimaryConsole();

        UserInput.CreateOptionMenu("Options:", new (string name, Action action)[] {("Use Flash Cards", MainMenuSelectFCS), ("Edit Flash Cards", MainMenuEditFCS),
                ("Stats", MainMenuStats), ("Exit", ExitApp)});

        if (AppRegistry.activeApp.name != "Flash Card Manager") return;

        MainMenu();
    }

    /// <summary> Menu to select a FCS to use.</summary>
    void MainMenuSelectFCS()
    {
        string[] flashNames = AppPersistentData.GetSubFiles("FlashCard");
        if (flashNames.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Flash Card Sets Can Be Found!", ConsoleColor.DarkRed));
            UserInput.WaitForUserInput(space: true);
            return;
        }
        int i = UserInput.CreateMultiPageOptionMenu("Flash Card Sets", flashNames.Select(s => new ConsoleLine(StringFunctions.SplitAtCapitalisation(s))).ToArray(), new ConsoleLine[] { new ConsoleLine("Exit") }, 8);
        if (i >= 0) UseFCS(new FlashCardSet($"FlashCard/{flashNames[i]}"));
    }

    /// <summary> Menu to select a FCS to edit.</summary>
    void MainMenuEditFCS()
    {
        string[] flashNames = AppPersistentData.GetSubFiles("FlashCard");
        int i = UserInput.CreateMultiPageOptionMenu("Flash Card Sets", new ConsoleLine[] { new ConsoleLine("New Flash Card Set") }.Concat(flashNames.Select(s => new ConsoleLine(StringFunctions.SplitAtCapitalisation(s)))).ToArray(), new ConsoleLine[] { new ConsoleLine("Exit") }, 8);

        if (i == 0) CreateFCS();
        else if (i > 0)
        {
            EditFCS(new FlashCardSet($"FlashCard/{flashNames[i - 1]}"));
        }
    }

    /// <summary> View flash card stats.</summary>
    void MainMenuStats()
    {
        ClearPrimaryConsole();

        int i = UserInput.CreateOptionMenu("Options:", new string[] { "General Stats", "Flash Card Set Stats", "Exit" });

        if (i == 0)
        {
            FlashCardSet[] f = AppPersistentData.GetSubFiles("FlashCard").Select(s => new FlashCardSet($"FlashCard/{s}")).ToArray();

            SendConsoleMessage(new ConsoleLine("General Flash Card Manager Stats", ConsoleColor.DarkBlue));

            SendConsoleMessage($"Time Revising: {new TimeSpan(f.Sum(s => s.timeSpent.Ticks)).ToString(@"hh\:mm\:ss")}");
            SendConsoleMessage($"Flash Card Sets Completed: {f.Sum(s => s.timesCompleted)}");
            ShiftLine();
            SendConsoleMessage($"Total Questions Answered: {f.Sum(s => s.questionsCompleted)}");
            SendConsoleMessage($"Total Questions Correctly: {f.Sum(s => s.questionsCompletedCorrect)}");
            UserInput.WaitForUserInput(space: true);
            MainMenuStats();
            return;
        }
        else if (i == 1)
        {
            string[] flashNames = AppPersistentData.GetSubFiles("FlashCard");
            if (flashNames.Length == 0)
            {
                SendConsoleMessage(new ConsoleLine("No Flash Card Sets Can Be Found!", ConsoleColor.DarkRed));
                UserInput.WaitForUserInput(space: true);
                MainMenuStats();
                return;
            }
            int k = UserInput.CreateMultiPageOptionMenu("Flash Card Sets", flashNames.Select(s => new ConsoleLine(StringFunctions.SplitAtCapitalisation(s))).ToArray(), new ConsoleLine[] { new ConsoleLine("Exit") }, 8);
            if (k < 0)
            {
                MainMenuStats();
                return;
            }
            FlashCardSet f = new FlashCardSet($"FlashCard/{flashNames[k]}");
            SendConsoleMessage(new ConsoleLine($"Stats For Flash Card Set '{StringFunctions.SplitAtCapitalisation(f.name)}'", ConsoleColor.DarkBlue));
            ShiftLine();
            f.PrintStats();
            UserInput.WaitForUserInput(space: true);
        }
    }

    /// <summary> Loop for flashcard use.</summary>
    // --- FLASH CARD USEAGE ---

    void UseFCS(FlashCardSet s)
    {
        FlashCard[] shuffledQuestions = s.questions.OrderBy(a => Manager.rng.Next()).ToArray();
        List<int> correctQuestions = new List<int>();

        DateTime startTime = DateTime.Now;

        for (int i = 0; i < shuffledQuestions.Length; i++)
        {
            ClearPrimaryConsole();

            SendConsoleMessage(new ConsoleLine($"Flash Card Set '{StringFunctions.SplitAtCapitalisation(s.name)}' [{i + 1} / {shuffledQuestions.Length}]", ConsoleColor.DarkBlue));
            ShiftLine();

            string answer = "";
            string correctAnswer = "";

            switch (FlashCard.GetQuestionType(shuffledQuestions[i]))
            {
                case 0:
                    StandardFlashCard sfc = (StandardFlashCard)shuffledQuestions[i];
                    SendConsoleMessage(new ConsoleLine("Promt:", AppRegistry.activeApp.colourScheme.primaryColour));
                    SendConsoleMessage(sfc.promt);
                    ShiftLine();
                    answer = UserInput.GetUserInput("Answer: ", clear: true);
                    correctAnswer = sfc.expectedAnswer;
                    break;
                case 1:
                    MultiChoiceFlashCard mcfc = (MultiChoiceFlashCard)shuffledQuestions[i];
                    SendConsoleMessage(new ConsoleLine("Promt:", AppRegistry.activeApp.colourScheme.primaryColour));
                    SendConsoleMessage(mcfc.promt);
                    ShiftLine();
                    answer = UserInput.CreateOptionMenu("Answer: ", mcfc.answers.ToArray()).ToString();
                    correctAnswer = mcfc.answers[mcfc.answerIndex];
                    break;
                case 2:
                    FillTheGapFlashCard ftgfc = (FillTheGapFlashCard)shuffledQuestions[i];
                    SendConsoleMessage(new ConsoleLine("Fill In The Gap:", AppRegistry.activeApp.colourScheme.primaryColour));
                    SendConsoleMessage(ftgfc.FormattedQuestion());
                    ShiftLine();
                    answer = UserInput.GetUserInput("Answer: ", clear: true);
                    correctAnswer = ftgfc.CorrectAnswer();
                    break;
            }

            if (shuffledQuestions[i].CheckAnswer(answer.Trim()))
            {
                correctQuestions.Add(i);
                SendConsoleMessage(new ConsoleLine("Answer Correct!", ConsoleColor.Green));
            }
            else
            {
                SendConsoleMessage(new ConsoleLine("Answer Incorrect!", ConsoleColor.DarkRed));
                SendConsoleMessage($"Correct Answer: {correctAnswer}");
            }

            UserInput.WaitForUserInput(space: true);
        }

        TimeSpan time = DateTime.Now - startTime;
        s.timeSpent += time;
        s.timesCompleted++;
        s.questionsCompleted += s.questions.Count;
        s.questionsCompletedCorrect += correctQuestions.Count;

        if (s.questionHighscore < correctQuestions.Count)
        {
            s.questionHighscore = correctQuestions.Count;
            s.bestTime = time;
        }
        else if (s.questionHighscore == correctQuestions.Count && s.bestTime > time) s.bestTime = time;

        //stats 
        ClearPrimaryConsole();
        SendConsoleMessage(new ConsoleLine($"'{StringFunctions.SplitAtCapitalisation(s.name)}' Complete!", ConsoleColor.DarkBlue));
        ShiftLine();
        SendConsoleMessage($"Score: [{correctQuestions.Count} / {s.questions.Count}] {Math.Round((double)correctQuestions.Count / (double)s.questions.Count * 100d, 2)}%");
        SendConsoleMessage($"Time: {time.ToString(@"mm\:ss")}");
        ShiftLine();
        s.PrintStats();

        SaveFCS(s);
        UserInput.WaitForUserInput(space: true);
    }

    // --- FLASH CARD SET MODIFICATIONS ---

    /// <summary> Creates a new FCS and opens editing menu.</summary>
    void CreateFCS()
    {
        string setName = UserInput.GetValidUserInput("Flash Card Set Name: ", new UserInputProfile(UserInputProfile.InputType.FullText, "[C:]", removeWhitespace: true));
        EditFCS(new FlashCardSet(setName, new List<FlashCard>()));
    }

    /// <summary> Main loop for editing a FCS.</summary>
    void EditFCS(FlashCardSet s)
    {
        ClearPrimaryConsole();
        SendConsoleMessage(new ConsoleLine($"Editing Set '{StringFunctions.SplitAtCapitalisation(s.name)}' Of {s.questions.Count} Questions.", ConsoleColor.DarkGreen));
        ShiftLine();
        bool deleted = false;

        int i = UserInput.CreateOptionMenu("Options:", new (string name, Action action)[] {
                    ("New Question", () => s.questions.Add(CreateQuestion(s))),
                    ("View Questions", () => ViewQuestions(s)),
                    ("Delete Flash Card Set", () => deleted = DeleteFCS(s)),
                    ("Exit", () => {})
                });

        if (deleted) { return; }

        if (i != 3) EditFCS(s);
        else SaveFCS(s);
    }

    /// <summary> Deletes given FCS file.</summary>
    bool DeleteFCS(FlashCardSet s)
    {
        if (UserInput.CreateTrueFalseOptionMenu($"Are You Sure You Want To Delete '{StringFunctions.SplitAtCapitalisation(s.name)}'?"))
        {
            AppPersistentData.DeleteFile($"FlashCard/{s.name}");
            SendConsoleMessage($"'{StringFunctions.SplitAtCapitalisation(s.name)}' Deleted!");
            UserInput.WaitForUserInput(space: true);
            return true;
        }
        else
        {
            SendConsoleMessage($"'{StringFunctions.SplitAtCapitalisation(s.name)}' Deletion Aborted!");
            UserInput.WaitForUserInput(space: true);
            return false;
        }
    }

    /// <summary> Saves given FCS to file.</summary>
    void SaveFCS(FlashCardSet s)
    {
        AppPersistentData.SaveFile($"FlashCard/{s.name}", s.ToString().Split('\n'));
    }

    // QUESTION MODIFICATIONS

    /// <summary> Create a question for a given FCS.</summary>
    FlashCard CreateQuestion(FlashCardSet s)
    {
        int questionType = UserInput.CreateOptionMenu("Question Type:", new string[] { "Standard", "Multi Choice", "Fill The Gap" });
        string promt = UserInput.GetValidUserInput("Create Your Flash Card's Promt:", new UserInputProfile());
        switch (questionType)
        {
            case 0:
                string answer = UserInput.GetValidUserInput("Create Your Flash Card's Answer:", new UserInputProfile());
                return new StandardFlashCard(promt, answer);
            case 1:
                List<string> answers = new List<string>();
                while (true)
                {
                    if (answers.Count >= 2 && UserInput.CreateOptionMenu("Options: ", new string[] { "New Choice", "Finish" }) == 1)
                    {
                        return new MultiChoiceFlashCard(promt, answers, UserInput.CreateOptionMenu("Select The Correct Answer: ", answers.ToArray()));
                    }
                    answers.Add(UserInput.GetValidUserInput($"Input Flash Card's Answer Choice {answers.Count + 1}:", new UserInputProfile(bannedChars: ":")));
                }
            default:
                List<int> validQuestionIndexes = new List<int>();
                while (true)
                {
                    if (validQuestionIndexes.Count > 0 && UserInput.CreateOptionMenu("Options: ", new string[] { "New Choice", "Finish" }) == 1)
                    {
                        return new FillTheGapFlashCard(promt, validQuestionIndexes);
                    }
                    int questionIndex = int.Parse(UserInput.GetValidUserInput($"Input Valid Question Word Index {validQuestionIndexes.Count + 1}:", new UserInputProfile(UserInputProfile.InputType.Int, bannedChars: ":")));
                    if (validQuestionIndexes.Contains(questionIndex) || questionIndex < 0 || questionIndex >= promt.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
                    {
                        SendConsoleMessage(new ConsoleLine("Index Already Registered, Or Out Of Bounds Of The Promt!", ConsoleColor.DarkRed));
                        UserInput.WaitForUserInput(space: true);
                        ClearLines(2, true);
                    }
                    else validQuestionIndexes.Add(questionIndex);
                }


        }
    }

    /// <summary> Select and edit a question from a given FCS.</summary>
    void ViewQuestions(FlashCardSet s)
    {
        if (s.questions.Count == 0)
        {
            SendConsoleMessage(new ConsoleLine("This Flash Card Set Has No Questions!", ConsoleColor.DarkRed));
            UserInput.WaitForUserInput(space: true);
            return;
        }

        int cq = 0;

        int pointer = 0;

        while (true)
        {
            ClearPrimaryConsole();
            SendConsoleMessage($"Flash Card Set '{StringFunctions.SplitAtCapitalisation(s.name)}'");

            SendConsoleMessage($"Question {cq + 1}/{s.questions.Count}");
            SendConsoleMessage($"Type: {FlashCard.QuestionTypeName[FlashCard.GetQuestionType(s.questions[cq])]}");
            ShiftLine();
            SendConsoleMessage($"Promt: {s.questions[cq].promt}");
            switch (FlashCard.GetQuestionType(s.questions[cq]))
            {
                case 0:
                    SendConsoleMessage($"Answer: {((StandardFlashCard)s.questions[cq]).expectedAnswer}");
                    break;
                case 1:
                    SendConsoleMessage($"Choices: ");
                    SendConsoleMessages(((MultiChoiceFlashCard)s.questions[cq]).answers.Select((s, index) => new ConsoleLine($"{index + 1}. {s}")).ToArray());
                    SendConsoleMessage($"Correct Answer: {((MultiChoiceFlashCard)s.questions[cq]).answers[((MultiChoiceFlashCard)s.questions[cq]).answerIndex]}");
                    break;
                case 2:
                    SendConsoleMessage($"Promt Indexes: {((FillTheGapFlashCard)s.questions[cq]).questionIndexes.ToElementString()}");
                    break;
            }
            ShiftLine();


            bool reload = false;


            pointer = UserInput.CreateOptionMenu("Options: ", new (ConsoleLine, Action)[] {
                        (new ConsoleLine("Edit Question"), () => { if (UserInput.CreateTrueFalseOptionMenu("Edit Question:")) { s.questions[cq] = CreateQuestion(s); reload = true;}} ),
                        (new ConsoleLine("Delete Question"), () => { if (UserInput.CreateTrueFalseOptionMenu("Delete Question:")) { s.questions.RemoveAt(cq); reload = true;}}),
                        (new ConsoleLine("Next Question", ConsoleColor.DarkBlue), () => cq = cq < s.questions.Count - 1 ? cq + 1 : 0),
                        (new ConsoleLine("Previous Question", ConsoleColor.DarkBlue), () => cq = cq > 0 ? cq - 1 : s.questions.Count - 1),
                        (new ConsoleLine("Exit", ConsoleColor.DarkBlue), () => {}),
                        },
            cursorStartIndex: pointer);


            if (reload)
            {
                ViewQuestions(s);
                return;
            }

            if (pointer == 4) return;
        }
    }

    // --- FLASH CARD CLASSES ---

    /// <summary>Class pertaining all information for a set of flash cards.</summary>
    class FlashCardSet
    {
        public string name;

        // STATS 
        public TimeSpan timeSpent = TimeSpan.Zero;
        public TimeSpan bestTime = TimeSpan.Zero;
        public int questionsCompleted = 0;
        public int questionsCompletedCorrect = 0;
        public int questionHighscore = 0;
        public int timesCompleted = 0;

        public List<FlashCard> questions;

        public FlashCardSet(string name, List<FlashCard> questions)
        {
            this.name = name;
            this.questions = questions;
        }

        public FlashCardSet(string filePath)
        {
            string[] s = AppPersistentData.LoadFile(filePath, 0, 7);
            name = s[0];
            timeSpent = TimeSpan.Parse(s[1]);
            bestTime = TimeSpan.Parse(s[2]);
            questionsCompleted = int.Parse(s[3]);
            questionsCompletedCorrect = int.Parse(s[4]);
            questionHighscore = int.Parse(s[5]);
            timesCompleted = int.Parse(s[6]);
            questions = AppPersistentData.LoadFile(filePath, 3, true, 7).Select(s => FlashCard.FlashCardFromString(s)).ToList();
        }

        public void PrintStats()
        {
            SendConsoleMessage($"Highscore: [{questionHighscore} / {questions.Count}] {Math.Round((double)questionHighscore / (double)questions.Count * 100d, 2)}%");
            SendConsoleMessage($"Highscore Time: {bestTime.ToString(@"mm\:ss")}");
            ShiftLine();
            SendConsoleMessage($"Times Completed: {timesCompleted}");
            SendConsoleMessage($"Total Time Spent: {timeSpent.ToString(@"hh\:mm\:ss")}");
            ShiftLine();
            SendConsoleMessage($"Questions Answered: {questionsCompleted}");
            SendConsoleMessage($"Questions Answered Correctley: {questionsCompletedCorrect}");
        }

        public override string ToString()
        {
            return $"{name}\n{timeSpent}\n{bestTime}\n{questionsCompleted}\n{questionsCompletedCorrect}\n{questionHighscore}\n{timesCompleted}\n" + string.Join('\n', questions.Select(q => q.ToString()));
        }
    }

    /// <summary>Class pertaining all information for a flash card question.</summary>
    abstract class FlashCard
    {
        public string promt = "";

        /// <summary>Checks if given answer to flash card is correct.</summary>
        public abstract bool CheckAnswer(string answer);

        public static FlashCard FlashCardFromString(string[] s)
        {
            switch (s[0][0])
            {
                case '0':
                    return new StandardFlashCard(s[1], s[2]);
                case '1':
                    return new MultiChoiceFlashCard(s[1], s[2].Split(':').ToList(), int.Parse(s[0][1].ToString()));
                default:
                    return new FillTheGapFlashCard(s[1], s[2].Split(':').Select(s => int.Parse(s)).ToList());
            }
        }

        public static int GetQuestionType(FlashCard card)
        {
            if (card as StandardFlashCard != null) return 0;
            if (card as MultiChoiceFlashCard != null) return 1;
            return 2;
        }

        public static string[] QuestionTypeName { get { return new string[] { "Standard", "Multi Choice", "Fill The Gap" }; } }
    }

    /// <summary>Class pertaining all information for a flash card question.</summary>F
    class StandardFlashCard : FlashCard
    {
        public string expectedAnswer;

        public StandardFlashCard(string promt, string expectedAnswer)
        {
            this.promt = promt;
            this.expectedAnswer = expectedAnswer;
        }

        /// <summary>Checks if given answer to flash card is correct.</summary>
        public override bool CheckAnswer(string answer)
        {
            //my IDE saids these brackets are unesccary (its lying!)
            return (answer.ToLower() == expectedAnswer.ToLower());
        }

        public override string ToString()
        {
            return $"0\n{promt}\n{expectedAnswer}";
        }
    }

    /// <summary>Class pertaining all information for a flash card question.</summary>
    class MultiChoiceFlashCard : FlashCard
    {
        public List<string> answers;
        public int answerIndex;

        public MultiChoiceFlashCard(string promt, List<string> answers, int answerIndex)
        {
            this.promt = promt;
            this.answers = answers;
            this.answerIndex = answerIndex;
        }

        /// <summary>Checks if given answer to flash card is correct.</summary>
        public override bool CheckAnswer(string answer)
        {
            return (int.Parse(answer) == answerIndex);
        }

        public override string ToString()
        {
            return $"1{answerIndex}\n{promt}\n{string.Join(':', answers)}";
        }
    }

    /// <summary>Class pertaining all information for a flash card question.</summary>
    class FillTheGapFlashCard : FlashCard
    {
        public List<int> questionIndexes;
        public int chosenIndex = 0;

        public FillTheGapFlashCard(string promt, List<int> questionIndexes)
        {
            this.promt = promt;
            this.questionIndexes = questionIndexes;
        }

        public string FormattedQuestion()
        {
            chosenIndex = questionIndexes[Manager.rng.Next(0, questionIndexes.Count)];
            return string.Join(' ', promt.Split(' ').Select((s, index) => chosenIndex == index ? new string('_', s.Length) : s).ToArray());
        }

        public string CorrectAnswer()
        {
            return promt.Split(' ', StringSplitOptions.RemoveEmptyEntries)[chosenIndex];
        }

        /// <summary>Checks if given answer to flash card is correct.</summary>
        public override bool CheckAnswer(string answer)
        {
            return (CorrectAnswer().ToLower() == answer.ToLower());
        }

        public override string ToString()
        {
            return $"2\n{promt}\n{string.Join(':', questionIndexes)}";
        }
    }
}