using System;
using MPI;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Environment = System.Environment;

namespace NikolinaBritvic4
{
    class Program
    {
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                Intracommunicator c = Communicator.world;
                int N = c.Size;

                List<(DateTime, decimal, decimal)> s = new List<(DateTime, decimal, decimal)>();
                List<(DateTime, decimal, decimal)>[] lst = new List<(DateTime, decimal, decimal)>[N];

                string[] imena = new string[N];
                if (c.Rank == 0)
                {
                    StreamReader sr = new StreamReader("stock_data_100.csv");
                    using (sr)
                    {
                        //prvi red
                        string linija = sr.ReadLine();
                        string[] prvired = linija.Split(',');

                        //drugi i treci red
                        linija = sr.ReadLine();
                        linija = sr.ReadLine();

                        for (int i = 0; i < N; i++)
                        {
                            lst[i] = new List<(DateTime, decimal, decimal)>();
                        }

                        while (!sr.EndOfStream)
                        {
                            //od cetvrtog nadalje
                            linija = sr.ReadLine();
                            string[] ostaliretci = linija.Split(',');

                            for (int i = 0; i < N; i++)
                                if (ostaliretci[i * 6 + 1] != "" && ostaliretci[i * 6 + 5] != "")
                                {
                                    imena[i] = prvired[i * 6 + 1];
                                    lst[i].Add((DateTime.Parse(ostaliretci[0].Substring(0, 19)), Convert.ToDecimal(ostaliretci[i * 6 + 1], CultureInfo.InvariantCulture), Convert.ToDecimal(ostaliretci[i * 6 + 5], CultureInfo.InvariantCulture)));
                                }
                        }
                    }
                }

                string naziv = " ";
                string unos;
                string[] niz_unos = new string[N];

                s = c.Scatter(lst, 0);
                naziv = c.Scatter(imena, 0);

                Stock dionica = new Stock(naziv, s);
                do
                {
                    if (c.Rank == 0)
                    {
                        Console.WriteLine("Unesi komandu: ");
                        unos = Console.ReadLine();
                        niz_unos = Enumerable.Repeat<string>(unos, c.Size).ToArray();
                    }

                    unos = c.Scatter(niz_unos, 0);
                    string[] podjela = unos.Split(' ');

                    if (unos.ToUpper() == "QUIT")
                    {
                        break;
                    }

                    if (podjela[0] == "perc")
                    {
                        //ZADATAK 2 A
                        if (podjela.Length == 3) 
                        {
                            var datum1 = DateTime.Parse(podjela[1], CultureInfo.InvariantCulture);
                            var datum2 = DateTime.Parse(podjela[2], CultureInfo.InvariantCulture);

                            if (datum2 < datum1)
                                continue;

                            var komanda = new Komanda(podjela[0], new List<DateTime>() { datum1, datum2 });

                            decimal stara = 1;
                            decimal nova = 1;
                            var rezultat1 = new Result<List<string>>();
                            rezultat1.Name = dionica.Ime;
                            rezultat1.Value = new List<string>();

                            for (int i = 0; i < dionica.Povijest.Count; i++)
                            {
                                if (komanda.Datum[0].Date < dionica.Povijest[i].Item1.Date && dionica.Povijest[i].Item1.Date <= komanda.Datum[1].Date && dionica.Povijest[i].Item1.Hour == 15 && dionica.Povijest[i].Item1.Minute == 45)
                                {
                                    nova = dionica.Povijest[i].Item3;
                                    stara = dionica.Povijest[i - 51].Item2;

                                    rezultat1.Value.Add($" {Math.Round((nova - stara) / stara * 100, 4)}");
                                }
                            }

                            var podaci1 = c.Gather(rezultat1, 0);

                            if (c.Rank == 0)
                            {
                                Console.WriteLine("-------------------------------------------");
                                Console.WriteLine("REZULTATI ZADATKA 2.a): ");

                                foreach (var element in podaci1)
                                {
                                    Console.WriteLine("Ime dionice:{0}", element.Name);
                                    Console.WriteLine("Podaci: ");
                                    foreach (var element1 in element.Value)
                                    {
                                        Console.Write(element1 + " ");
                                    }
                                    Console.WriteLine();
                                }

                                Console.WriteLine("-------------------------------------------");
                                Console.WriteLine("REZULTATI ZADATKA 3): ");

                                int provjera = 0, duljina = 0;
                                foreach (var element in podaci1)
                                {
                                    duljina = element.Value.Count();
                                    if (duljina > 8)
                                        provjera = duljina - 8;

                                    Console.WriteLine("Ime dionice:{0}", element.Name);
                                    Console.WriteLine("Podaci: ");

                                    foreach (var element1 in element.Value)
                                    {
                                        if (provjera != 0)
                                            provjera -= 1;
                                        else
                                            Console.Write(element1 + " ");
                                    }
                                    Console.WriteLine();
                                }
                            }
                        }

                        // ZADATAK 2 B
                        else if (podjela.Length == 2)
                        {
                            var datum = DateTime.Parse(podjela[1], CultureInfo.InvariantCulture);
                            var komanda = new Komanda(podjela[0], new List<DateTime>() { datum });

                            decimal stara = 1;
                            decimal nova = 1;
                            var rezultat2 = new Result<List<string>>();
                            rezultat2.Name = dionica.Ime;
                            rezultat2.Value = new List<string>();
                            for (int i = 0; i < dionica.Povijest.Count; i++)
                            {
                                if (komanda.Datum[0].Date == dionica.Povijest[i].Item1.Date && dionica.Povijest[i].Item1.Hour == 9 && dionica.Povijest[i].Item1.Minute == 30)
                                {
                                    for (int j = 0; j < 26; j++)
                                    {
                                        nova = dionica.Povijest[i + j].Item3;
                                        stara = dionica.Povijest[i + j].Item2;

                                        rezultat2.Value.Add($" {dionica.Povijest[i+j].Item1} : {Math.Round((nova - stara) / stara * 100, 4)}");
                                    }
                                    break;
                                }
                            }

                            var podaci2 = c.Gather(rezultat2, 0);
                            if (c.Rank == 0)
                            {
                                Console.WriteLine("-------------------------------------------");
                                Console.WriteLine("REZULTATI ZADATKA 2.b): ");
                                foreach (var element in podaci2)
                                {
                                    Console.WriteLine("Ime dionice: {0}", element.Name);
                                    Console.WriteLine("Podaci: ");
                                    foreach (var element1 in element.Value)
                                    {
                                            Console.WriteLine(element1);
                                    }
                                }
                                int provjera = 0, duljina = 0;
                                Console.WriteLine("-------------------------------------------");
                                Console.WriteLine("REZULTATI ZADATKA 3): ");
                                foreach (var element in podaci2)
                                {
                                    duljina = element.Value.Count();
                                    if (duljina > 8)
                                        provjera = duljina - 8;

                                    Console.WriteLine("Ime dionice: {0}", element.Name);
                                    Console.WriteLine("Podaci: ");
                                    foreach (var element1 in element.Value)
                                    {
                                        if (provjera != 0)
                                            provjera -= 1;
                                        else
                                            Console.WriteLine(element1);
                                    }
                                }
                            }
                        }

                      
                    }

                    
                } while (unos.ToUpper() != "QUIT");
            }
        }
    }
}
