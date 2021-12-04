using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDisposableObject : IDisposable, IDisposedNotifier
{
}

public interface IDisposedNotifier 
{
    public event Action DisposeEvent;
}
