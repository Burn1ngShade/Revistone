obj Cow{
    var x;
    val y;
    var z;
    val s = 5;

    Cow(x, y, z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
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
    var cow = Cow(Add(1, 1), -2, 3);
    Out(cow.Moo());

    for i in [2, 4, 5] {
        Out(i, Colour.Green);
    }
}

func Add(x, y){
    return x + y;
}

Main();