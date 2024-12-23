val b = 1;

func APlusB(a)
{
    return a + b;
}

func Main()
{
    var a = inp("Enter A Number: ");
    out(APlusB(a));
}

Main();
out("Program End!");