using kcs_cache;
using kcs_cache.Models;

var minHeight = 10;
var optimalHeight = 14;
var minWidth = 70;
var optimalWidth = 80;
if (Console.WindowHeight < minHeight || Console.WindowWidth < minHeight)
{
    Console.WriteLine($"Minimum terminal window for kvs-cache is {minHeight} height, {minWidth} width.");
    Console.WriteLine("Please enlarge the terminal and rerun");
    return;
}

var height = Math.Min(Console.WindowHeight, optimalHeight);
for (var i = 0; i < height; ++i)
{
    Console.WriteLine();
}

var controller = new Controller(new Rectangle(0,  Math.Max(0, Console.CursorTop - height), Math.Min(Console.WindowWidth, optimalWidth), Math.Min(Console.WindowHeight, optimalHeight)));

Console.CancelKeyPress += delegate(object? _, ConsoleCancelEventArgs e)
{
    controller.Break();
    e.Cancel = true;
};

if (args.Length > 0)
{
    switch (args[0])
    {
        case "--testboard":
            controller.DrawTestBoard();
            break;
        case "--testkeys":
            controller.TestKeys();
            break;
    }
    return;
}

controller.Execute();
