using System;  
using System.IO;  
using System. Text; 

class Advent1
{
    public static int StringCheckAndConversion(string line,int starting_index)
    {
        string[]Digits = {"one","two","three","four","five","six","seven","eight","nine"};
        int i = starting_index;
        if (i < 2)
        {
            return -1;
        }
        string Sub = line.Substring(i-2,3);
        for (int j= 0; j < 9;j++)
        {
            if (Sub == Digits[j])
            {
                return j+1;
            }
        }
        if (i < 3)
        {
            return -1;
        }
        Sub = line.Substring(i-3,4);
        for (int j = 0; j < 9; j++)
        {
            if (Sub == Digits[j])
            {
                return j+1;
            }
        }
        if (i < 4)
        {
            return -1;
        }
        Sub = line.Substring(i-4,5);
        for (int j = 0; j < 9; j++)
        {
            if (Sub == Digits[j])
            {
               
                return j+1;
            }
        }
        return -1;

    }
    public static void Main()
    {
        string path = "1.txt";
        int final = 0; 
        int count = 0;
        using (StreamReader reader = new StreamReader(path))
        {
            while (!reader.EndOfStream)
            {
                int first = -1;
                int last = 0;
                string line = reader.ReadLine();
                int convertString = 0;
        
                for (int i = 0; i < line.Length; i++)
                {
                    if (char.IsDigit(line[i]))
                    {
                        if (first == -1)
                        {
                            first = (int) line[i] - (int) '0';

                        }
                        last = (int) line[i] - (int) '0';
                    }
                
                    convertString = StringCheckAndConversion(line,i);
                    if (convertString == -1)
                    {
                        continue;
                    }
                    last = convertString;
                    if (first == -1)
                    {
                        first = convertString; 
                    }
                }
            
                final += first*10 + last;
                //Console.WriteLine(first*10 + last);
            }
        }
        Console.WriteLine(final);
    }
}

