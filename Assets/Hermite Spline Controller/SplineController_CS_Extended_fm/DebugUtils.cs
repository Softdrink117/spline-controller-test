#define DEBUG
 
using System;
using System.Diagnostics;
 
public class DebugUtils
{
    [Conditional("DEBUG")]
    static public void Assert(bool condition)
    {
        if (!condition) 
			throw new Exception();
    }
	
    [Conditional("DEBUG")]
    static public void AssertMsg(bool condition, string msg)
    {
        if (!condition) 
			throw new Exception(msg);
    }
}
