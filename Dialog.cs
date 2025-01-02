using System.Linq;

namespace Projektseminar
{
    internal class Dialog
    {
        public static int ChooseInstanceAmount()
        {
            int instanceAmountChoiceInt;
            string instanceAmountChoiceString;

            do
            {
                Console.WriteLine("Choose how many instances you want to test:");
                instanceAmountChoiceString = Console.ReadLine(); //Lese Instanzauswahl ein
                if (instanceAmountChoiceString == "")
                {
                    instanceAmountChoiceInt = 1;
                    break;
                }

            } while (!(int.TryParse(instanceAmountChoiceString, out instanceAmountChoiceInt) && instanceAmountChoiceInt != 0));

            return instanceAmountChoiceInt;
        }
        public static string ChooseInstance()
        {
            string[] allInstances = Directory.GetFiles("../../../", "*.txt");
            List<string> allDirectories = new List<string>();
            List<string> allSubDirectories = new List<string>();

            foreach (string superDir in Directory.GetDirectories("../../../Diagramms")) //Alle Ordner unter Diagramm - d.h. Unix-Timestamp
            {
                if (Directory.GetFiles(superDir, "*.txt", SearchOption.TopDirectoryOnly).Any())
                {
                    allDirectories.Add(superDir);
                }
                foreach (string subDir in Directory.GetDirectories(superDir)) //Alle Ordner unter Diagramm - d.h. Unix-Timestamp
                {
                    if (Directory.GetFiles(subDir, "*.txt", SearchOption.TopDirectoryOnly).Any())
                    {
                        allSubDirectories.Add(superDir + "\\");
                        break;
                    }
                }
            }
            int instanceChoiceInt;
            string instanceChoiceString;

            do
            {
                Console.WriteLine("Choose an instance to load by writing the number:");
                Console.WriteLine($"{0}. Random Instance");

                //Gebe alle .txt-files direkt in der Projektmappe aus
                Console.WriteLine("Single Instances:");
                for (int i = 1; i <= allInstances.Length; i++)
                {
                    Console.WriteLine($"{i}. {allInstances[i - 1]}");
                }

                //Gebe alle .txt-files aus die direkt in einem Unterordner sind
                Console.WriteLine("Files in Directory:");
                for (int k = allInstances.Length + 1; k <= allDirectories.Count + allInstances.Length; k++)
                {
                    Console.WriteLine($"{k}. {allDirectories[k - (allInstances.Length + 1)]}");
                }

                //Gebe alle .txt-files aus die in einem Unterordner des Unterordners ist
                Console.WriteLine("Files in Sub-Directory:");
                for (int j = allInstances.Length + allDirectories.Count + 1; j <= allSubDirectories.Count + allInstances.Length + allDirectories.Count; j++)
                {
                    Console.WriteLine($"{j}. {allSubDirectories[j - (allInstances.Length + allDirectories.Count + 1)]}");
                }
                instanceChoiceString = Console.ReadLine(); //Lese Instanzauswahl ein
            }
            while (!(int.TryParse(instanceChoiceString, out instanceChoiceInt) && instanceChoiceInt >= 0 && instanceChoiceInt <= allInstances.Length + allSubDirectories.Count + allDirectories.Count)); //Erzwinge Auswahl erneut wenn nicht innerhalb der Grenzen

            //Gebe Dateipfad oder "Random" zurück
            if (instanceChoiceInt == 0)
            {
                instanceChoiceString = "Random";
            }
            else if (instanceChoiceInt <= allInstances.Length)
            {
                instanceChoiceString = allInstances[instanceChoiceInt - 1];
            }
            else if (instanceChoiceInt > allInstances.Length && instanceChoiceInt <= (allInstances.Length + allDirectories.Count))
            {
                instanceChoiceString = allDirectories[instanceChoiceInt - (allInstances.Length + 1)];
            }
            else
            {
                instanceChoiceString = allSubDirectories[instanceChoiceInt - (allInstances.Length + allDirectories.Count + 1)];
            }
            return instanceChoiceString;
        }

        public static int ChooseSolver()
        {
            string[] availableSolvers = { "1. Google OR-Tools", "2. Simulated Annealing (Preferred)", "3. Local Search" };
            int solverChoiceInt;
            string solverChoiceString;

            do
            {
                Console.WriteLine("Loading Successful. Please choose your Solver:");

                foreach (string solver in availableSolvers)
                {
                    Console.WriteLine(solver);
                }
                solverChoiceString = Console.ReadLine();
            }
            while (!(int.TryParse(solverChoiceString, out solverChoiceInt) && solverChoiceInt > 0 && solverChoiceInt <= availableSolvers.Length));

            return solverChoiceInt;
        }

