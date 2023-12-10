class Advent4
{
    public static int ReadTicket(string line,int[] cards, int current_line)
    {
        string[] splitted = line.Split('|');
        string[] winning_numbers = splitted[0].Split(' ');

        string[] my_numbers = splitted[1].Split(' ');
        List<int> winning = new List<int>();
        List<int> mine = new List<int>();
        
        for (int i = 2; i < winning_numbers.Length; i++)
        {
            if (winning_numbers[i] == "")
            {
                continue;
            }
            if (winning_numbers[i].Length > 1)
            {
                if (winning_numbers[i][1] == ':')
                {
                    continue;
                }
                if (winning_numbers[i].Length > 2 && winning_numbers[i][2] == ':')
                {
                    continue;
                }
            }
            winning_numbers[i].Trim();
            winning.Add(int.Parse(winning_numbers[i]));
        }
        for (int i = 0; i < my_numbers.Length; i++)
        {
            if (my_numbers[i] == "")
            {
                continue;
            }
            my_numbers[i].Trim();
            mine.Add(int.Parse(my_numbers[i]));
        }
        int result = 0;
        foreach (int num in mine)
        {
            if (winning.Contains(num))
            {
                result += 1;
            }
        }
        for (int i = current_line+1; i <= current_line +result; i++)
        {
            if (i >= cards.Length)
            {
                continue;
            }
            cards[i] += cards[current_line];
        }
        Console.WriteLine(cards[current_line]);
        return cards[current_line];
    }
    public static void Main()
    {
        string path = "4.txt";
        int total_sum = 0;
        int[]cards = Enumerable.Repeat(1, 1000).ToArray();
        int current_line = 0;
        using (StreamReader reader = new StreamReader(path))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                total_sum += ReadTicket(line,cards,current_line);
                current_line++;
                Console.WriteLine("");
            }
        }
        Console.WriteLine(total_sum);
    }
}
