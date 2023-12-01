using KvsCache.ConsoleDraw;
using KvsCache.Models.Geometry;

namespace KvsCache.Browse;

public static class Progress
{
    public static void Run(Action action, ConsoleUi console, Rectangle rectangle, string prefix)
    {
        var taskWorking = new Task(action);
        var taskProgress = new Task(() => Run(console, rectangle, prefix, taskWorking));
        taskWorking.Start();
        taskProgress.Start();
        taskWorking.Wait();
        taskProgress.Wait();
    }
    
    private static void Run(ConsoleUi console, Rectangle rectangle, string prefix, Task workingTask)
    {
        var writtenStarted = false;
        var periodCounterDown = 5;
        var countDots = 0;

        while (!workingTask.IsCompleted)
        {
            if (periodCounterDown > 0)
            {
                Thread.Sleep(100);
                --periodCounterDown;
                continue;
            }

            if (!writtenStarted)
            {
                console.WriteAt(rectangle.Left, rectangle.Top, prefix + new string(' ', rectangle.Width - prefix.Length));
                writtenStarted = true;
            }

            if (countDots > 7 || prefix.Length + countDots >= rectangle.Width)
            {
                countDots = 0;
                console.WriteAt(rectangle.Left + prefix.Length, rectangle.Top, new string(' ', rectangle.Width - prefix.Length));
            }

            console.WriteAt(rectangle.Left + prefix.Length + countDots, rectangle.Top, ".");

            periodCounterDown = 5;
            ++countDots;
        }

        console.WriteAt(rectangle.Left, rectangle.Top, new string(' ', rectangle.Width));
    }
}
