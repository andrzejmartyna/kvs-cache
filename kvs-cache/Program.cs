using kcs_cache;
using kcs_cache.Models;

var height = 14;
for (var i = 0; i < height; ++i)
{
    Console.WriteLine();
}

var controller = new Controller(new Rectangle(0, Console.CursorTop - height, 80, 14));

Console.CancelKeyPress += delegate
{
    controller.OnExit();
};

//controller.DrawTestBoard();
//controller.TestKeys();
controller.Execute();
