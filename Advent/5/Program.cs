class Advent5
{
    public static  List<long> GetSeeds(string ling)
    {
        string[]splitted = ling.Split(' ');
        List<long> seeds = new List<long>();
        for (int i = 1; i < splitted.Length;i++ )
        {
            seeds.Add(long.Parse(splitted[i]));
        }
        return seeds;
    }
    public static void ApplyInterval(List< List<(long,long,long)>> chunk_list,List<long>seeds)
    {
        foreach (List<(long,long,long)> chunk in chunk_list)
        {
            int tmp = seeds.Count;
            int count = 0;
            for (int j = 0; j < chunk.Count; j++)
            {
                for (int i = 0; i < tmp-count; i += 2)
                {
                    long to,from,range;
                    (to,from,range) = chunk[j];
                    ///seeds[i] -> seeds[i] + seeds[i+i] , from - from+range
                    if(seeds[i] + seeds[i+1] < from || from+range < seeds[i])
                    {
                        continue;
                    }
                    long overlapStart = Math.Max(seeds[i],from);
                    long overlapEnd = Math.Min(seeds[i]+seeds[i+1],from+range);
                    seeds.Add(to+(overlapStart-from));
                    seeds.Add(overlapEnd-overlapStart);
                    if (seeds[i] < overlapStart)
                    {
                        seeds.Add(seeds[i]);
                        seeds.Add(seeds[i]-overlapStart);
                    }
                    if (seeds[i]+seeds[i+1] > overlapEnd)
                    {
                        seeds.Add(overlapEnd);
                        seeds.Add((seeds[i]+seeds[i+1]) - overlapEnd);
                    }
                    seeds.RemoveAt(i);
                    seeds.RemoveAt(i);
                    i -= 2;
                    count += 2;
                }
            }
        }
        

    }

    public static (long, long, long) CurrentInterval(string line)
    {
        string[] splitted = line.Split(' ');
        long num1,num2,num3;
        num1 = long.Parse(splitted[0]);
        num2 = long.Parse(splitted[1]);
        num3 = long.Parse(splitted[2]);
        return (num1,num2,num3);
    }
    public static void Main()
    {
        string path = "5.txt";
        using (StreamReader reader = new StreamReader(path))
        {
            string line = reader.ReadLine();
            List<long> seeds = GetSeeds(line);
            reader.ReadLine();
            List< List<(long,long,long)>> chunk_list =new List<List<(long, long, long)>>();
            while (!reader.EndOfStream)
            {
                List<(long,long,long)> current_chunk = new List<(long, long, long)>();
                while (!reader.EndOfStream && (line = reader.ReadLine()) != "" )
                {
                    Console.WriteLine(line);
                    if ( !char.IsDigit(line[0]))
                    {
                        continue;
                    }
                    current_chunk.Add(CurrentInterval(line));
                }
                chunk_list.Add(current_chunk);
            }
            ApplyInterval(chunk_list,seeds);
            long min = 97987989789997887;
            for (int i = 0; i < seeds.Count; i+=2)
            {
                if (seeds[i] < min )
                {
                    min = seeds[i];
                }
            }
            Console.WriteLine(min);
        }
    }

}