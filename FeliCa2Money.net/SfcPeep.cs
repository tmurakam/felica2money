using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace FeliCa2Money
{
    class SfcPeep
    {
        private List<string> lines;

        public List<string> Execute(string arg)
        {
            lines = new List<string>();

            string SfcPeepPath = FeliCa2Money.Properties.Settings.Default.SFCPeepPath;

            Process p = new Process();
            p.StartInfo.FileName = SfcPeepPath;
            p.StartInfo.Arguments = arg;
            p.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(SfcPeepPath);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.OutputDataReceived += new DataReceivedEventHandler(EventHandler_OutputDataReceived);
            p.Start();

            p.BeginOutputReadLine();
            p.WaitForExit();

            return lines;
        }

        private void EventHandler_OutputDataReceived(object sender, DataReceivedEventArgs ev)
        {
            if (ev.Data != null)
            {
                lines.Add(ev.Data);
            }
        }
    }
}
