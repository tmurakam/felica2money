/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2011 Takuya Murakami
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace FeliCa2Money
{
    class SfcPeep
    {
        private List<string> lines;

        // SFCPeep を実行し、実行結果の文字列リストを返す
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
            p.StartInfo.CreateNoWindow = true;
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
