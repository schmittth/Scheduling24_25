using Projektseminar.Instance;
using Projektseminar.LocalSearch;
using Projektseminar.MetaHeuristic;
using Projektseminar.OpeningHeuristic;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Projektseminar
{
    public class ScheduleRequestsSat
    {
        //Variablen

        //Methoden
        public static void Main(string[] args)
        {
            int seedValue = 0; //Initialisiere eine Int der den Random-Seed für diese Ausführung entält. Setze auf 0

            foreach (string subDir in Directory.GetDirectories("../../../Diagramms/", "Set*"))
            {
                int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; //Generiere sog. Unix-Timestamp.

                List<string> subFiles = new List<string>();
                int instanceAmount = 0;

                subFiles = Directory.GetFiles(subDir, "*.txt", SearchOption.AllDirectories).ToList();
                instanceAmount = subFiles.Count;


                string priorityRule = "LTT"; //Initialisiere PriortityRule String

                Tuple<double, int> simAnnealParams = Tuple.Create(0.99, 5000);

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
                    problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\initialSolution.html", false, seedValue, null);

                }
            }
        }
    }
}