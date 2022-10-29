using System;
using System.IO;

namespace ExecCsTest
{
    class Program2
    {
        static void Main(string[] args)
        {
            try
            {
                File.Copy(@"c:\WexflowTesting\file2.txt", @"c:\WexflowTesting\ExecCs_dest\file2.txt", true);
                Console.WriteLine("File copied!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
