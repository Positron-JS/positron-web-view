using System;
using System.Collections.Generic;
using System.Text;

namespace NeuroSpeech.Positron;


public class JSTypeContext
{

    public StringBuilder writer;

    public Type type;
    public string className;

    public readonly Dictionary<string, string> Properties 
        = new Dictionary<string, string>();

    public readonly Dictionary<string, string> StaticProperties
        = new Dictionary<string, string>();

    public readonly Dictionary<string, string> Methods
        = new Dictionary<string, string>();

    public readonly Dictionary<string, string> StaticMethods
        = new Dictionary<string, string>();
}
