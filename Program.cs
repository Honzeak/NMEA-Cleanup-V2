﻿using System;
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
        static void Main(string[] args)
        {
           string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);

            path = path.Substring(6);

            Console.WriteLine("Ahoj, napis prosim nazev souboru s neopracovanymi GPS daty (i s priponou).\nNazev souboru musi byt ve stejnem adresari jako je tento program.\n\nNazev souboru:");

            string filenameIn = Console.ReadLine();

            while (!File.Exists(Path.Combine(path, filenameIn)))
            {
                Console.WriteLine("Program nemuze najit soubor. Ujisti se ze jsi zadal nazev souboru spravne (s priponou) a ze je ve stejne slozce jako tento program.\nZkus napsat nazev souboru znovu:");
                filenameIn = Console.ReadLine();
            }

            Console.WriteLine("Nyni napis nazev vystupniho souboru (bez pripony, bude to .csv) a nebo jenom zmackni enter:");

            string filenameOut = Console.ReadLine();
            filenameOut = (String.IsNullOrEmpty(filenameOut)) ? "GPSoutput" :  filenameOut;
            
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

            filenameOut += ".csv";
            
            string[] lines = File.ReadAllLines(Path.Combine(path, filenameIn));
            int linesCount = lines.GetLength(0);


            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, filenameOut)))
            {
                outputFile.WriteLine("NMEA, Message ID, UTC of position fix, Latitude, Direction of latitude, Longitude, Direction of longitude, GPS Quality, Number of SVs in use (range from 00 through to 24+), HDOP, Orthometric height (MSL reference), unit of measure, Geoid separation, unit of measure, Age of differential GPS data record, checksum data");

                foreach (string line in lines)
                {
                    if (line.StartsWith("NMEA,$GPGGA")|| line.StartsWith("NMEA,$GLGGA")|| line.StartsWith("NMEA,$GNGGA"))
                    {
                        string edited;
                        int commaIndex = 0;
                        float coord = 0;
                        string coordString;
                        

                        edited=line.Insert(14, ":");
                        edited=edited.Insert(17, ":");
                        edited=edited.Replace(',',';');

                        commaIndex = getNthIndex(edited,';',3);
                        coordString=edited.Substring(commaIndex+1,11);
                        if(coordString[1]!=';')
                        {
                            //coordString=coordString.Replace('.',',');
                            coord=float.Parse(coordString.Substring(0,2));
                            coord+=float.Parse(coordString.Substring(2))/60;
                            edited = edited.Replace(coordString, coord.ToString());


                            commaIndex = getNthIndex(edited,';',5);
                            coordString=edited.Substring(commaIndex+1,12);
                            //coordString=coordString.Replace('.',',');
                            Console.WriteLine(coordString.Substring(0,3));
                            coord=float.Parse(coordString.Substring(0,3));
                            Console.WriteLine(float.Parse(coordString.Substring(3)));
                            coord+=(float.Parse(coordString.Substring(3))/60);
                            edited = edited.Replace(coordString, coord.ToString());

                            outputFile.WriteLine(edited);
                        }
                    }
                }
                Console.WriteLine("Hotovo! Ocisteny soubor byl vytvoren.");

            }

            Console.WriteLine("\nPro ukonceni programu stiskni enter...");
            Console.ReadLine();
        }
    }
}
