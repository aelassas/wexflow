using System;
using System.IO;

namespace ExecCsTest
{
    class Program1
    {
        static void Main(string[] args)
        {
            try
            {
                File.Copy(@"c:\WexflowTesting\file1.txt", @"c:\WexflowTesting\ExecCs_dest\file1.txt", true);
                Console.WriteLine("File copied!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
