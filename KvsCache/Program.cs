using KvsCache;
using KvsCache.Models.Geometry;

const int minHeight = 10;
const int optimalHeight = 20;
const int minWidth = 70;
const int optimalWidth = 80;

var currentHeight = Console.WindowHeight;
var currentWidth = Console.WindowWidth;
if (currentHeight < minHeight || currentWidth < minHeight)
{
    Console.WriteLine($"The terminal window is too small ({currentHeight} height, {currentWidth} width).");
    Console.WriteLine($"Minimum size for kvs-cache is {minHeight} height, {minWidth} width; optimal is {optimalHeight} height, {optimalWidth} width.");
    Console.WriteLine("Please enlarge the terminal and rerun.");
    return;
}

var height = Math.Min(currentHeight, optimalHeight);
for (var i = 0; i <= height; ++i)
{
    Console.WriteLine();
}

var controller = new Controller(new Rectangle(0, Math.Max(0, Console.CursorTop - height), Math.Min(currentWidth, optimalWidth), Math.Min(currentHeight, optimalHeight)));

Console.CancelKeyPress += delegate (object? _, ConsoleCancelEventArgs e)
{
    controller.Break();
    e.Cancel = true;
};

var testSleep = 0;
if (args.Length > 0)
{
    switch (args[0])
    {
        case "--testboard":
            controller.DrawTestBoard();
            return;
        case "--testkeys":
            controller.TestKeys();
            return;
        case "--testsleep":
            testSleep = args.Length >= 2 && int.TryParse(args[1], out var testSleepGiven) ? testSleepGiven : 4000;
            break;
    }
}

controller.Execute(testSleep);
