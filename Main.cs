using Projektseminar.Instance;
using Projektseminar.MetaHeuristic;
using Projektseminar.OpeningHeuristic;

namespace Projektseminar
{
    public class ScheduleRequestsSat
    {
        //Variablen

        //Methoden
        public static void Main(string[] args)
        {
            int seedValue = 0; //Initialisiere eine Int der den Random-Seed für diese Ausführung entält. Setze auf 0

            foreach (string subDir in Directory.GetDirectories("../../../Diagramms/", "ClassroomInstance*"))
            {
                int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; //Generiere sog. Unix-Timestamp.

                List<string> subFiles = new List<string>();
                int instanceAmount = 0;

                subFiles = Directory.GetFiles(subDir, "*.txt", SearchOption.TopDirectoryOnly).ToList();
                instanceAmount = subFiles.Count;


                string priorityRule = "LTT"; //Initialisiere PriortityRule String
                string neighboorhood = "N3"; //Initialisiere Nachbarschafts String


                Tuple<double, int> simAnnealParams = Tuple.Create(0.99, 2500);

                //
                for (int instanceCounter = 0; instanceCounter < instanceAmount; instanceCounter++)
                {

                    Random randSeed = new Random();
                    seedValue = randSeed.Next(0, Int32.MaxValue);

                    Importer importer = new Importer();
                    importer.ImportInstanceFromFile(subFiles[instanceCounter]);

                    Problem problem = importer.GenerateProblem();

                    GifflerThompson gifflerThompson = new GifflerThompson(problem, priorityRule);


                    problem = gifflerThompson.InitialSolution();
                    problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\initialSolution.html", false, seedValue, gifflerThompson.Stopwatch.Elapsed);

                    //Solver: Simulated Annealing

                    SimulatedAnnealing simAnneal = new SimulatedAnnealing(problem, simAnnealParams.Item1, simAnnealParams.Item2, neighboorhood);
                    problem = simAnneal.DoSimulatedAnnealing(seedValue);

                    simAnneal.Log(subFiles[instanceCounter], seedValue, simAnneal.Stopwatch.Elapsed, "Simulated Annealing", simAnneal.CoolingFactor, simAnneal.Iterations, simAnneal.Neighboorhood, gifflerThompson.PriorityRule);
                    problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\simAnneal.html", false, seedValue, simAnneal.Stopwatch.Elapsed);
                    //problem.ProblemAsFile($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\instanceExport.txt");


                }
            }


        }
    }
}