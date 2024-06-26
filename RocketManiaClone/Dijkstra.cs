﻿using System.Collections.Generic;
using System.Linq;

namespace RocketManiaClone
{
    public static class Dijkstra
    {
        public static int GraphMatrix(this MainWindow window, int m, int n)
        {
            if (m == n) return 0;
            string s = m.ToString();
            if (m < 10) s = "0" + s;
            string t = n.ToString();
            if (n < 10) t = "0" + t;
            if (window.GraphMatrixDict.TryGetValue(string.Format("{0}-{1}", s, t), out int value))
                return value;
            else
                return 1024;
        }

        public static Dictionary<int, int> Algorithm(MainWindow window, int start)
        {
            int[] dist = new int[100];
            int[] path = new int[100];
            BoolArray remaining = new BoolArray(100);

            for (int i = 0; i < 100; ++i)
            {
                dist[i] = window.GraphMatrix(start, i);
                path[i] = dist[i] == 1024 ? -1 : start;
                remaining[i] = true;
            }
            remaining[start] = false;
            int min, next = -1;
            for (int i = 0; i < 98; ++i)
            {
                min = 1024;
                for (int j = 0; j < 100; ++j)
                    if (remaining[j] && dist[j] < min)
                    {
                        min = dist[j];
                        next = j;
                    }
                if (min == 1024) break;
                remaining[next] = false;

                for (int j = 0; j < 100; ++j)
                    if (remaining[j] && dist[j] > dist[next] + window.GraphMatrix(next, j))
                    {
                        dist[j] = dist[next] + window.GraphMatrix(next, j);
                        path[j] = next;
                    }
            }
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (int i = 0; i < 100; ++i)
                if (path[i] != -1 && path[i] != i)
                    dict[i] = path[i];
            return dict;
        }

        public static List<int> Rockets(MainWindow window)
        {
            BoolArray subset = new BoolArray(100);
            for (int i = 0; i < 10; ++i)
                foreach (int n in Algorithm(window, 10 * i).Keys)
                    subset[n] = true;
            List<int> set = new List<int>();
            for (int j = 0; j < 100; ++j)
                if (subset[j]) set.Add(j);
            return set;
        }

        public static List<int> Fires(MainWindow window)
        {
            BoolArray subset = new BoolArray(100);
            for (int i = 0; i < 10; ++i)
                foreach (int n in Algorithm(window, 10 * i + 9).Keys)
                    subset[n] = true;
            List<int> set = new List<int>();
            for (int j = 0; j < 100; ++j)
                if (subset[j]) set.Add(j);
            return set;
        }
    }
}
