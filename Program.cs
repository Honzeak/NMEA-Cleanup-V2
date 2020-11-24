using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NC2
{
    class Program
    {
        static int getNthIndex(string s, char c, int noc)
        {
            int count = 0;
            for(int i=0;i<s.Length;i++)
            {
                if(s[i]==c)
                    count++;
                if(count==noc)
                    return i;
            }
            return -1;
        }

        static bool checkForMissingData(string line, char separator)
        {
            int firstOccurence = line.IndexOf(separator);
            char prevChar = line[firstOccurence];
            for(int i = firstOccurence+1; i<line.Length;i++)
            {
                if(line[i]==prevChar&&prevChar==separator&&line[i+1]!='*'){return true;}
                prevChar = line[i];
            }
            return false;
        }
        static int Main(string[] args)
        {
           string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            //Define root path
            path = path.Substring(6);

            //Get the input file name
            Console.WriteLine("Ahoj, napis prosim nazev souboru s neopracovanymi GPS daty (i s priponou).\nNazev souboru musi byt ve stejnem adresari jako je tento program.\n\nNazev souboru:");
            string filenameIn = Console.ReadLine();


            bool GSV = false;

            //Chybova hlaska
            while (!File.Exists(Path.Combine(path, filenameIn)))
            {
                Console.WriteLine("Program nemuze najit soubor. Ujisti se ze jsi zadal nazev souboru spravne (s priponou) a ze je ve stejne slozce jako tento program.\nZkus napsat nazev souboru znovu:");
                filenameIn = Console.ReadLine();
            }

            Console.WriteLine("Budeš si přát vypsat i GSV zprávy (SNR data atd.)?\n(A/N):");

            while(true)
            {
                string gsvString = Console.ReadLine();
                if(gsvString.ToUpper() == "A"){GSV = true;break;}
                if(gsvString.ToUpper() == "N"){GSV = false;break;}
                Console.WriteLine("Neocekavany vstup. \nOdpovidej pouze 'a' nebo 'n':");
            }


            //Get the output name
            Console.WriteLine("Nyni napis nazev vystupniho souboru (bez pripony, bude to .csv) a nebo jenom zmackni enter:");
            string filenameOut = Console.ReadLine();

            //Automatic file name if not specified
            filenameOut = (String.IsNullOrEmpty(filenameOut)) ? "GPSoutput" :  filenameOut;
            
            //Automatic file name index
            int i = 0;
            while (File.Exists(Path.Combine(path, filenameOut+".csv")))
            {
                i++;
                if (i == 1)
                {
                    filenameOut+=i.ToString();
                }
                else
                {
                    filenameOut=filenameOut.Remove(filenameOut.Length-1);
                    filenameOut += i.ToString();
                }
            }

            //Add file extension
            filenameOut += ".csv";
            
            //Read input file contents
            string[] lines = File.ReadAllLines(Path.Combine(path, filenameIn));
            int linesCount = lines.GetLength(0);

            //Filter and write files
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, filenameOut)))
            {
                outputFile.WriteLine("NMEA, Message ID, UTC of position fix, Latitude, Direction of latitude, Longitude, Direction of longitude, GPS Quality, Number of SVs in use (range from 00 through to 24+), HDOP, Orthometric height (MSL reference), unit of measure, Geoid separation, unit of measure, Age of differential GPS data record, checksum data");

                //Attrifutes format variables
                int nmeaAttIndex=0;
                string nmeaAttStr = "";
                int nmeaAtt = 0;

                //Check attributes format
                if(lines[50].Substring(0,4)=="NMEA")
                {
                    nmeaAttStr = "NMEA,";
                    nmeaAttIndex = nmeaAttStr.Length;
                    nmeaAtt = 1;
                }

                    foreach (string line in lines)
                    {
                        //Detect the GGA messages
                        if (line.StartsWith(nmeaAttStr+"$GPGGA")|| line.StartsWith(nmeaAttStr+"$GLGGA")|| line.StartsWith(nmeaAttStr+"$GNGGA"))
                        {
                            string edited;
                            int coordCommaIndex = 0;
                            float coord = 0;
                            string coordString;
                            float result;
                            
                            //Replace commas to make working with decimal separators easier
                            edited=line.Replace(',',';');
                            
                            //Discard empty messages
                            if(checkForMissingData(edited,';')){continue;}

                            //Improve readability of the timestamp attribute
                            edited=edited.Insert(getNthIndex(edited,';',1+nmeaAtt)+3, ":");
                            edited=edited.Insert(getNthIndex(edited,';',1+nmeaAtt)+6, ":");


                            ///LATITUDE


                            //Get the 3rd (latitude) attribute
                            coordCommaIndex = getNthIndex(edited,';',2+nmeaAtt);
                            coordString=edited.Substring(coordCommaIndex+1,11);

                            //Get the deg. part of coordinate
                            coord=float.Parse(coordString.Substring(0,2));

                            //Get the minute part of coordinate
                            if(!float.TryParse(coordString.Substring(2), out result))
                            {
                                if(!float.TryParse(coordString.Substring(2).Replace('.',','), out result))
                                    return -1;
                            }

                            //Convert minutes to degrees and add to the deg. part
                            coord+=(result/60);

                            //Replace the non-converted string
                            edited = edited.Replace(coordString, coord.ToString());


                            ///LONGITUDE


                            //Get the 5th attribute (longitude) index location
                            coordCommaIndex = getNthIndex(edited,';',4+nmeaAtt);
                            //Isolate the attribute
                            coordString=edited.Substring(coordCommaIndex+1,12);
                            
                            //Get the deg. part of coordinate
                            coord=float.Parse(coordString.Substring(0,3));
                            
                            //Get the minute part of coordinate
                            if(!float.TryParse(coordString.Substring(3), out result))
                            {
                                if(!float.TryParse(coordString.Substring(3).Replace('.',','), out result))
                                    return -1;
                            }

                            //Convert minutes to degrees and add to the deg. part
                            coord+=(result/60);

                            //Replace the non-converted string
                            edited = edited.Replace(coordString, coord.ToString());

                            //Write into the output file
                            outputFile.WriteLine(edited);
                        }

                        if (line.StartsWith(nmeaAttStr+"$GPGSV")|| line.StartsWith(nmeaAttStr+"$GLGSV")|| line.StartsWith(nmeaAttStr+"$GNGSV"))
                        {

                        }

                    }
            }

                Console.WriteLine("Hotovo! Ocisteny soubor byl vytvoren.");

            Console.WriteLine("\nPro ukonceni programu stiskni enter...");
            Console.ReadLine();
            return 1;
        }
    }
}
