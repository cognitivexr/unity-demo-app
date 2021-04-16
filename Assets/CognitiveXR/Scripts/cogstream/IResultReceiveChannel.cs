using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResultReceiveChannel
{

    EngineResult<T> Receive();
}
