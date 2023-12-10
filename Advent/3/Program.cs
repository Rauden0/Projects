

using System; 
using System.Collections.Generic; 
  
class Advent3
{
    public static bool TouchesSymbol(string line, int index,int line_index,List<(int,int)> touch_list)
    {
        for (int i = index -1; i<=index+1;i ++)
        {
            if (i < 0)
            {
                continue;
            }
            if (i >= line.Length)
            {
                continue;
            }
            if (line[i] == '*')
            {
                touch_list.Add((line_index,index));
            }
        }
        return false;

    }
    public static void checkMiddleLine(string[] lines, int current_line, List<(int,List<(int,int)>)> numbers)
    {
        int current = 0;
        List<(int,int)> touch_list = new List<(int, int)>();
        int line_index = 0;
        int index = 0;
        for (int i = 0; i < lines[1].Length; i++)
        {
            if (char.IsDigit(lines[1][i]))
            {
                current *= 10;
                current += (int) lines[1][i] - (int) '0';
            } else
            {
                if(current != 0)
                {
                    numbers.Add((current,touch_list));
                }
                current = 0;
                touch_list = new List<(int, int)>();
                continue;
            }

            if(lines[0].Length == lines[1].Length)
            {
                TouchesSymbol(lines[0],i,current_line-1,touch_list);
            }
            TouchesSymbol(lines[1],i,current_line,touch_list);
            if(lines[2].Length == lines[1].Length)
            {
                TouchesSymbol(lines[2],i,current_line+1,touch_list);
            }   
        }
        if(current != 0)
        {
            numbers.Add((current,touch_list));
        }
    }
    public static void Main()
    {
        string path ="3.txt";
        string[] lines = {"","",""};
        List<List<int>> result = new List<List<int>>();
        int total = 0;
        int current_line = 0;
        using (StreamReader reader = new StreamReader(path))
        {
            while (!reader.EndOfStream)
            {
                List<(int,List<(int,int)>)> nums = new List<(int,List<(int,int)>)>();
                string line = reader.ReadLine();
                for (int i =0; i < 2; i++)
                {
                    lines[i] = lines[i+1];
                }
                lines[2] = line;
                checkMiddleLine(lines, current_line,nums);
                current_line++;
                foreach ((int number,List<(int,int)> idk ) in nums)
                {
                }
                Console.WriteLine("");
            }
            List<(int,List<(int,int)>)> numbers = new List<(int,List<(int,int)>)>();;
            for (int i =0; i < 2; i++)
            {
                lines[i] = lines[i+1];
            }
            lines[2] = "";
            checkMiddleLine(lines,current_line,numbers);
            foreach ((int number,List<(int,int)> idk) in numbers)
            {
                Console.Write(number);
                Console.Write(" ");
                total += number;
            }
            Console.WriteLine("");
        }
        Console.WriteLine(total);
    }
}