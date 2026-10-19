using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
namespace AddUserInterface
{
    internal class Classes
    {
        string fileName = @"animals.txt";
        public void TextWriter(int animalID, string animalName, string animalSpecies, int animalAge, int animalRecoveryScore)
        {
            string housingUnit;
            string status;
            if (File.Exists(fileName))
            {
                //This is where we search for an existing ID.
                //The boolean variable will help to determine if the ID does exist or not.
                bool Exist=false;
                List <string> animalInfolist = new List <string>();
                animalInfolist = File.ReadAllLines(fileName).ToList();
                foreach(string line in animalInfolist)
                {
                    string[] ids = line.Split(',');
                    if (int.Parse(ids[0]) == animalID)
                    {
                        Exist = true;
                    }
                }
                //if the ID does not exist It will continue to add the new information.
                //Other wise if the ID does exist I message will pop up and say that the ID exists and it will not add new information to the file.
                if(Exist== true)
                {
                    MessageBox.Show("ID already exists.");
                }
                else
                {
                    FileStream myStream = new FileStream(fileName, FileMode.Append);
                    StreamWriter writer = new StreamWriter(myStream);
                    switch (animalRecoveryScore)
                    {
                        case int a when a >= 0 && a <= 19:
                            housingUnit = "Intensive Care unit";
                            status = "Critical";
                            writer.WriteLine(animalID + ", " + animalName + ", " + animalSpecies + ", " + animalAge + ", " + animalRecoveryScore + ", " + status + ", " + housingUnit);
                            break;
                        case int a when a >= 20 && a < 39: // Uses standard && operator
                            housingUnit = "High-Dependancy Ward";
                            status = "Serious";
                            writer.WriteLine(animalID + ", " + animalName + ", " + animalSpecies + ", " + animalAge + ", " + animalRecoveryScore + ", " + status + ", " + housingUnit);
                            break;
                        case int a when a >= 40 && a <= 59:
                            housingUnit = "Recovery Ward";
                            status = "Stable";
                            writer.WriteLine(animalID + ", " + animalName + ", " + animalSpecies + ", " + animalAge + ", " + animalRecoveryScore + ", " + status + ", " + housingUnit);
                            break;
                        case int a when a >= 60 && a <= 79:
                            housingUnit = "Outdoor Enclosure";
                            status = "Recovering";
                            writer.WriteLine(animalID + ", " + animalName + ", " + animalSpecies + ", " + animalAge + ", " + animalRecoveryScore + ", " + status + ", " + housingUnit);
                            break;
                        case int a when a >= 80 && a <= 100:
                            housingUnit = "Pre-Release Camp";
                            status = "Release-Ready";
                            writer.WriteLine(animalID + ", " + animalName + ", " + animalSpecies + ", " + animalAge + ", " + animalRecoveryScore + ", " + status + ", " + housingUnit);
                            break;

                        default:
                            Console.WriteLine("Please enter a number between 0 and a 100");
                            break;
                    }

                    //writer.WriteLine("successful!!");
                    writer.Close();
                    myStream.Close();
                }

            }
            else
            {
                FileStream myStream = new FileStream(fileName, FileMode.Create);
                StreamWriter writer = new StreamWriter(myStream);
                switch (animalRecoveryScore)
                {
                    case int a when a >= 0 && a <= 19:
                        housingUnit = "Intensive Care unit";
                        status = "Critical";
                        writer.WriteLine(animalID + ", " + animalName + ", " + animalSpecies + ", " + animalAge + ", " + animalRecoveryScore + ", " + status + ", " + housingUnit);
                        break;
                    case int a when a >= 20 && a < 39: // Uses standard && operator
                        housingUnit = "High-Dependancy Ward";
                        status = "Serious";
                        writer.WriteLine(animalID + ", " + animalName + ", " + animalSpecies + ", " + animalAge + ", " + animalRecoveryScore + ", " + status + ", " + housingUnit);
                        break;
                    case int a when a >= 40 && a <= 59:
                        housingUnit = "Recovery Ward";
                        status = "Stable";
                        writer.WriteLine(animalID + ", " + animalName + ", " + animalSpecies + ", " + animalAge + ", " + animalRecoveryScore + ", " + status + ", " + housingUnit);
                        break;
                    case int a when a >= 60 && a <= 79:
                        housingUnit = "Outdoor Enclosure";
                        status = "Recovering";
                        writer.WriteLine(animalID + ", " + animalName + ", " + animalSpecies + ", " + animalAge + ", " + animalRecoveryScore + ", " + status + ", " + housingUnit);
                        break;
                    case int a when a >= 80 && a <= 100:
                        housingUnit = "Pre-Release Camp";
                        status = "Release-Ready";
                        writer.WriteLine(animalID + ", " + animalName + ", " + animalSpecies + ", " + animalAge + ", " + animalRecoveryScore + ", " + status + ", " + housingUnit);
                        break;

                    default:
                        Console.WriteLine("Please enter a number between 0 and a 100");
                        break;
                }
                writer.Close();
                myStream.Close();
            }
        }
    }
}

