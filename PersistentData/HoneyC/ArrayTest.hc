import "BaseLib.hc";

func Main()
{
    var arr = [1, 2, 3, 4, 5];
    for i in arr {
        Out(arr[i], Colour.Green);
    }
}

Main();