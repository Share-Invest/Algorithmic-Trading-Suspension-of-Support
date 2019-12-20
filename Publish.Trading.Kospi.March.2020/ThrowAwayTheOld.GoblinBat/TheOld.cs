﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShareInvest.Log.Message;

namespace ShareInvest.ThrowAway
{
    public class TheOld
    {
        public void ForsakeOld(string path)
        {
            try
            {
                uint date = uint.Parse(DateTime.Now.AddDays(-10).ToString("yyMMdd"));

                foreach (string log in Directory.GetDirectories(path))
                {
                    if (date < uint.Parse(log))
                        continue;

                    new Task(() =>
                    {
                        DirectoryInfo di = new DirectoryInfo(Path.Combine(path, log));
                        di.Delete(true);
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                new LogMessage().Record("Exception", ex.ToString());
                MessageBox.Show(string.Concat(ex.ToString(), "\n\nLook in the\n\n'Log'\n\nFolder."), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}