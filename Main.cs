using Projektseminar.Algorithms;
using Projektseminar.Instance;
using Projektseminar.Standalone;

namespace Projektseminar
{
    public class ScheduleRequestsSat
    {
        //Variablen

        //Methoden
        public static void Main(string[] args)
        {
            int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; //Generiere sog. Unix-Timestamp.
            int seedValue = 0; //Initialisiere eine Int der den Random-Seed für diese Ausführung entält. Setze auf 0

            string instanceChoice = Dialog.ChooseInstance(); //Bestimme ob randomisierte oder feste Instanz        

            List<string> subFiles = new List<string>();
            int instanceAmount = 0;

            if (instanceChoice.EndsWith(".txt")) //When ein .txt-File selektiert wird nur eine Instanz geladen
            {
                instanceAmount = 1;
            }
            else if (instanceChoice.EndsWith("\\"))
            {
                foreach (string subDir in Directory.GetDirectories(instanceChoice))
                {
                    foreach (string subFile in Directory.GetFiles(subDir, "*.txt", SearchOption.TopDirectoryOnly))
                    {
                        subFiles.Add(subFile);
                    }
                }
                instanceAmount = subFiles.Count;
            }
            else
            {
                subFiles = Directory.GetFiles(instanceChoice, "*.txt", SearchOption.TopDirectoryOnly).ToList();
                instanceAmount = subFiles.Count;
            }

            string priorityRule = "LTT"; //Initialisiere PriortityRule String
            string neighborhood = "N3"; //Initialisiere Nachbarschafts String

            //
            for (int instanceCounter = 0; instanceCounter < instanceAmount; instanceCounter++)
            {
                Random randSeed = new Random();
                seedValue = randSeed.Next(0, Int32.MaxValue);

                Importer importer = new Importer();
                if (instanceChoice.EndsWith(".txt"))
                {
                    importer.ImportInstanceFromFile(instanceChoice);
                }
                else
                {
                    importer.ImportInstanceFromFile(subFiles[instanceCounter]);
                }

                Problem problem = importer.GenerateProblem();

                GifflerThompson gifflerThompson = new GifflerThompson(problem, priorityRule);

                problem = gifflerThompson.InitialSolution();
                problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\initialSolution.html", false, seedValue, gifflerThompson.Stopwatch.Elapsed);


                SimulatedAnnealing simAnneal = new SimulatedAnnealing(problem, 0.99, 2500, neighborhood);
                problem = simAnneal.DoSimulatedAnnealing(seedValue);

                simAnneal.Log(instanceChoice, seedValue, simAnneal.Stopwatch.Elapsed, "Simulated Annealing", simAnneal.CoolingFactor, simAnneal.Iterations, simAnneal.Neighborhood, gifflerThompson.PriorityRule);
                problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\simAnneal.html", false, seedValue, simAnneal.Stopwatch.Elapsed);

            }
        }
    }
}