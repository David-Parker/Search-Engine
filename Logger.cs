using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SearchBackend
{
    class Logger
    {
        private System.IO.StreamWriter writer;
        private string errors;
        private Stopwatch sw;
        private int delay;

        public Logger(string filepath, int delay)
        {
            writer = new System.IO.StreamWriter(filepath, true);
            sw = new Stopwatch();
            this.delay = delay;
            errors = "";
        }

        public void Initialize()
        {
            sw.Start();
        }

        public void Log(string data)
        {
            lock(errors)
            {
                errors += data;
            }
            if (sw.Elapsed.Seconds > delay)
            {
                lock (writer)
                {
                    writer.WriteLine(errors);
                    writer.Flush();
                    sw.Restart();
                    errors = "";
                }
            }
        }
    }
}
