Imports System
Imports System.IO

Namespace ExecCsTest
    Module Program1
         Sub Main(args As String())
            Try
                File.Copy("c:\WexflowTesting\file1.txt", "c:\WexflowTesting\ExecVb_dest\file1.txt", True)
                Console.WriteLine("File copied!")
            Catch e As Exception
                Console.WriteLine(e)
            End Try
        End Sub
    End Module
End Namespace
