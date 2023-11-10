using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTools
{
    /// <summary>
    /// Class to create IComparer<T> from lambdas
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FuncComparer<T> : IComparer<T>
    {
        private Func<T, T, int> m_Comparer;

        public FuncComparer(Func<T, T, int> comparer) => m_Comparer = comparer;

        public static IComparer<T> Create(Func<T, T, int> comparer) => new FuncComparer<T>(comparer);

        public int Compare(T x, T y) => m_Comparer(x, y);
    }
}