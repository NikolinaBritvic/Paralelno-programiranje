using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LIB;

namespace drugidomaci
{
    class Program
    {
        public static decimal[,] PomnoziMatriceSekvencijalno(decimal[,] A, decimal[,] B)
        {
            decimal[,] C = new decimal[A.GetLength(0), B.GetLength(1)];

            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < B.GetLength(1); j++)
                {
                    decimal s = 0;
                    for (int k = 0; k < A.GetLength(1); k++)
                    {

                        s += A[i, k] * B[k, j];
                    }
                    C[i, j] = s;
                }
            }
            return C;
        }

        public static decimal[,] PomnoziMatrice(decimal[,] A, decimal[,] B)
        {
            decimal[,] C = new decimal[A.GetLength(0), B.GetLength(1)];

            List<Task> task1 = new List<Task>();
            for (int i = 0; i < A.GetLength(0); i++)
            {
                Task t1 = Task.Factory.StartNew((object obj) =>
                {
                    int I = (int)obj;

                    List<Task> task2 = new List<Task>();
                    for (int j = 0; j < B.GetLength(1); j++)
                    {
                        Task t2 = Task.Factory.StartNew((object ob) =>
                        {
                            int J = (int)ob;

                            List<Task> task3 = new List<Task>();
                            decimal[] a = new decimal[A.GetLength(1)];
                            decimal s = 0;
                            for (int k = 0; k < A.GetLength(1); k++)
                            {
                                Task t3 = Task.Factory.StartNew((object o) =>
                                {
                                    int K = (int)o;
                                    a[K] = A[I, K] * B[K, J];

                                }, k);
                                task3.Add(t3);
                            }
                            Task.WaitAll(task3.ToArray());
                            SumPrefixIdeal<decimal> spi = new SumPrefixIdeal<decimal>(a);
                            s = spi.Reduce((x, y) => x + y);
                            C[I, J] = s;
                        }, j);
                        task2.Add(t2);
                    }

                    Task.WaitAll(task2.ToArray());
                }, i);
                task1.Add(t1);
            }
            Task.WaitAll(task1.ToArray());

            return C;
        }
        static void Main()
        {
            //PRVI ZADATAK
            Console.WriteLine("PRVI ZADATAK\n");

            string datoteka = File.ReadAllText("domaci2-stringovi.csv");
            string[] slova = datoteka.Split(",");

            int[] T = new int[slova.Length];
            int[] TT = new int[slova.Length];

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < slova.Length; i++)
            {
                Task t = Task.Factory.StartNew((object obj) =>
                {
                    int I = (int)obj;

                    if (slova[I] == slova[I].ToUpper())
                    {
                        T[I] = 1;
                        TT[I] = 0;
                    }
                    else
                    {
                        T[I] = 0;
                        TT[I] = 1;
                    }
                }, i);
                tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray<Task>());

            //Console.WriteLine("Slova: {0}", Helper<string>.WriteArray(slova));
            //Console.WriteLine("T: {0}", Helper<int>.WriteArray(T));

            int[] S = Helper<int>.Copy(T);
            //Console.WriteLine("S: {0}", Helper<int>.WriteArray(S));
            int[] SS = Helper<int>.Copy(TT);
            //Console.WriteLine("SS: {0}", Helper<int>.WriteArray(SS));

            SumPrefixIdeal<int> spi = new SumPrefixIdeal<int>(T);
            SumPrefixIdeal<int> spi2 = new SumPrefixIdeal<int>(TT);
            int nvel = spi.Prescan((x, y) => x + y, 0);
            int nmal = spi2.Prescan((x, y) => x + y, 0);

            //Console.WriteLine("U polju P ima {0} velikih slova.", nvel);
            //Console.WriteLine("T = {0}", Helper<int>.WriteArray(T));
            //Console.WriteLine("U polju P ima {0} malih slova.", nmal);
            //Console.WriteLine("TT = {0}", Helper<int>.WriteArray(TT));

            string[] R = new string[nvel];
            string[] RR = new string[nmal];

            for (int i = 0; i < slova.Length; i++)
            {
                Task t = Task.Factory.StartNew((obj) =>
                {
                    int I = (int)obj;
                    if (S[I] == 1)
                    {
                        int index = T[I];
                        R[index] = slova[I];
                    }
                    if (SS[I] == 1)
                    {
                        int index = TT[I];
                        RR[index] = slova[I];
                    }
                }, i);
                tasks[i] = t;
            }
            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("VELIKA SLOVA = {0}", Helper<string>.WriteArray(R));
            Console.WriteLine("MALA SLOVA = {0}", Helper<string>.WriteArray(RR));
            Console.WriteLine("------------------------------------------------");

            // DRUGI ZADATAK

            Console.WriteLine("DRUGI ZADATAK\n");

            List<string> aa = new List<string>();
            List<string> bb = new List<string>();
            bool bul = false;

            StreamReader sr = new StreamReader("domaci2-matrice.csv");
            using (sr)
            {
                string linija;
                while ((linija = sr.ReadLine()) != null)
                {
                    if (linija == "A")
                    {
                        continue;
                    }
                    else if (linija == "B")
                    {
                        bul = true;
                    }
                    else
                    {
                        if (bul)
                            bb.Add(linija);
                        else
                            aa.Add(linija);
                    }
                }
                sr.Close();
            }
            int brrA = aa.Count;
            int brsA = aa[0].Split(',').Length;
            int brrB = bb.Count;
            int brsB = bb[0].Split(',').Length;

            decimal[,] A = new decimal[brrA, brsA];
            decimal[,] B = new decimal[brrB, brsB];

            List<Task> taskoviA = new List<Task>();
            for (int i = 0; i < brrA; i++)
            {
                Task t = Task.Factory.StartNew((object obj) =>
                {
                    int I = (int)obj;
                    string[] retci = aa[I].Split(',');

                    List<Task> lista1 = new List<Task>();
                    for (int k = 0; k < brsA; k++)
                    {
                        Task t1 = Task.Factory.StartNew((object ob) =>
                        {
                            int J = (int)ob;
                            A[I, J] = decimal.Parse(retci[J].Replace(".", ","));
                        }, k);
                        lista1.Add(t1);
                    }
                    Task.WaitAll(lista1.ToArray());
                }, i);
                taskoviA.Add(t);
            }
            Task.WaitAll(taskoviA.ToArray());

            List<Task> taskoviB = new List<Task>();
            for (int i = 0; i < brrB; i++)
            {
                Task tb = Task.Factory.StartNew((object obj) =>
                {
                    int I = (int)obj;
                    string[] retci = bb[I].Split(',');

                    List<Task> lista1 = new List<Task>();
                    for (int k = 0; k < brsB; k++)
                    {
                        Task tb = Task.Factory.StartNew((object ob) =>
                        {
                            int J = (int)ob;
                            B[I, J] = decimal.Parse(retci[J].Replace(".", ","));
                        }, k);
                        lista1.Add(tb);
                    }
                    Task.WaitAll(lista1.ToArray());
                }, i);
                taskoviB.Add(tb);
            }
            Task.WaitAll(taskoviB.ToArray());


            decimal[,] BT = new decimal[brsB, brrB];

            List<Task> ts = new List<Task>();
            for (int i = 0; i < brrB; i++)
            {
                Task t = Task.Factory.StartNew((object obj) =>
                  {
                      int I = (int)obj;
                      for (int j = 0; j < brsB; j++)
                      {
                          BT[j, I] = B[I, j];
                      }
                  }, i);
                ts.Add(t);
            }
            Task.WaitAll(ts.ToArray());

            decimal[,] C = PomnoziMatrice(A, BT);

            Console.WriteLine("Ucitane su matrice A i B i napravljene su transponirana B i C matrica.");
            Console.WriteLine("------------------------------------------------");

            //TRECI ZADATAK
            Console.WriteLine("TRECI ZADATAK\n");

            decimal[] niz_m = new decimal[C.GetLength(1)];

            List<Task> taskovi1 = new List<Task>();
            for (int i = 0; i < C.GetLength(1); i++)
            {
                Task t = Task.Factory.StartNew((object obj) =>
                {
                    decimal[] niz = new decimal[C.GetLength(0)];

                    int I = (int)obj;
                    List<Task> taskovi2 = new List<Task>();
                    for (int j = 0; j < C.GetLength(0); j++)
                    {
                        Task t2 = Task.Factory.StartNew((object o) =>
                          {
                              int J = (int)o;
                              niz[J] = C[J, I];
                          }, j);
                        taskovi2.Add(t2);

                    }
                    Task.WaitAll(taskovi2.ToArray());
                    SumPrefixIdeal<decimal> spimax = new SumPrefixIdeal<decimal>(niz);
                    decimal max = spimax.Reduce((x, y) => x >= y ? x : y);

                    if (max >= 0)
                        niz_m[I] = max;
                    else
                        niz_m[I] = 0;
                }, i);
                taskovi1.Add(t);
            }
            Task.WaitAll(taskovi1.ToArray());

            Console.WriteLine("Niz maksimuma = {0}", Helper<decimal>.WriteArray(niz_m));

            Console.WriteLine();
            Console.WriteLine("------------------------------------------------");

            //CETVRTI ZADATAK

            Console.WriteLine("CETVRTI ZADATAK\n");

            //SEKVENCIJALNO 1.ZADATAK
            string datoteka1 = File.ReadAllText("domaci2-stringovi.csv");
            string[] slova1 = datoteka1.Split(",");

            List<string> malaslova = new List<string>();
            List<string> velikaslova = new List<string>();

            for (int i = 0; i < slova1.Length; i++)
            {
                if (slova1[i] == slova1[i].ToLower())
                {
                    malaslova.Add(slova1[i]);
                }
                else
                {
                    velikaslova.Add(slova1[i]);
                }
            }

            string[] nizmalih = malaslova.ToArray();
            string[] nizvelikih = velikaslova.ToArray();

            Console.Write("MALA SLOVA SEKVENCIJALNO: ");
            for (int i = 0; i < nizmalih.Length; i++)
            {
                Console.Write(nizmalih[i] + " ");
            }
            Console.WriteLine();
            Console.Write("VELIKA SLOVA SEKVENCIJALNO: ");
            for (int i = 0; i < nizvelikih.Length; i++)
            {
                Console.Write(nizvelikih[i] + " ");
            }
            Console.WriteLine();
            Console.WriteLine("------------------------------------------------");

            //SEKVENCIJALNO 2.ZADATAK

            decimal[,] A1 = new decimal[brrA, brsA];
            decimal[,] B1 = new decimal[brrB, brsB];

            for (int i1 = 0; i1 < brrA; i1++)
            {
                string[] retci = aa[i1].Split(',');
                for (int k1 = 0; k1 < brsA; k1++)
                {
                    A1[i1, k1] = decimal.Parse(retci[k1].Replace(".", ","));
                }
            }
            for (int i2 = 0; i2 < brrB; i2++)
            {
                string[] retci = bb[i2].Split(',');
                for (int k2 = 0; k2 < brsB; k2++)
                {
                    B1[i2, k2] = decimal.Parse(retci[k2].Replace(".", ","));
                }
            }
                decimal[,] BT1 = new decimal[brsB, brrB];
                for (int i = 0; i < brrB; i++)
                {

                    for (int j = 0; j < brsB; j++)
                    {
                        BT1[j, i] = B1[i, j];
                    }

                }

                decimal[,] C1 = PomnoziMatriceSekvencijalno(A1, BT1);

                Console.WriteLine("Ucitane su A i B sekvencijalno i napravljene su transponirana B i C matrica sekvencijalno.");
                Console.WriteLine("------------------------------------------------");

            //SEKVENCIJALNO 3.ZADATAK
            decimal[] niz_m1 = new decimal[C.GetLength(1)];
            decimal max = -14234;

            for (int i = 0; i < C.GetLength(1); i++)
            {
                max = -14234;
                for (int k = 0; k < C.GetLength(0); k++)
                {
                    if (C[k, i] >= max)
                        max = C[k, i];
                }
                if (max >= 0)
                    niz_m1[i] = max;
                else
                    niz_m1[i] = 0;
            }
            Console.WriteLine("Niz maksimuma sekv. = {0}", Helper<decimal>.WriteArray(niz_m1));

        }
    }
}



