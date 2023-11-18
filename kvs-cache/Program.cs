using kcs_cache;
using kcs_cache.Models;

var height = 14;
for (var i = 0; i < height; ++i)
{
    Console.WriteLine();
}

var top = Console.CursorTop - height;
var operationRectangle = new Rectangle(0, top, 80, 14);

Console.CancelKeyPress += delegate {
    Console.SetCursorPosition(0, operationRectangle.Bottom);
    Console.WriteLine();
};

new Controller(operationRectangle).Start();
Console.WriteLine();
