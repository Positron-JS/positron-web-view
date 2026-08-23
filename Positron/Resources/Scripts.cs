using System;
using System.Collections.Generic;
using System.Text;

namespace NeuroSpeech.Positron.Resources
{
    internal class Scripts
    {

        public static string Read(string name)
        {
            using var s = typeof(Scripts).Assembly.GetManifestResourceStream("NeuroSpeech.Positron.Resources." + name);
            var reader = new StreamReader(s!);
            return reader.ReadToEnd();
        }

        public static string TSLib => Read("TSLib.js");

        public static string Positron => Read("Positron.js");

    }
}
