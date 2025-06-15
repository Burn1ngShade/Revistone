using Revistone.Functions;
using Revistone.Interaction;
using Revistone.Console;
using Revistone.Management;
using Revistone.App.Command;

using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.PersistentDataFunctions;
using Revistone.Console.Widget;

//FCS -> Flash Card Set

namespace Revistone.App.BaseApps;

public class FlashCardApp : App
{
    // --- APP BOILER ---

    public FlashCardApp() : base() { }
    public FlashCardApp(string name, string description, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, AppCommand[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, description, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands, 40) { }

    public override App[] OnRegister()
    {
        return [
                new FlashCardApp("Flash Card Manager", "Pratice And Memorise Topics.", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.DarkGreen.ToArray(), ConsoleColor.Green.ToArray()), (Alternate(DarkGreenAndDarkBlue, 6, 3), 5), [], 70, 40)
            ];
    }

    static ConsoleColor[] inputCol = [];

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();

        for (int i = 0; i <= 10; i++) { UpdateLineExceptionStatus(true, i); }

        inputCol = SettingsApp.GetValueAsConsoleColour("Input Text Colour");

        ShiftLine();
        ConsoleLine[] title = TitleFunctions.CreateTitle("REVISE", AdvancedHighlight(64, AppRegistry.PrimaryCol, (AppRegistry.SecondaryCol.ToArray(), 0, 10), (AppRegistry.PrimaryCol.ToArray(), 32, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1);
        SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), title.Length).ToArray());
        ShiftLine();
        MainMenu();
    }

    // --- MENU OPTIONS ---

    /// <summary> Main menu loop.</summary>
    void MainMenu()
    {
        ClearPrimaryConsole();

        UserInput.CreateOptionMenu("--- Options ---", [("Use Flash Cards", MainMenuSelectFCS), ("Edit Flash Cards", MainMenuEditFCS),
                ("Stats", MainMenuStats), ("Exit", ExitApp)]);

        if (AppRegistry.activeApp.name != "Flash Card Manager") return;

        MainMenu();
    }

    /// <summary> Menu to select a FCS to use.</summary>
    void MainMenuSelectFCS()
    {
        string[] flashNames = GetSubFiles(GeneratePath(DataLocation.App, "FlashCard"));
        if (flashNames.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Flash Card Sets Can Be Found!", ConsoleColor.DarkRed));
            UserInput.WaitForUserInput(space: true);
            return;
        }
        int i = UserInput.CreateMultiPageOptionMenu("Flash Card Sets", [.. flashNames.Select(s => new ConsoleLine(StringFunctions.SplitAtCapitalisation(s), AppRegistry.SecondaryCol))], [new ConsoleLine("Exit", AppRegistry.PrimaryCol)], 8);
        if (i >= 0) UseFCS(new FlashCardSet($"FlashCard/{flashNames[i]}"));
    }

    /// <summary> Menu to select a FCS to edit.</summary>
    void MainMenuEditFCS()
    {
        string[] flashNames = GetSubFiles(GeneratePath(DataLocation.App, "FlashCard"));
        int i = UserInput.CreateMultiPageOptionMenu("Flash Card Sets", [new ConsoleLine("New Flash Card Set", AppRegistry.PrimaryCol), .. flashNames.Select(s => new ConsoleLine(StringFunctions.SplitAtCapitalisation(s), AppRegistry.SecondaryCol))], [new ConsoleLine("Exit", AppRegistry.PrimaryCol)], 8);

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

        int i = UserInput.CreateOptionMenu("--- Options ---", ["General Stats", "Flash Card Set Stats", "Exit"]);

        if (i == 0)
        {
            FlashCardSet[] f = GetSubFiles(GeneratePath(DataLocation.App, "FlashCard")).Select(s => new FlashCardSet($"FlashCard/{s}")).ToArray();

            SendConsoleMessage(new ConsoleLine("General Flash Card Manager Stats", AppRegistry.PrimaryCol));
            ShiftLine();
            SendConsoleMessage(new ConsoleLine($"Time Revising: {new TimeSpan(f.Sum(s => s.timeSpent.Ticks)):hh\\:mm\\:ss}", AppRegistry.SecondaryCol));
            SendConsoleMessage(new ConsoleLine($"Flash Card Sets Completed: {f.Sum(s => s.timesCompleted)}", AppRegistry.SecondaryCol));
            ShiftLine();
            SendConsoleMessage(new ConsoleLine($"Total Questions Answered: {f.Sum(s => s.questionsCompleted)}", AppRegistry.SecondaryCol));
            SendConsoleMessage(new ConsoleLine($"Total Questions Correctly: {f.Sum(s => s.questionsCompletedCorrect)}", AppRegistry.SecondaryCol));
            UserInput.WaitForUserInput(space: true);
            MainMenuStats();
            return;
        }
        else if (i == 1)
        {
            string[] flashNames = GetSubFiles(GeneratePath(DataLocation.App, "FlashCard"));
            if (flashNames.Length == 0)
            {
                SendConsoleMessage(new ConsoleLine("No Flash Card Sets Can Be Found!", ConsoleColor.DarkRed));
                UserInput.WaitForUserInput(space: true);
                MainMenuStats();
                return;
            }
            int k = UserInput.CreateMultiPageOptionMenu("Flash Card Sets", [.. flashNames.Select(s => new ConsoleLine(StringFunctions.SplitAtCapitalisation(s), AppRegistry.SecondaryCol))], [new ConsoleLine("Exit", AppRegistry.PrimaryCol)], 8);
            if (k < 0)
            {
                MainMenuStats();
                return;
            }
            FlashCardSet f = new FlashCardSet($"FlashCard/{flashNames[k]}");
            SendConsoleMessage(new ConsoleLine($"Stats For Flash Card Set '{StringFunctions.SplitAtCapitalisation(f.name)}'", AppRegistry.PrimaryCol));
            ShiftLine();
            f.PrintStats();
            UserInput.WaitForUserInput(space: true);
        }
    }

    /// <summary> Loop for flashcard use.</summary>
    // --- FLASH CARD USEAGE ---

    void UseFCS(FlashCardSet s, bool subset = false)
    {
        FlashCard[] shuffledQuestions = [.. s.questions.OrderBy(a => Manager.rng.Next())];
        List<int> correctQuestions = new List<int>();

        DateTime startTime = DateTime.Now;
        TimerWidget.CreateTimer("Flash Cards", "0", true, -int.MaxValue, false); // far left so easier to see

        for (int i = 0; i < shuffledQuestions.Length; i++)
        {
            ClearPrimaryConsole();

            SendConsoleMessage(new ConsoleLine($"Flash Card Set '{StringFunctions.SplitAtCapitalisation(s.name)}' [{i + 1} / {shuffledQuestions.Length}]", AppRegistry.PrimaryCol));
            ShiftLine();

            string answer = "";
            string correctAnswer = "";

            switch (FlashCard.GetQuestionType(shuffledQuestions[i]))
            {
                case 0:
                    StandardFlashCard sfc = (StandardFlashCard)shuffledQuestions[i];
                    SendConsoleMessage(new ConsoleLine("Promt:", AppRegistry.PrimaryCol));
                    SendConsoleMessage(new ConsoleLine(sfc.promt, AppRegistry.SecondaryCol));
                    ShiftLine();
                    answer = UserInput.GetUserInput("Answer: ", clear: true);
                    correctAnswer = sfc.expectedAnswer;
                    break;
                case 1:
                    MultiChoiceFlashCard mcfc = (MultiChoiceFlashCard)shuffledQuestions[i];
                    (string answer, int index)[] shuffledAnswers = [.. mcfc.answers.Select((x, i) => (x, i)).OrderBy(a => Manager.rng.Next())];
                    SendConsoleMessage(new ConsoleLine("Promt:", AppRegistry.PrimaryCol));
                    SendConsoleMessage(new ConsoleLine(mcfc.promt, AppRegistry.SecondaryCol));
                    ShiftLine();
                    answer = shuffledAnswers[UserInput.CreateOptionMenu("Answer: ", shuffledAnswers.Select(x => x.answer).ToArray())].index.ToString();
                    correctAnswer = mcfc.answerIndex.ToString();
                    break;
                case 2:
                    FillTheGapFlashCard ftgfc = (FillTheGapFlashCard)shuffledQuestions[i];
                    SendConsoleMessage(new ConsoleLine("Fill In The Gap:", AppRegistry.PrimaryCol));
                    SendConsoleMessage(new ConsoleLine(ftgfc.FormattedQuestion(), AppRegistry.SecondaryCol));
                    ShiftLine();
                    answer = UserInput.GetUserInput("Answer: ", clear: true);
                    correctAnswer = ftgfc.CorrectAnswer();
                    break;
            }

            if (shuffledQuestions[i].CheckAnswer(answer.Trim()))
            {
                correctQuestions.Add(i);
                SendConsoleMessage(new ConsoleLine("Answer Correct!", AppRegistry.SecondaryCol));
            }
            else
            {
                if (shuffledQuestions[i] is MultiChoiceFlashCard mcfc) // fix UI to show actual answer instead of index
                {
                    answer = mcfc.answers[int.Parse(answer.Trim())];
                    correctAnswer = mcfc.answers[mcfc.answerIndex];
                }
                SendConsoleMessage(new ConsoleLine("Answer Incorrect!", ConsoleColor.DarkRed));
                SendConsoleMessage(new ConsoleLine($"Your Answer: {answer.Trim()}", BuildArray(AppRegistry.SecondaryCol.Extend(12), inputCol)));
                SendConsoleMessage(new ConsoleLine($"Correct Answer: {correctAnswer}", AppRegistry.SecondaryCol));
            }

            if (UserInput.WaitForUserInput([ConsoleKey.Enter, ConsoleKey.E], space: true, customMessage: new ConsoleLine("Press [Enter] To Continue Or [E] To Exit", BuildArray(AppRegistry.PrimaryCol.Extend(6), AppRegistry.SecondaryCol.Extend(7), AppRegistry.PrimaryCol.Extend(16), AppRegistry.SecondaryCol.Extend(3), AppRegistry.PrimaryCol.Extend(8)))) == ConsoleKey.E)
            {
                TimerWidget.CancelTimer("Flash Cards");
                return;
            }
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
        else if (s.questionHighscore > s.questions.Count) // some questions where deleted
        {
            s.questionHighscore = correctQuestions.Count;
            s.bestTime = time;
        }
        else if (s.questionHighscore == correctQuestions.Count && s.bestTime > time) s.bestTime = time;

        TimerWidget.CancelTimer("Flash Cards");

        //stats 
        ClearPrimaryConsole();
        SendConsoleMessage(new ConsoleLine($"'{StringFunctions.SplitAtCapitalisation(s.name)}' Complete!", AppRegistry.PrimaryCol));
        ShiftLine();
        SendConsoleMessage(new ConsoleLine($"Score: [{correctQuestions.Count} / {s.questions.Count}] {Math.Round((double)correctQuestions.Count / (double)s.questions.Count * 100d, 2)}%", AppRegistry.SecondaryCol));
        SendConsoleMessage(new ConsoleLine($"Time: {time:mm\\:ss}", AppRegistry.SecondaryCol));
        ShiftLine();
        s.PrintStats();
        if (!subset) SaveFCS(s);

        if (correctQuestions.Count < s.questions.Count)
        {
            if (UserInput.WaitForUserInput([ConsoleKey.Enter, ConsoleKey.R], space: true, customMessage: new ConsoleLine("Press [Enter] To Continue Or [R] To Retry Incorrect Questions", BuildArray(AppRegistry.PrimaryCol.Extend(6), AppRegistry.SecondaryCol.Extend(7), AppRegistry.PrimaryCol.Extend(16), AppRegistry.SecondaryCol.Extend(3), AppRegistry.PrimaryCol.Extend(19)))) == ConsoleKey.R)
            {
                FlashCardSet incorrectSet = new FlashCardSet($"{s.name}{(subset ? "" : " - Incorrect Answers")}", [.. shuffledQuestions.Where((x, i) => !correctQuestions.Contains(i))]);
                UseFCS(incorrectSet, true);
            }
        }
        else UserInput.WaitForUserInput(space: true);

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
        SendConsoleMessage(new ConsoleLine($"Editing Set '{StringFunctions.SplitAtCapitalisation(s.name)}' Of {s.questions.Count} Questions.", AppRegistry.PrimaryCol));
        ShiftLine();
        bool deleted = false;

        int i = UserInput.CreateOptionMenu("--- Options ---", [
                    ("New Question", () => s.questions.Add(CreateQuestion(s))),
                    ("View Questions", () => ViewQuestions(s)),
                    ("Rename Flash Card Set", () => RenameFCS(s)),
                    ("Delete Flash Card Set", () => deleted = DeleteFCS(s)),
                    ("Exit", () => {})
                ]);

        if (deleted) { return; }

        if (i != 4) EditFCS(s);
        else SaveFCS(s);
    }

    void RenameFCS(FlashCardSet s)
    {
        string newName = UserInput.GetValidUserInput("New Flash Card Set Name: ", new UserInputProfile(UserInputProfile.InputType.FullText, "[C:]", removeWhitespace: true));
        DeleteFile(GeneratePath(DataLocation.App, $"FlashCard/{s.name}"));
        s.name = newName;
        SaveFCS(s);
    }

    /// <summary> Deletes given FCS file.</summary>
    bool DeleteFCS(FlashCardSet s)
    {
        if (UserInput.CreateTrueFalseOptionMenu($"Are You Sure You Want To Delete '{StringFunctions.SplitAtCapitalisation(s.name)}'?"))
        {
            DeleteFile(GeneratePath(DataLocation.App, $"FlashCard/{s.name}"));
            SendConsoleMessage(new ConsoleLine($"'{StringFunctions.SplitAtCapitalisation(s.name)}' Deleted!", AppRegistry.PrimaryCol));
            UserInput.WaitForUserInput(space: true);
            return true;
        }
        else
        {
            SendConsoleMessage(new ConsoleLine($"'{StringFunctions.SplitAtCapitalisation(s.name)}' Deletion Aborted!", AppRegistry.PrimaryCol));
            UserInput.WaitForUserInput(space: true);
            return false;
        }
    }

    /// <summary> Saves given FCS to file.</summary>
    void SaveFCS(FlashCardSet s)
    {
        SaveFile(GeneratePath(DataLocation.App, $"FlashCard/{s.name}"), s.ToString().Split('\n'));
    }

    // QUESTION MODIFICATIONS

    /// <summary> Create a question for a given FCS.</summary>
    FlashCard CreateQuestion(FlashCardSet s, FlashCard? fromFlashCard = null)
    {
        int questionType = UserInput.CreateOptionMenu("Question Type:", ["Standard", "Multi Choice", "Fill The Gap"],
        cursorStartIndex: fromFlashCard == null ? 0 : FlashCard.GetQuestionType(fromFlashCard));
        string promt = UserInput.GetValidUserInput("Create Your Flash Card's Promt:", new UserInputProfile(), fromFlashCard?.promt ?? "");
        switch (questionType)
        {
            case 0:
                string answer = UserInput.GetValidUserInput("Create Your Flash Card's Answer:", new UserInputProfile(), fromFlashCard is StandardFlashCard sfc ? sfc.expectedAnswer : "");
                return new StandardFlashCard(promt, answer);
            case 1:
                List<string> answers = new List<string>();
                while (true)
                {
                    if (answers.Count >= 2 && UserInput.CreateOptionMenu("--- Options --- ", ["New Choice", "Finish"]) == 1)
                    {
                        return new MultiChoiceFlashCard(promt, answers, UserInput.CreateOptionMenu("Select The Correct Answer: ", answers.ToArray()));
                    }
                    answers.Add(UserInput.GetValidUserInput($"Input Flash Card's Answer Choice {answers.Count + 1}:", new UserInputProfile(bannedChars: ":")));
                }
            default:
                if (fromFlashCard is FillTheGapFlashCard ftgfc && UserInput.CreateTrueFalseOptionMenu("Keep Current Fill The Gap Indexes?"))
                {
                    return new FillTheGapFlashCard(promt, ftgfc.questionIndexes);
                }

                List<int> validQuestionIndexes = [];
                while (true)
                {
                    if (validQuestionIndexes.Count > 0 && UserInput.CreateOptionMenu("--- Options --- ", ["New Choice", "Finish"]) == 1)
                    {
                        return new FillTheGapFlashCard(promt, validQuestionIndexes);
                    }
                    SendConsoleMessage(new ConsoleLine(promt, AppRegistry.SecondaryCol));
                    int questionIndex = int.Parse(UserInput.GetValidUserInput($"Input Valid Question Word Index {validQuestionIndexes.Count + 1}:", new UserInputProfile(UserInputProfile.InputType.Int, bannedChars: ":")));
                    ClearLines(1, true);
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
    void ViewQuestions(FlashCardSet s, int questionPointer = 0)
    {
        if (s.questions.Count == 0)
        {
            SendConsoleMessage(new ConsoleLine("This Flash Card Set Has No Questions!", ConsoleColor.DarkRed));
            UserInput.WaitForUserInput(space: true);
            return;
        }

        questionPointer = Math.Min(questionPointer, s.questions.Count - 1);
        int pointer = 0;

        while (true)
        {
            ClearPrimaryConsole();
            SendConsoleMessage(new ConsoleLine($"Flash Card Set '{StringFunctions.SplitAtCapitalisation(s.name)}'", AppRegistry.PrimaryCol));

            SendConsoleMessage(new ConsoleLine($"Question {questionPointer + 1}/{s.questions.Count}", AppRegistry.SecondaryCol));
            SendConsoleMessage(new ConsoleLine($"Type: {FlashCard.QuestionTypeName[FlashCard.GetQuestionType(s.questions[questionPointer])]}", AppRegistry.SecondaryCol));
            ShiftLine();
            SendConsoleMessage(new ConsoleLine($"Promt: {s.questions[questionPointer].promt}", AppRegistry.SecondaryCol));
            switch (FlashCard.GetQuestionType(s.questions[questionPointer]))
            {
                case 0:
                    SendConsoleMessage(new ConsoleLine($"Answer: {((StandardFlashCard)s.questions[questionPointer]).expectedAnswer}", AppRegistry.SecondaryCol));
                    break;
                case 1:
                    SendConsoleMessage(new ConsoleLine($"Choices: ", AppRegistry.SecondaryCol));
                    SendConsoleMessages(((MultiChoiceFlashCard)s.questions[questionPointer]).answers.Select((s, index) => new ConsoleLine($"{index + 1}. {s}", AppRegistry.SecondaryCol)).ToArray());
                    SendConsoleMessage(new ConsoleLine($"Correct Answer: {((MultiChoiceFlashCard)s.questions[questionPointer]).answers[((MultiChoiceFlashCard)s.questions[questionPointer]).answerIndex]}", AppRegistry.SecondaryCol));
                    break;
                case 2:
                    SendConsoleMessage(new ConsoleLine($"Promt Indexes: {((FillTheGapFlashCard)s.questions[questionPointer]).questionIndexes.ToElementString()}", AppRegistry.SecondaryCol));
                    break;
            }
            ShiftLine();
            bool reload = false;

            pointer = UserInput.CreateOptionMenu("--- Options --- ", [
                        (new ConsoleLine("Edit Question", AppRegistry.SecondaryCol), () => { if (UserInput.CreateTrueFalseOptionMenu("Edit Question:")) { s.questions[questionPointer] = CreateQuestion(s, s.questions[questionPointer]); reload = true;}} ),
                        (new ConsoleLine("Delete Question", AppRegistry.SecondaryCol), () => { if (UserInput.CreateTrueFalseOptionMenu("Delete Question:")) { s.questions.RemoveAt(questionPointer); reload = true;}}),
                        (new ConsoleLine("Next Question", AppRegistry.PrimaryCol), () => questionPointer = questionPointer < s.questions.Count - 1 ? questionPointer + 1 : 0),
                        (new ConsoleLine("Last Question", AppRegistry.PrimaryCol), () => questionPointer = questionPointer > 0 ? questionPointer - 1 : s.questions.Count - 1),
                        (new ConsoleLine("Exit", AppRegistry.PrimaryCol), () => {}),
                        ],
            cursorStartIndex: pointer);


            if (reload)
            {
                ViewQuestions(s, questionPointer);
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
            string[] s = LoadFile(GeneratePath(DataLocation.App, filePath), 0, 7);
            name = s[0];
            timeSpent = TimeSpan.Parse(s[1]);
            bestTime = TimeSpan.Parse(s[2]);
            questionsCompleted = int.Parse(s[3]);
            questionsCompletedCorrect = int.Parse(s[4]);
            questionHighscore = int.Parse(s[5]);
            timesCompleted = int.Parse(s[6]);
            questions = LoadFile(GeneratePath(DataLocation.App, filePath), 3, true, 7).Select(s => FlashCard.FlashCardFromString(s)).ToList();
        }

        public void PrintStats()
        {
            SendConsoleMessage(new ConsoleLine($"Highscore: [{questionHighscore} / {questions.Count}] {Math.Round(questionHighscore / (double)questions.Count * 100d, 2)}%", AppRegistry.SecondaryCol));
            SendConsoleMessage(new ConsoleLine($"Highscore Time: {bestTime:mm\\:ss}", AppRegistry.SecondaryCol));
            ShiftLine();
            SendConsoleMessage(new ConsoleLine($"Times Completed: {timesCompleted}", AppRegistry.SecondaryCol));
            SendConsoleMessage(new ConsoleLine($"Total Time Spent: {timeSpent:hh\\:mm\\:ss}", AppRegistry.SecondaryCol));
            ShiftLine();
            SendConsoleMessage(new ConsoleLine($"Questions Answered: {questionsCompleted}", AppRegistry.SecondaryCol));
            SendConsoleMessage(new ConsoleLine($"Questions Answered Correctly: {questionsCompletedCorrect}", AppRegistry.SecondaryCol));
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

        public static string[] QuestionTypeName { get { return ["Standard", "Multi Choice", "Fill The Gap"]; } }
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
            return answer.ToLower().Replace(" ", "") == expectedAnswer.ToLower().Replace(" ", "");
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
            return answer == answerIndex.ToString();
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
            return CorrectAnswer().ToLower().Replace(" ", "") == answer.ToLower().Replace(" ", "");
        }

        public override string ToString()
        {
            return $"2\n{promt}\n{string.Join(':', questionIndexes)}";
        }
    }
}