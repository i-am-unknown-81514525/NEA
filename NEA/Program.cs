using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ui;
using ui.core;
using ui.test;

namespace NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Test.Setup();
            ConsoleHandler.ConsoleIntermediateHandler.Setup();
            while (true)
            {
                byte result = ConsoleHandler.ConsoleIntermediateHandler.Read();
                if (result == 3)
                {
                    ConsoleHandler.ConsoleIntermediateHandler.Reset();
                    return;
                }
                Global.InputHandler.Dispatch(result);
            }
        }
    }
}