        public static string ChoosePriorityRule()
        {
            Console.WriteLine("Please choose a Priority rule: ");
            string[] availableRules = { "LPT", "SPT", "LTT", "STT" };
            Console.Write("Currently supported rules: ");
            string ruleChoice;

            do
            {
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
                ruleChoice = Console.ReadLine();
            }
            while (!availableRules.Contains(ruleChoice));

            return ruleChoice;
        }

        public static Tuple<double, int> ChooseSimAnnealParameters()
        {
            Console.WriteLine("Please provide a cooling factor (Please use , for decimal values) :");
            double coolingFactor = 0.8;

            Console.WriteLine("How many iterations should be ran for each temperature:");
            int iterations = Int32.Parse(Console.ReadLine());
            return Tuple.Create(coolingFactor, iterations);
        }

        public static int SeedAlgorithm()
        {
            Console.WriteLine("Please type your seed value for random Instance: (Default value: \"Random\")");
            int seedChoiceInt = 0;
            string seedChoiceString = Console.ReadLine();

            if (!(int.TryParse(seedChoiceString, out seedChoiceInt)))
            {
                Console.WriteLine("Choose default value: \"Random\"");
            }

            return seedChoiceInt;
        }

        public static string ChooseNeighboorhood()
        {
            Console.WriteLine("Please choose a neighboorhood:");
            string[] availableNeighboorhoods = { "N1", "N3", "N5" };
            Console.Write("Currently supported neighboorhoods: ");
            string neighboorhoodChoice;

            do
            {
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
                neighboorhoodChoice = Console.ReadLine();
            }
            while (!availableNeighboorhoods.Contains(neighboorhoodChoice));

            return neighboorhoodChoice;
        }

        public static Tuple<int, int, int, int, int> ChooseRandomInstanceSize()
        {
            int jobsChoiceInt;
            string jobsChoiceString;

            do
            {
                Console.WriteLine("Please provide an amount of jobs:");
                jobsChoiceString = Console.ReadLine();

            }
            while (!(int.TryParse(jobsChoiceString, out jobsChoiceInt) && jobsChoiceInt != 0));

            int machineChoiceInt;
            string machineChoiceString;
            do
            {
                Console.WriteLine("Please provide a number of machines:");
                machineChoiceString = Console.ReadLine();
            }
            while (!(int.TryParse(machineChoiceString, out machineChoiceInt) && machineChoiceInt != 0));

            int minTaskPerJobInt = 1;
            string minTaskPerJobString;
            do
            {
                Console.WriteLine("Please type the minimal amount of task each job should have for random Instance: (Default value: \"1\")");
                minTaskPerJobString = Console.ReadLine();
                if (minTaskPerJobString == "")
                {
                    Console.WriteLine("Choose default value: \"1\"");
                    minTaskPerJobInt = 1;
                    break;
                }
            }
            while (!(int.TryParse(minTaskPerJobString, out minTaskPerJobInt) && minTaskPerJobInt != 0 && minTaskPerJobInt <= machineChoiceInt));

            int minTaskTimeInt = 10;
            string minTaskTimeString;
            do
            {
                Console.WriteLine("Please type the minimal task and setup time for random Instance: (Default value: \"10\")");
                minTaskTimeString = Console.ReadLine();
                if (minTaskTimeString == "")
                {
                    Console.WriteLine("Choose default value: \"10\"");
                    minTaskTimeInt = 10;
                    break;
                }
            }
            while (!(int.TryParse(minTaskTimeString, out minTaskTimeInt) && minTaskTimeInt != 0));

            int maxTaskTimeInt = 100;
            string maxTaskTimeString;
            do
            {
                Console.WriteLine("Please type your maximal task and setup time for random Instance: (Default value: \"99\")");
                maxTaskTimeString = Console.ReadLine();
                if (maxTaskTimeString == "")
                {
                    Console.WriteLine("Choose default value: \"99\"");
                    maxTaskTimeInt = 99;
                    break;
                }
                else
                {
                    maxTaskTimeInt += 1;
                }
            }
            while (!(int.TryParse(maxTaskTimeString, out maxTaskTimeInt) && maxTaskTimeInt > minTaskTimeInt));

            return Tuple.Create(jobsChoiceInt, machineChoiceInt, minTaskPerJobInt, minTaskTimeInt, maxTaskTimeInt);
        }
    }
}
