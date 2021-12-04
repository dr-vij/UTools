using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    /// <summary>
    /// Class to create IComparer<T> from lambdas
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FuncComparer<T> : IComparer<T>
    {
        private Func<T, T, int> mComparer;

        public FuncComparer(Func<T, T, int> comparer) => mComparer = comparer;

        public static IComparer<T> Create(Func<T, T, int> comparer) => new FuncComparer<T>(comparer);

        public int Compare(T x, T y) => mComparer(x, y);
    }
}