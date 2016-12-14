using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.Common;
using System.Threading;
using System.IO;
using System.Xml;

namespace ServerRace
{

    class Program
    {

        
        static List<Game> games = new List<Game>();
        static List<String> playersOnline = new List<String>();
        static Mutex mutex = new Mutex();
        static ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        static DbConnection cn;
        static DbCommand cmd;

        static String varNameFileMap = @"Track\track1.txt";
        static List<String> mapsFileName = new List<string>();

        static void  AddResultatsInDB(String map, String winner)
        {
            cmd.CommandText = "Insert INTO Resultats values(map,name)values '" + map + "','" + winner + "';";
        }
       //CLASS MAP
        #region 
       
        class Map
    {
            public String name = "";
        public int turn;
        public string[] lines;
        public int[,] mapArr;
            public int currentMap = 1;
            public int rows = 93;
           public int cols = 300;
        private int[,] A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, Zfence, ZBottomRight, ZUpperLeft, ZUpperRight, ZLowerLeft, Wstart, Xfinish;
        private const int unitDimension = 3;
        public Map()
        {
                turn = 1;
                A = new int[unitDimension, unitDimension] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };// дорога
                B = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };// обочина
                C = new int[,] { { 1, 1, 1 }, { 0, 0, 0 }, { 0, 0, 0 } };// -
                D = new int[,] { { 0, 0, 0 }, { 0, 0, 0 }, { 1, 1, 1 } };// -
                E = new int[,] { { 1, 0, 0 }, { 1, 0, 0 }, { 1, 0, 0 } };// -
                F = new int[,] { { 0, 0, 1 }, { 0, 0, 1 }, { 0, 0, 1 } };// -
                G = new int[,] { { 1, 1, 1 }, { 1, 0, 0 }, { 1, 0, 0 } };// -
                H = new int[,] { { 0, 0, 1 }, { 0, 0, 1 }, { 1, 1, 1 } };//-
                I = new int[,] { { 1, 0, 0 }, { 1, 0, 0 }, { 1, 1, 1 } };//-
                J = new int[,] { { 1, 1, 1 }, { 0, 0, 1 }, { 0, 0, 1 } };//-
                K = new int[,] { { 2, 2, 2 }, { 1, 1, 1 }, { 1, 1, 1 } };//-
                L = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 2, 2, 2 } };//-
                M = new int[,] { { 2, 1, 1 }, { 2, 1, 1 }, { 2, 1, 1 } };//-
                N = new int[,] { { 1, 1, 2 }, { 1, 1, 2 }, { 1, 1, 2 } };//-
                O = new int[,] { { 2, 2, 2 }, { 2, 1, 1 }, { 2, 1, 1 } };//-
                P = new int[,] { { 2, 1, 1 }, { 2, 1, 1 }, { 2, 2, 2 } };//-
                Q = new int[,] { { 1, 1, 2 }, { 1, 1, 2 }, { 2, 2, 2 } };//-
                R = new int[,] { { 2, 2, 2 }, { 1, 1, 2 }, { 1, 1, 2 } };//-
                S = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 0 } };//-
                T = new int[,] { { 0, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };//-
                U = new int[,] { { 1, 1, 0 }, { 1, 1, 1 }, { 1, 1, 1 } };//-
                V = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 0, 1, 1 } };//-
                Zfence = new int[,] { { 2, 2, 2 }, { 2, 2, 2 }, { 2, 2, 2 } };// fence - забор
                ZBottomRight = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 2 } };//2 bottom right fence
                ZUpperLeft = new int[,] { { 2, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };//2 the upper left fence
                ZUpperRight = new int[,] { { 1, 1, 2 }, { 1, 1, 1 }, { 1, 1, 1 } };//2 upper right fence
                ZLowerLeft = new int[,] { { 2, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };//2 lower left corner fence
                Wstart = new int[,] { { 4, 4, 4 }, { 4, 6, 4 }, { 4, 4, 4 } };// start
                Xfinish = new int[,] { { 5, 5, 5 }, { 5, 5, 5 }, { 5, 5, 5 } };// finish
                mapArr = new int[1, 1];
            }
        public bool ReadMap(String nameFileMap)
        {
            try
            {
                lines = File.ReadAllLines(nameFileMap);

                    DirectoryInfo di = new DirectoryInfo(@"Track");
                    FileInfo[] fiArr = di.GetFiles();
                    foreach (FileInfo f in fiArr)
                    {
                        if (f.FullName == nameFileMap)
                        {
                            Char[] extention = new Char[f.Extension.Length + 1];
                            extention[0] = '.';
                            for (int i = 1, j = 0; i < extention.Length; i++, j++)
                            {
                                extention[i] = f.Extension[j];

                            }
                            mapsFileName.Add(f.Name.TrimEnd(extention));
                            this.name = f.Name.TrimEnd(extention);
                            Console.WriteLine("ADD FILENAME " + f.Name.TrimEnd(extention));
                        }
                    }

                   
            }
            catch (Exception ex1)
            {
                Console.WriteLine(ex1.Message);
                return false;
            }


            //System.Console.WriteLine("Pole: \n"); // проверка
            //foreach (string line in lines)
            //{
            //    Console.WriteLine(line); // вывод в консоле
            //}
            //File.WriteAllLines(nameFileMap, lines); // запись в файл
            rows = (lines.Length - 1) * unitDimension;
            cols = lines[1].Length * unitDimension;
            //Console.WriteLine("lines.Length = " + lines.Length + "  rows = " + rows + "   cols = " + cols);
            turn = Convert.ToInt32(lines[0]);
            Console.WriteLine("turn = " + turn);
            mapArr = new int[rows, cols];
            for (int i = 1; i < lines.Length; i++)
            {
                char[] conversionLine = lines[i].ToCharArray();
                for (int j = 0; j < lines[1].Length; j++)
                {
                    switch (conversionLine[j])
                    {
                        case 'A':
                            //Console.Write('A');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = A[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = A[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = A[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = A[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = A[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = A[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = A[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = A[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = A[2, 2];
                            break;
                        case 'B':
                            //Console.Write('B');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = B[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = B[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = B[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = B[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = B[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = B[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = B[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = B[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = B[2, 2];
                            break;
                        case 'C':
                            //Console.Write('C');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = C[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = C[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = C[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = C[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = C[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = C[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = C[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = C[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = C[2, 2];
                            break;
                        case 'D':
                            //Console.Write('D');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = D[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = D[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = D[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = D[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = D[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = D[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = D[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = D[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = D[2, 2];
                            break;
                        case 'E':
                            //Console.Write('E');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = E[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = E[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = E[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = E[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = E[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = E[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = E[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = E[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = E[2, 2];
                            break;
                        case 'F':
                            //Console.Write('F');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = F[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = F[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = F[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = F[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = F[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = F[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = F[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = F[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = F[2, 2];
                            break;
                        case 'G':
                            //Console.Write('G');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = G[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = G[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = G[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = G[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = G[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = G[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = G[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = G[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = G[2, 2];
                            break;
                        case 'H':
                            //Console.Write('H');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = H[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = H[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = H[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = H[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = H[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = H[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = H[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = H[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = H[2, 2];
                            break;
                        case 'I':
                            //Console.Write('I');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = I[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = I[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = I[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = I[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = I[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = I[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = I[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = I[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = I[2, 2];
                            break;
                        case 'J':
                            //Console.Write('J');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = J[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = J[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = J[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = J[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = J[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = J[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = J[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = J[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = J[2, 2];
                            break;
                        case 'K':
                            //Console.Write('K');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = K[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = K[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = K[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = K[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = K[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = K[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = K[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = K[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = K[2, 2];
                            break;
                        case 'L':
                            //Console.Write('L');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = L[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = L[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = L[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = L[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = L[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = L[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = L[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = L[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = L[2, 2];
                            break;
                        case 'M':
                            //Console.Write('M');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = M[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = M[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = M[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = M[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = M[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = M[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = M[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = M[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = M[2, 2];
                            break;
                        case 'N':
                            //Console.Write('N');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = N[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = N[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = N[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = N[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = N[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = N[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = N[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = N[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = N[2, 2];
                            break;
                        case 'O':
                            //Console.Write('O');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = O[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = O[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = O[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = O[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = O[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = O[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = O[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = O[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = O[2, 2];
                            break;
                        case 'P':
                            //Console.Write('P');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = P[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = P[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = P[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = P[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = P[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = P[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = P[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = P[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = P[2, 2];
                            break;
                        case 'Q':
                            //Console.Write('Q');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = Q[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = Q[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = Q[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = Q[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = Q[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = Q[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = Q[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = Q[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = Q[2, 2];
                            break;
                        case 'R':
                            //Console.Write('R');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = R[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = R[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = R[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = R[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = R[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = R[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = R[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = R[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = R[2, 2];
                            break;
                        case 'S':
                            //Console.Write('S');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = S[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = S[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = S[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = S[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = S[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = S[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = S[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = S[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = S[2, 2];
                            break;
                        case 'T':
                            //Console.Write('T');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = T[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = T[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = T[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = T[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = T[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = T[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = T[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = T[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = T[2, 2];
                            break;
                        case 'U':
                            //Console.Write('U');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = U[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = U[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = U[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = U[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = U[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = U[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = U[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = U[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = U[2, 2];
                            break;
                        case 'V':
                            //Console.Write('V');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = V[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = V[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = V[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = V[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = V[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = V[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = V[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = V[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = V[2, 2];
                            break;
                        case 'Z':
                            //Console.Write('Z');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = Zfence[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = Zfence[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = Zfence[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = Zfence[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = Zfence[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = Zfence[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = Zfence[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = Zfence[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = Zfence[2, 2];
                            break;
                        case '%':
                            //Console.Write('%');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = ZBottomRight[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = ZBottomRight[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = ZBottomRight[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = ZBottomRight[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = ZBottomRight[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = ZBottomRight[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = ZBottomRight[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = ZBottomRight[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = ZBottomRight[2, 2];
                            break;
                        case 'X':
                            //Console.Write('X');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = ZUpperLeft[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = ZUpperLeft[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = ZUpperLeft[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = ZUpperLeft[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = ZUpperLeft[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = ZUpperLeft[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = ZUpperLeft[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = ZUpperLeft[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = ZUpperLeft[2, 2];
                            break;
                        case 'W':
                            //Console.Write('W');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = ZUpperRight[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = ZUpperRight[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = ZUpperRight[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = ZUpperRight[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = ZUpperRight[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = ZUpperRight[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = ZUpperRight[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = ZUpperRight[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = ZUpperRight[2, 2];
                            break;
                        case 'Y':
                            //Console.Write('Y');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = ZLowerLeft[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = ZLowerLeft[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = ZLowerLeft[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = ZLowerLeft[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = ZLowerLeft[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = ZLowerLeft[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = ZLowerLeft[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = ZLowerLeft[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = ZLowerLeft[2, 2];
                            break;
                        case '*':
                            //Console.Write('*');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = Wstart[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = Wstart[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = Wstart[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = Wstart[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = Wstart[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = Wstart[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = Wstart[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = Wstart[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = Wstart[2, 2];
                            break;
                        case '#':
                            //Console.Write('#');
                            mapArr[(i - 1) * unitDimension, j * unitDimension] = Xfinish[0, 0];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 1] = Xfinish[0, 1];
                            mapArr[(i - 1) * unitDimension, j * unitDimension + 2] = Xfinish[0, 2];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension] = Xfinish[1, 0];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 1] = Xfinish[1, 1];
                            mapArr[(i - 1) * unitDimension + 1, j * unitDimension + 2] = Xfinish[1, 2];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension] = Xfinish[2, 0];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 1] = Xfinish[2, 1];
                            mapArr[(i - 1) * unitDimension + 2, j * unitDimension + 2] = Xfinish[2, 2];
                            break;
                        default:
                            break;
                    }
                }
                //Console.WriteLine();
            }
            //Console.WriteLine("Проверка 0-го эл 11 стр.: " + mapArr[0, 0].ToString());
            //Console.WriteLine("Проверка 0-го эл 12 стр.: " + mapArr[1, 0].ToString());
            //Console.WriteLine("Проверка 0-го эл 13 стр.: " + mapArr[2, 0].ToString());
            //Console.WriteLine("Проверка 0-го эл 21 стр.: " + mapArr[3, 0].ToString());
            //Console.WriteLine("Проверка 0-го эл 22 стр.: " + mapArr[4, 0].ToString());
            //Console.WriteLine("Проверка 0-го эл 23 стр.: " + mapArr[5, 0].ToString());
            //Console.WriteLine("Проверка 0-го эл 31 стр.: " + mapArr[6, 0].ToString());
            //Console.WriteLine("Проверка 0-го эл 32 стр.: " + mapArr[7, 0].ToString());
            //Console.WriteLine("Проверка 0-го эл 33 стр.: " + mapArr[8, 0].ToString());
            //Console.WriteLine("Проверка 1-го эл 31 стр.: " + mapArr[6, 1].ToString());
            //Console.WriteLine("Проверка 1-го эл 32 стр.: " + mapArr[7, 1].ToString());
            //Console.WriteLine("Проверка 1-го эл 33 стр.: " + mapArr[8, 1].ToString());
            return true;
        }
        public byte[] getMapBytes(int c)//формируем для клиента байтовый массив
            {
                byte[] byteArr = new Byte[this.mapArr.Length + 1];
                byteArr[0] = (Byte)c;
                for (int i = 0, j = 0, k = 1; i <= mapArr.Length;)
                {
                    byteArr[k++] = (Byte)mapArr[i, j++];
                    if (i == this.cols)
                        i++;

                }
                return byteArr;
            }
        }
        #endregion




        //Class GAME
        #region
        class Game
        {
            public int countPlayers = 0;//кол-во игроков
            public int state = 0;//статус игры 1-игра создана 2- заполнена игроками,готова к заезду
                                    //3-поехали 4-финиш 5-игра записана в БД 
            public Map map;//карта
            public List<Car> activeCars = new List<Car>();
            String winner = "";
            Thread threadGame = null;
            public Mutex mutex = new Mutex();
            bool endGame = false;

            public Game(int countPlayers,int countMap)
            {
                this.countPlayers = countPlayers;
                this.map = new Map();
                this.map.ReadMap(varNameFileMap);

                threadGame = new Thread(runGame);
                threadGame.IsBackground=true;
                threadGame.Start();
            }
            public int AddCar(Car car)//Добавление в игру машинку
            {
                //car.Turn = this.map.turn;
                activeCars.Add(car);
                if (activeCars.Count == countPlayers)
                {
                    state = 2;
                }
                return countPlayers-activeCars.Count;
            }

            private byte[] CreateSendForClientPosition()//формирование посылки для клиента с параметрами игры
            {
                byte[] bytes= new byte[activeCars.Count*3];
               // bytes[0] =(byte) this.state;
                int i = 0;
                foreach (Car car in activeCars)
                {
                    bytes[i++] = (byte)car.Turn;
                    bytes[i++] = (byte)car.positionX;
                    bytes[i++] = (byte)car.positionY;
                }

                return bytes;
            }

            private void carsOnStart()
            {


                for (int k = 0; k < activeCars.Count; k++)
                {
                    for (int i = 0, j = 0;i<map.rows&&j<map.cols ;)
                    {
                        if (map.mapArr[i, j] == 6)//6-стартовая позиция машинки
                        {
                            activeCars[k].previousPosition = 6;
                            activeCars[k].positionX = j;// map.mapArr[i,j];
                            activeCars[k].positionY = i;
                            activeCars[k].Turn = map.turn;
                            map.mapArr[i, j] = 7;
                            Console.WriteLine(activeCars[k] + " on Start " );
                            break;
                            //Thread.Sleep(300);
                        }
                        if (++j == map.cols)
                        { i++; j = 0; }

                    }
                }

            }

            private byte[] CreateBytesToClientNameCars()//Для рассылки имен игроков 
            {
                String names = "";

                for (int i=0;i<this.activeCars.Count;i++)
                {
                    if(i==activeCars.Count-1)
                        names = names + activeCars[i].name ;
                    else
                    names = names+activeCars[i].name + "|";
                    
                }
                Console.WriteLine("ALL CARS: " + names);
                Byte[] bytes = Encoding.UTF8.GetBytes(names);
               
                return bytes;
            }
            private void runGame()//Метод потока ИГРЫ
            {


                while (!endGame)
                {
                    this.mutex.WaitOne();
                    switch (this.state) {
                        case 2:
                    
                        carsOnStart();
                        byte[] bytes = CreateBytesToClientNameCars();
                        foreach (Car c in activeCars)
                        {
                            c.sendToClient(bytes);

                        }
                        state = 3;

                            break;
                        case 3:
                    
                            // Console.WriteLine("Летят клиентам позиции");
                            byte[] bytePosition = CreateSendForClientPosition();
                            // Console.WriteLine("Длина массива " + bytePosition.Length);
                           // Console.WriteLine("Кол-во игроков - " + activeCars.Count);

                            for (int i = 0; i < activeCars.Count; i++)
                            {
                                if (activeCars[i].Turn < 10)
                                {
                                    moveCar(activeCars[i]);
                                    Console.WriteLine(activeCars[i].name + " : turn=" + activeCars[i].Turn +
                                        "\tSpeed=" + activeCars[i].Speed +
                                        "\tX=" + activeCars[i].positionX + "\tY" + activeCars[i].positionY);
                                }
                            }
                            foreach (Car c in activeCars)
                                c.sendToClient(bytePosition);
                        break;
                        case 4:
                            Program.AddResultatsInDB(this.map.name,this.winner);
                            this.state = 5;

                            break;
                        case 5:
                            lockSlim.EnterWriteLock();
                            games.Remove(this);
                            lockSlim.ExitWriteLock();
                            this.endGame = true;

                            break;




                }
                    this.mutex.ReleaseMutex();
                    Thread.Sleep(100);
                }

            }

            private void moveCar(Car car)
            {
                for (int i = 0; i < car.Speed; i++)
                {
                    map.mapArr[car.positionY, car.positionX] = car.previousPosition;

                    switch (car.Turn)
                    {
                        case 1:
                            
                            car.positionY--;
                            break;
                        case 2:
                            car.positionX++;
                            car.positionY--;
                            break;
                        case 3:
                            car.positionX++;
                            break;
                        case 4:
                            car.positionX++;
                            car.positionY++;
                            break;
                        case 5:
                            car.positionY++;
                            break;
                        case 6:
                            car.positionX--;
                            car.positionY++;
                            break;
                        case 7:
                            car.positionX--;
                            break;
                        case 8:
                            car.positionX--;
                            car.positionY--;
                            break;
                        //case 0:break;
                    }
                    if (car.positionX < map.cols && car.positionY < map.rows&&
                        car.positionX >=0 && car.positionY >=0)
                    {
                        //Console.WriteLine("предыдущий "+car.previousPosition.ToString());
                        car.previousPosition = map.mapArr[car.positionY, car.positionX];
                        if (map.mapArr[car.positionY, car.positionX] == 7 ||
                            map.mapArr[car.positionY, car.positionX] == 2)
                        {
                            car.Turn = 11;
                            car.Speed = 0;
                            break;
                        }
                        if (map.mapArr[car.positionY, car.positionX] == 1 && car.Speed != 0)
                        {
                           
                            car.Speed = 1;
                            //Console.WriteLine("Speed down: "+car.Speed);

                        }

                        if (map.mapArr[car.positionY, car.positionX] == 5)
                        {
                            car.Turn = 10;
                            this.state = 4;
                            break;
                        }

                        if (map.mapArr[car.positionY, car.positionX] != 7)
                        {
                            //Console.WriteLine("Текущий " + map.mapArr[car.positionY, car.positionX].ToString());
                            map.mapArr[car.positionY, car.positionX] = 7;
                        }                       
                        
                        
                    }
                    else
                    {
                        car.Turn = 11;
                        break;
                        //car.positionX = map.cols - 1;
                        //car.positionY = map.rows - 1;
                    }

                }
            } 
        }
        #endregion

        //class CAR
        #region
        class Car
        {
            private int speed = 0;
            public int previousPosition = 0;
            public int positionX = 0;
            public int positionY = 0;
            private int turn = 1;

            public int Speed
            {
                get { return this.speed; }
                set
                {
                    if (value >= 4) value = 3;
                    if (value < 0) value = 0;
                    this.speed = value;
                }
            }

            public String name;
           
            public int Turn//коэффициент поворота
            {
                get
                        {
                            return this.turn;
                        }
                set
                        {
                                if (value == 9) value = 1;
                                if (value == 0) value = 8;
                                this.turn = value;
                        }
            }
            private TcpClient socket;
            NetworkStream NS;
            public bool isAlive = true;
        

            public Car(TcpClient socket)
            {
               
                   // turn = 1;
                    this.socket = socket;
                    NS = socket.GetStream();//Вопрос  -Делать ли это здесь
                    
            }
            

            public void sendToClient(byte[] arr)//отправляет клиенту INFO
            {
                //Console.WriteLine(Encoding.UTF8.GetString(arr));
                try
                {
                    Console.WriteLine("летит "+name+" ");
                    //NetworkStream NS = this.socket.GetStream();
                    NS.Write(arr, 0, arr.Length);
                    //NS = null;
                    
                    //NS.SetLength(0) ;
                }
                catch (Exception ex)
                {
                    this.turn =11;
                }
           }

        }
        #endregion

        //class ThrClient
        #region
        class ThrClient
        {
            TcpClient clientMaster;//сокет Master
            TcpClient clientSlave;//сокет Slave
            String name;//Login
            Car car;//Машинка для игры
            Game game;//Игра для этого клиента
           

            public ThrClient(TcpClient clientMaster, TcpClient clientSlave)
            {
                this.clientMaster = clientMaster;
                this.clientSlave = clientSlave;
            }

            public void runClient()//Метод потока клиента
            {

                String strRemoteEndPoint = clientMaster.Client.RemoteEndPoint.ToString();
                Console.WriteLine("Подключен " + strRemoteEndPoint);
                MemoryStream MS = new MemoryStream();
                while (true)
                {
                    try
                    {
                        byte[] buf = new Byte[128];
                        NetworkStream NS = clientMaster.GetStream();//создаем поток из сокета
                        while (true)
                        {
                            int cnt = NS.Read(buf, 0, buf.Length);
                            if (cnt == 0) throw new Exception("Получено 0 байт");
                            MS.Write(buf, 0, cnt);
                            if (!NS.DataAvailable) break;
                        }
                        byte[] a = MS.ToArray();

                        String msg = Encoding.UTF8.GetString(a, 0, a.Length);
                        Console.WriteLine("Получено от клиента: {0} //END", msg);

                        mutex.WaitOne();
                        msg = this.handleRequest(msg);
                        mutex.ReleaseMutex();

                        Console.WriteLine("Шлем клиенту: {0} //END", msg);
                        //----Ответ клиенту----------------------------

                        a = Encoding.UTF8.GetBytes(msg);
                        NS.Write(a, 0, a.Length);
                        //Очистка MemoryStream-----
                        MS.SetLength(0);
                        Console.WriteLine("Клиенту месеdж отослан");
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                           Console.WriteLine("Разрыв соединения {0}: " ,strRemoteEndPoint);
                            if (playersOnline.Contains(this.name))
                                playersOnline.Remove(this.name);
                          if (this.car != null)
                            this.car.isAlive = false;
                            this.clientMaster.Close();
                            this.clientSlave.Close();
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            break;
                        }
                     }
                    Thread.Sleep(10);
                }
            }//конец метода связи с клиентом по сокету

            private string handleRequest(String msg)//Обработка запроса от клиента
            {
                try
                {
                    String[] z = msg.Split(new Char[] { '|' });
                    switch (z[0])
                    {
                        case "LOGIN":
                            Console.WriteLine("LOGIN " + z[1] + "|" + z[2]);
                            lockSlim.EnterWriteLock();
                            if (ContainsInDB(z[1], z[2]) && !ContainsInOnline(z[1]))//Если есть в базе и нет в онлайне
                            {
                                //Console.WriteLine("LOGIN in DB" + z[1] + "|" + z[2]);
                                this.name = z[1];
                                playersOnline.Add(this.name);
                                lockSlim.ExitWriteLock();
                                return "LOGINOK|";
                            }
                            else
                            {
                                lockSlim.ExitWriteLock();
                                return "LOGINERROR|";
                            }
                            break;
                        case "REG":
                            lockSlim.EnterWriteLock();
                            if (!ContainsInDB(z[1], z[2]))
                            {
                                if (AddInDBNewClient(z[1], z[2], z[3], z[4], z[5]) == 1)
                                {
                                    lockSlim.ExitWriteLock();
                                    return "REGOK|";
                                }
                                else
                                {
                                    lockSlim.ExitWriteLock();
                                    return "REGERROR|DataBase Error";
                                }
                            }
                            else
                            {
                                lockSlim.ExitWriteLock();
                                return "REGERROR|Такой пользователь уже зарегистрирован";
                            }
                            break;
                        case "START":
                            
                                this.car = new Car(this.clientSlave);
                                  this.car.name = this.name; 
                            int quantity = 0;//осталось ждать игроков
                            int countGame = Convert.ToInt16(z[1]);
                            lockSlim.EnterWriteLock();
                            if (countGame == 0)//Создаем новую игру
                            {
                                this.game = new Game(Convert.ToInt16(z[2]), Convert.ToInt16(z[3]));
                                game.AddCar(this.car);
                             
                                games.Add(game);
                                lockSlim.ExitWriteLock();
                                return "STARTOK|1";
                            }
                            else//вписываем в определенную игру
                            {
                                try
                                {
                                    this.game = games[countGame - 1];
                                    quantity = this.game.AddCar(this.car);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    return "STARTERROR|Нет такой игры";
                                }
                                lockSlim.ExitWriteLock();
                                return "STARTOK|" + quantity;
                            }
                           
                            break;
                        case "CHOICE":
                            int k = 0;
                            lockSlim.EnterReadLock();
                            string send = "CHOICE|";
                            for (int i = 0; i < games.Count; i++)
                            {
                                send = send + games[i].map.currentMap.ToString() + "^" +
                                    games[i].activeCars.Count +"^"+games[i].countPlayers+ "&";
                            }
                            lockSlim.ExitReadLock();
                            return send;
                            break;
                        case "TURN":
                            Console.WriteLine(this.name + ": " + z[1]);
                            switch (z[1])
                            {
                                case "LEFT":
                                    this.game.mutex.WaitOne();
                                     this.car.Turn--;
                                   // int turn = this.car.getTurn()-1;
                                   //this.car.setTurn()
                                    this.game.mutex.ReleaseMutex();
                                    break;
                                case "RIGHT":
                                    this.game.mutex.WaitOne();
                                    this.car.Turn++;
                                    this.game.mutex.ReleaseMutex();
                                    break;
                            }

                            break;
                        case "MOVE":
                            Console.WriteLine(this.name+": "+z[1] );
                            switch (z[1])
                            {
                                case "FORWARD":
                                    Console.WriteLine("GOGOGo");
                                    this.game.mutex.WaitOne();
                                    this.car.Speed++;
                                    this.game.mutex.ReleaseMutex();
                                    break;
                                case "BACK":
                                    Console.WriteLine("STOPE");
                                    this.game.mutex.WaitOne();
                                    this.car.Speed--;
                                    this.game.mutex.ReleaseMutex();
                                    break;
                            }

                            break;
                        case "STAT":
                            lockSlim.EnterWriteLock();

                            lockSlim.ExitWriteLock();
                            return "STAT|" + "resultats";
                            break;

                        case "EXIT":
                            lockSlim.EnterWriteLock();
                            this.car.Turn=0;
                            playersOnline.Remove(this.name);
                            lockSlim.ExitWriteLock();
                            return "EXIT|" + "resultats";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("HandleRequest :" + ex.Message);
                }
                return msg;
            }

            //Методы CONTAINS
            #region
            private bool ContainsInDB(String name, String pass)//Если содержится в базе данных
            {
                int i = 0;
                try { 
                cmd.CommandText = "SELECT * FROM Players Where name='" + name + "' AND pass='" + pass + "';";
                DbDataReader R = cmd.ExecuteReader();

                while (R.Read())
                {
                    i++;
                }
                R.Close();
                // cn.Close();
            }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                    if (i >= 1)
                        return true;
                    else
                        return false;
                 return true;
            }
            private bool ContainsInOnline(string name)//метод для поиска имени 
            {
                foreach (string n in playersOnline)
                    if (n == name)
                        return true;
                return false;
            }
            #endregion
            //Методы работы с DATA BASE
            #region
            private int AddInDBNewClient(String name, String pass, String email, String gender, String age)
            {
                cmd.CommandText = "Insert INTO Players(name,pass,email,gender,age) values ('" + name + "','" + pass +
                                            "','" + email + "','" + gender + "'," + Convert.ToInt16(age) + ");";
                int k = cmd.ExecuteNonQuery();
                if (k == 1)
                    Console.WriteLine("AddClient");
                else
                    Console.WriteLine("NotAddClient");
                return k;
                //return 1;
            }
            #endregion
        }
        #endregion


        //Поток СЕРВЕРА для подключения клиентов
        #region
        class ThreadServer
        {

            private TcpListener serverSlave;
            private TcpListener serverMaster;
            private bool isRun = true;

            public void run()
            {
                

                try
                {
                    getIp();//определяем Ip компа
                    connectToDB();//подключаем DataBase

                    string ip = System.Configuration.ConfigurationManager.AppSettings["Ip"];
                    this.serverSlave = new TcpListener(IPAddress.Parse(ip), 5000);
                    this.serverSlave.Start();//Начинаем прослушивание
                    this.serverMaster = new TcpListener(IPAddress.Parse(ip), 5001);
                    this.serverMaster.Start();//Начинаем прослушивание
                                              // string ip = System.Configuration.ConfigurationManager.AppSettings["Ip"];

                    //Подключение БазыДанных
                    // String strProvider = ConfigurationManager.AppSettings["provider"];
                    // String strConString = ConfigurationManager.AppSettings["conString"];

                   // DbProviderFactory Factory = DbProviderFactories.GetFactory(strProvider);
                    //cn = Factory.CreateConnection();

                    //cn.ConnectionString = strConString;
                    //cmd = cn.CreateCommand();
                    //cn.Open();
                    while (this.isRun)
                    {
                        Console.WriteLine("Ожидание запроса на установление соединения");
                        TcpClient clientMaster = this.serverSlave.AcceptTcpClient();

                      //  Console.WriteLine("Подключение : {0} ", clientMaster.Client.RemoteEndPoint.ToString());

                        TcpClient clientSlave = this.serverMaster.AcceptTcpClient();
                       // Console.WriteLine("Подключение : {0} ", clientSlave.Client.RemoteEndPoint.ToString());

                        ThrClient newClient = new ThrClient(clientMaster, clientSlave);

                        Thread T1 = new Thread(newClient.runClient);
                        T1.IsBackground = true;
                        T1.Start();
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Ошибка серверного сокета:{0}", ex.Message);
                    Console.WriteLine("FATAL ERROR!!! :{0}", ex.Message);
                }
            }

            public void stopServer()
            {
                this.serverMaster.Stop();
                this.serverSlave.Stop();
                this.isRun = false;

                //Закрываем DBConnection
                 cn.Close();
                 cn.Dispose();
            }

        }
#endregion
        //MAIN
        #region
        static void Main(string[] args)
        {
           
            if (File.Exists(varNameFileMap))
            {
                Console.WriteLine("Файл существует.");
                Map map = new Map();
                map.ReadMap(varNameFileMap);
            }
            // Console.WriteLine("Press any key to exit.");
            // System.Console.ReadKey();

            //Считываем доступные карты
            try
            {
                DirectoryInfo di = new DirectoryInfo(@"Track");
                FileInfo[] fiArr = di.GetFiles();
                foreach (FileInfo f in fiArr)
                {
                    
                    Char[] extention = new Char[f.Extension.Length + 1];
                    extention[0] = '.';
                    for (int i = 1,j=0; i < extention.Length; i++,j++)
                    {
                        extention[i] = f.Extension[j];
                       
                    }
                    mapsFileName.Add(f.Name.TrimEnd(extention));
                    Console.WriteLine("ADD FILENAME " + f.Name.TrimEnd(extention));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Нет доступных карт");
            }

            //----------------------------------------------------
            ThreadServer TS = new ThreadServer();
            Thread T = new Thread(TS.run);
            T.IsBackground = true;
            T.Start();
            while(true) //(!Console.KeyAvailable)
            {
                Thread.Sleep(500);
            }
            TS.stopServer();
            Console.WriteLine("Good Bye");
        }
        #endregion

      static  private void getIp()
        {
            string myHost = Dns.GetHostName();
            //Console.WriteLine(myHost);

            // получаем IP-адрес хоста
            string myIP = "";
            int cnt = 0;
            bool isTrue = true;
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            while (isTrue)
            {
                cnt = 0;
                foreach (IPAddress ipa in Dns.GetHostEntry(myHost).AddressList)
                {
                    Console.WriteLine(++cnt + ":  " + ipa.ToString());
                }
                key = Console.ReadKey();
                for (int i = 0; i < cnt; i++)
                {
                    if (key.KeyChar.ToString() == (i + 1).ToString())
                    {
                        myIP = Dns.GetHostEntry(myHost).AddressList[i].ToString();
                        isTrue = false;
                        break;
                    }
                }
            }
            Console.WriteLine();

            //записываем в xml app.config
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            foreach (XmlElement element in xmlDoc.DocumentElement)
            {
                if (element.Name.Equals("appSettings"))
                {
                    foreach (XmlNode node in element.ChildNodes)
                    {
                        if (node.Attributes[0].Value.Equals("Ip"))
                        {
                            node.Attributes[1].Value = myIP;
                        }
                    }
                }
            }
            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            ConfigurationManager.RefreshSection("appSettings");


        }
      static  private void connectToDB()
        {
            
                // Подключение БазыДанных
                String nameDB = ConfigurationManager.AppSettings["nameDB"];
                String strConString = ConfigurationManager.AppSettings["conString"];
                String PathForDB = strConString + Environment.CurrentDirectory + nameDB;

                String strProvider = ConfigurationManager.AppSettings["provider"];


                DbProviderFactory Factory = DbProviderFactories.GetFactory(strProvider);
                cn = Factory.CreateConnection();

                cn.ConnectionString = PathForDB;
                cmd = cn.CreateCommand();
                cn.Open();
            
        }
    }
}
