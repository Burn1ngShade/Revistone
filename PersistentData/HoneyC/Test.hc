enum Colour {
    Black = 0;
    DarkBlue = 1;
    DarkGreen = 2;
    DarkCyan = 3;
    DarkRed = 4;
    DarkMagenta = 5;
    DarkYellow = 6;
    Gray = 7;
    DarkGray = 8;
    Blue = 9;
    Green = 10;
    Cyan = 11;
    Red = 12;
    Magenta = 13;
    Yellow = 14;
    White = 15;
}

obj Cow{
    var x;
    val y;
    var z;
    var babyCow;
    val s = 5;

    obj Cow(x, y, z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.babyCow = Cow(1, 2, 3);
    }

    func Moo()
    {
        for i in Range((x + y + z) * s)
        {
            Out("Moooo!", Colour.Blue);
        }
    }
}

func Main()
{
    Out("This Is My Test Script");
    val cowNumber = 2 * 4 - (2 + 2);
    var cow = Cow(Add(1, 1), (-2 + cowNumber) * 0.5, 3);
    cow.Moo();
    cow.babyCow.Moo();

    if (true == true){
        Out("WOW");
    }

    for i in [2, 4, 5] {
        Out(i, Colour.Green);
    }
}

func Add(x, y){
    return x + y;
}

Main();