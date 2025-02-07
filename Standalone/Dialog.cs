using System.Linq;

namespace Projektseminar.Standalone
{
    internal class Dialog
    {
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
            while (!(int.TryParse(instanceChoiceString, out instanceChoiceInt) && instanceChoiceInt > 0 && instanceChoiceInt <= allInstances.Length + allSubDirectories.Count + allDirectories.Count)); //Erzwinge Auswahl erneut wenn nicht innerhalb der Grenzen

            //Gebe Dateipfad oder "Random" zurück
            if (instanceChoiceInt <= allInstances.Length)
            {
                instanceChoiceString = allInstances[instanceChoiceInt - 1];
            }
            else if (instanceChoiceInt > allInstances.Length && instanceChoiceInt <= allInstances.Length + allDirectories.Count)
            {
                instanceChoiceString = allDirectories[instanceChoiceInt - (allInstances.Length + 1)];
            }
            else
            {
                instanceChoiceString = allSubDirectories[instanceChoiceInt - (allInstances.Length + allDirectories.Count + 1)];
            }
           return instanceChoiceString;      
        }
    }
}
