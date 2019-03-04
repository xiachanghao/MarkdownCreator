using System;
using System.Linq;
using System.IO;
using System.Text;

namespace MarkdownCreator.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var markdownCreator = new Core.Creator();
            markdownCreator.AutoTransform();

            System.Console.ReadKey();
        }
    }
}
