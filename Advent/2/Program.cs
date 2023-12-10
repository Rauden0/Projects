class Advent2
{
    public static bool checkPlay(string play, int[] colors)
    {
        string[] split =play.Split(' ');
        int i = int.Parse(split[1]);
        if (split[2] == "red")
        {
            if (i > colors[0] )
            {
                colors[0] = i;
            }

        }
        if (split[2] == "blue")
        {
            if (i > colors[1] )
            {
                colors[1] = i;
            }
        }
        if (split[2] == "green")
        {
            if (i > colors[2] )
            {
                colors[2] = i;
            }
        }
        return true;
    }
    public static bool checkSet(string set,int[] colors)
    {
        string[] plays = set.Split(',');
        for(int i = 0; i < plays.Length; i++)
        {
            checkPlay(plays[i],colors);
        }
        return true;
        

    }
    public static bool checkCorrect(string input,int[] colors)
    {
        string[] sets = input.Split(';');
        for(int i = 0; i < sets.Length; i++)
        {
            if (checkSet(sets[i],colors) == false)
            {
                return false;
            }
        }
        return true;

    }
    public static void Main()
    {
        string path ="2.txt";
        int total = 0;
        using (StreamReader reader = new StreamReader(path))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] splitted = line.Split(':');
                string[] game = splitted[0].Split(' ');
                int id = int.Parse(game[1]);
                int[] colors ={0,0,0};
                if (checkCorrect(splitted[1],colors) == false)
                {
                    continue;
                }
                Console.Write(colors[0]);
                Console.Write(" ");
                
                Console.Write(  colors[1]);
                Console.Write(" ");
                
                Console.Write(colors[2]);
                Console.WriteLine("");

                total += colors[0] * colors[1] * colors[2];
            }
       }
       Console.WriteLine(total);
    }   
}