import "BaseLib.hc";

obj NumberGame 
{ 
    val testOperators = [1 + 1, Add(1, 2)];
    val operators = ["+", "-", "*", "/"]; # This is an example end of line comment  
    var score = 0;
    var question = "";
    var answer = 0;

    func GenQuestion()
    {
        var numOne = /# this is an example midline comment #/  Random.RangeInt(-10, 11); 
        var numTwo = Random.RangeInt(-10, 11); 
        var op = Random.RangeInt(0, 3);
        
        if (op == 3 && (numOne == 0 || numTwo == 0)) { numTwo += 1; }

        question = "{numOne} {operators[op]} {numTwo} = ";
        if (op == 0) { answer = numOne + numTwo; }
        else if (op == 1) { answer = numOne - numTwo; }
        else if (op == 2) { answer = numOne * numTwo; }
        else if (op == 3) { answer = numOne / numTwo; }

        Out("### Question Generated ###");
    }
}

func Main() {
    Out("--- HoneyC Number Game ---", Colour.Magenta);
    
    var numGame = NumberGame();
    while (true)
    {
        numGame.GenQuestion();
        if (In(numGame.question, Colour.DarkBlue) != numGame.answer)
        {
            Out("You Stupid Idiot You Got It Wrong! Answer: {numGame.answer}", Colour.Magenta);
            Out("Final Score: {numGame.score}", Colour.Magenta);
            In("Press Enter To Play Again...", Colour.Magenta);
            Main();
            return;
        }        
        numGame.score += 1;
        Out("Correct! Current Score: {numGame.score}");
    }
}

Main();