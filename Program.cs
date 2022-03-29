using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DomaciRadPokusaj
{
    class Program
    {
        static void Main(string[] args)
        {
            // PRVI ZADATAK

            string[] nazivi_fileova = new string[100];

            for (int i = 0; i < 100; i++)
            {
                nazivi_fileova[i] = "file_" + (i + 1).ToString().PadLeft(3, '0') + ".csv";
            }

            
            int[,] velika_matrica = new int[3000, 100];
            int N = 100;

            Task[] ts = new Task[N];
            for (int i = 0; i < N; i++)
            {
                Task t = Task.Factory.StartNew(
                (k) =>
                    {
                        int I = (int)k;
                        try
                        {
                            using (StreamReader sr = new StreamReader(nazivi_fileova[I]))
                            {
                                int br = 0;
                                string linija;
                                while ((linija = sr.ReadLine()) != null)
                                {
                                    string[] brojevi_u_retku= linija.Split(",");
                                    for (int j = 0; j < brojevi_u_retku.Length; j++)
                                    {
                                        velika_matrica[30 * I + br, j] = int.Parse(brojevi_u_retku[j]);
                                    }
                                    br++;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Doslo je do pogreske.");
                            Console.WriteLine(e.Message);
                        }

                    }, i);
                ts[i] = t;
            }
            Task.WaitAll(ts);
            Console.WriteLine("Procitani su podaci i napravljena je velika matrica. Klikni enter za nastavak. ");
            Console.ReadKey();

            // DRUGI I TRECI ZADATAK


            Task[] ts1 = new Task[100];
            Task[] ts2 = new Task[1200];
            float[,] nova_matrica = new float[12, 100];

            for (int i = 0; i < 100; i++)
            {
                Task t1 = Task.Factory.StartNew(
                (k) =>
                {
                    int I = (int)k;

                    for (int j = 0; j < 3000; j = j + 250)
                    {
                        Task t2 = Task.Factory.StartNew(
                        (l) =>
                        {
                            int L = (int)l;
                            int suma = 0;
                            for (int m = L; m < L + 250; m++)
                            {
                                suma += velika_matrica[m, I];
                            }
                            nova_matrica[L / 250, I] = (suma / 250);

                        }, j);

                        ts2[(12 * I) + (j / 250)] = t2;
                    }

                }, i);
                ts1[i] = t1;
            }

            Task.WaitAll(ts1);
            Task.WaitAll(ts2);
            Console.WriteLine("Napravljeni su prosjeci i nova matrica. Klikni enter za nastavak. ");
            Console.ReadKey();

            //SEKVENCIJALNI NACIN
           
            for (int i = 0; i < 100; i++)
            {
              try
              {
                    using (StreamReader sr = new StreamReader(nazivi_fileova[i]))
                        {
                            int br = 0;
                            string linija;
                            while ((linija = sr.ReadLine()) != null)
                            {
                                string[] brojevi_u_retku = linija.Split(",");
                                for (int j = 0; j < brojevi_u_retku.Length; j++)
                                {
                                    velika_matrica[30 * i + br, j] = int.Parse(brojevi_u_retku[j]);
                                }
                                br++;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Doslo je do pogreske.");
                        Console.WriteLine(e.Message);
                    }
            }

            float[,] nova_matr = new float[12, 100];
            int suma1;
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 3000; j = j + 250)
                {
                    suma1 = 0;
                    for (int m = j; m < j + 250; m++)
                    {
                        suma1 += velika_matrica[m, i];
                    }
                    nova_matr[j / 250, i] = (suma1 / 250);
                }
            }

            //USPOREDBA NOVE MATRICE DOBIVENE PARALELNO I SEKVENCIJALNO
            for(int i=0;i<12;i++)
            {
                for(int j=0;j<100;j++)
                {
                   Console.Write(nova_matr[i, j] - nova_matrica[i, j]);
                }
                Console.WriteLine();
            }

            // Oduzimanjem sam dobila nul matricu sto znaci da su jednake
        }
    }
}


