using System;

namespace NeuroSpeech.Positron;

public class ErrorEventArgs : EventArgs
{
    public string? Error { get; set; }

    public string? Stack { get; set; }

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(this.Stack))
            return this.Error;
        return this.Error + "\r\n" + this.Stack;
    }
}
