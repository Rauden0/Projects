using System;
using System.IO;

class Advent6
{
    public static (int, int) BinarySearch(int first, int last, long distance, bool small, int time)
    {
        int mid = (first + last) / 2;
        int low = 0;
        int high = 0;
        int dontCare = 0;

        if (first >= last)
        {
            Console.WriteLine(last);
            return (first, last);
        }

        if (Check(mid, distance, time))
        {
            (low, dontCare) = BinarySearch(first, mid, distance, true, time);
            (dontCare, high) = BinarySearch(mid + 1, last, distance, false, time);
        }
        else
        {
            if (small)
            {
                (low, dontCare) = BinarySearch(mid + 1, last, distance, false, time);
            }
            else
            {
                (dontCare, high) = BinarySearch(first, mid, distance, true, time);
            }
        }

        return (low, high);
    }

    public static bool Check(int time, long distance, int total_time)
    {
        return distance < time * (total_time - time);
    }

    public static int Calculate(string time, string distance)
    {
        string[] SplittedTime = time.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string[] SplittedDistance = distance.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        int time_int = 0;
        long distance_int = 0;

        for (int i = 1; i < SplittedTime.Length && i < SplittedDistance.Length; i++)
        {
            time_int *= (int)Math.Pow(10, SplittedTime[i].Length);
            time_int += int.Parse(SplittedTime[i]);

            distance_int *= (long)Math.Pow(10, SplittedDistance[i].Length);
            distance_int += long.Parse(SplittedDistance[i]);
        }

        int a, b;
        (a, b) = BinarySearch(0, time_int, distance_int, false, time_int);
        Console.WriteLine(a);
        return b - a;
    }

    public static void Main()
    {
        string path = "base.txt";
        using (StreamReader reader = new StreamReader(path))
        {
            string time = reader.ReadLine();
            string distance = reader.ReadLine();
            Console.WriteLine(Calculate(time, distance));
        }
    }
}