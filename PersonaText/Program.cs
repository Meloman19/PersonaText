using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PersonaTextLib;
using System.IO;

namespace PersonaText
{
    class Program
    {
        static void Main(string[] args)
        {
            string InputFile;
            string OutputFile;

            if (args.Length >= 4)
            {
                if (args[0] == "-in")
                {
                    InputFile = args[1];

                    if (args[2] == "-out")
                    {
                        OutputFile = args[3];
                    }
                    else return;
                }
                else return;
            }
            else return;

            ParseCommand(InputFile, OutputFile, "");
        }

        private static void ParseCommand(string input, string output, string add)
        {
            if (Path.GetExtension(output).ToLower() == ".txt")
            {
                
            }
        }

    }
}
