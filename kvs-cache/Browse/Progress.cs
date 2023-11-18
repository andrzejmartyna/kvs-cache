using kcs_cache.Models;

namespace kcs_cache.Browse;

public static class Progress
{
    public static void Run(Rectangle rectangle, string prefix, Task workingTask)
    {
        var writtenStarted = false;
        var periodCounterDown = 5;
        var countDots = 0;

        while (workingTask.Status < TaskStatus.RanToCompletion)
        {
            if (periodCounterDown > 0)
            {
                Thread.Sleep(100);
                --periodCounterDown;
                continue;
            }

            if (!writtenStarted)
            {
                Console.SetCursorPosition(rectangle.Left, rectangle.Top);
                Console.WriteLine(prefix);
                writtenStarted = true;
            }

            if (countDots > 7 || prefix.Length + countDots >= rectangle.Width)
            {
                countDots = 0;
                Console.SetCursorPosition(rectangle.Left + prefix.Length, rectangle.Top);
                Console.WriteLine(new string(' ', rectangle.Width - prefix.Length));
            }

            Console.SetCursorPosition(rectangle.Left + prefix.Length + countDots, rectangle.Top);
            Console.Write('.');

            periodCounterDown = 5;
            ++countDots;
        }

        Console.SetCursorPosition(rectangle.Left, rectangle.Top);
        Console.WriteLine(new string(' ', rectangle.Width));
    }
}
