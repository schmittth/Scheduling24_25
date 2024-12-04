using Google.OrTools.ConstraintSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektseminar
{
    internal class Dialog
    {
        public static string ChooseInstance()
        {
            //string[] allInstances = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.txt");
            string[] allInstances = Directory.GetFiles("../../../", "*.txt");

            Console.WriteLine("Choose an instance to load by writing the number:");
            for (int i = 1; i <= allInstances.Length; i++)
            {
                Console.WriteLine($"{i}. {allInstances[i - 1]}");
            }
            Console.WriteLine($"{allInstances.Length + 1}. Random Instance");

            string instanceChoice = Console.ReadLine();
            if (Int32.Parse(instanceChoice) > 0 && Int32.Parse(instanceChoice) <= allInstances.Length)
            {
                return allInstances[Int32.Parse(instanceChoice) - 1];
            }
            else if (Int32.Parse(instanceChoice) == allInstances.Length + 1)
            {
                return "Random";
            }
            return "NaN";
        }

        public static int ChooseSolver()
        {
            Console.WriteLine("Loading Successful. Please choose your Solver:");
            string[] availableSolvers = { "1. Google OR-Tools", "2. Simulated Annealing (Preferred)", "3. Local Search" };

            foreach (string solver in availableSolvers)
            {
                Console.WriteLine(solver);
            }
            string solverChoice = Console.ReadLine();
            return Int32.Parse(solverChoice);
        }

        public static string ChoosePriorityRule()
        {
            Console.WriteLine("Please choose a Priority rule: ");
            string[] availableRules = { "LPT", "SPT", "LTT", "STT" };
            Console.Write("Currently supported rules: ");

            int i = 1;
            foreach (string rule in availableRules)
            {
                Console.Write(rule);
                if (i < availableRules.Length)
                {
                    Console.Write(",");
                    i++;
                }
                else
                {
                    Console.WriteLine("");
                }
            }

            string solverChoice = Console.ReadLine();
            return solverChoice;
        }

        public static Tuple<int, double, int> ChooseSimAnnealParameters()
        {
            Console.WriteLine("Please provide a starting temperature:");
            int temperature = Int32.Parse(Console.ReadLine());

            Console.WriteLine("Please provide a cooling factor (Please use , for decimal values) :");
            double coolingFactor = Double.Parse(Console.ReadLine());

            Console.WriteLine("How many iterations should be ran for each temperature:");
            int iterations = Int32.Parse(Console.ReadLine());

            return Tuple.Create(temperature, coolingFactor, iterations);
        }

        public static string ChooseNeighboorhood()
        {
            Console.WriteLine("Please choose a neighboorhood:");
            string[] availableNeighboorhoods = { "N1", "N3" };
            Console.Write("Currently supported neighboorhoods: ");

            int i = 1;
            foreach (string neighboorhood in availableNeighboorhoods)
            {
                Console.Write(neighboorhood);
                if (i < availableNeighboorhoods.Length)
                {
                    Console.Write(",");
                    i++;
                }
                else
                {
                    Console.WriteLine("");
                }
            }

            string neighboorhoodChoice = Console.ReadLine();
            return neighboorhoodChoice;
        }

        public static Tuple<int,int,int,int,int,int> ChooseRandomInstanceSize()
        {
            int jobsChoiceInt;
            string jobsChoiceString;
            do
            {
                Console.WriteLine("Please provide an amount of jobs:");
                jobsChoiceString = Console.ReadLine();

            } while (!(int.TryParse(jobsChoiceString, out jobsChoiceInt)));

            int machineChoiceInt;
            string machineChoiceString;
            do
            {
                Console.WriteLine("Please provide a number of machines:");
                machineChoiceString = Console.ReadLine();

            } while (!(int.TryParse(machineChoiceString, out machineChoiceInt)));

            Console.WriteLine("Please type your seed value for random Instance: (Default value: \"Random\")");
            int seedChoiceInt = 0;
            string seedChoiceString = Console.ReadLine(); 
            
            if (!(int.TryParse(seedChoiceString, out seedChoiceInt)))
            {
                Console.WriteLine("Choose default value: \"Random\"");
            }

            Console.WriteLine("Please type the minimal amount of task each job should have for random Instance: (Default value: \"1\")");
            int minTaskPerJobInt = 1;
            string minTaskPerJobString = Console.ReadLine();

            if (!(int.TryParse(minTaskPerJobString, out minTaskPerJobInt)))
            {
                Console.WriteLine("Choose default value: \"1\"");
            }

            Console.WriteLine("Please type the minimal task and setup time for random Instance: (Default value: \"10\")");
            int minTaskTimeInt = 10;
            string minTaskTimeString = Console.ReadLine();

            if (!(int.TryParse(minTaskTimeString, out minTaskTimeInt)))
            {
                Console.WriteLine("Choose default value: \"10\"");
            }

            Console.WriteLine("Please type your maximal task and setup time for random Instance: (Default value: \"99\")");
            int maxTaskTimeInt = 100;
            string maxTaskTimeString = Console.ReadLine();

            if (!(int.TryParse(seedChoiceString, out seedChoiceInt)))
            {
                Console.WriteLine("Choose default value: \"99\"");
            }
            else
            {
                maxTaskTimeInt += 1;
            }

            return Tuple.Create(jobsChoiceInt, machineChoiceInt, seedChoiceInt, minTaskPerJobInt, minTaskTimeInt, maxTaskTimeInt);
        }
    }
}
