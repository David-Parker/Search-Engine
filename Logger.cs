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
        private System.IO.StreamWriter _writer;
        private string _errors;
        private Stopwatch _sw;
        private int _delay;
        public bool _on;

        public Logger(string filepath, int delay, bool on)
        {
            _writer = new System.IO.StreamWriter(filepath, true);
            _sw = new Stopwatch();
            this._delay = delay;
            _errors = "";
            _on = on;
        }

        public void Initialize()
        {
            if(_on)
            {
                _sw.Start();
            }
        }

        public void Log(string data)
        {
            if(_on)
            {
                lock (_errors)
                {
                    _errors += data;
                }
                if (_sw.Elapsed.Seconds >= _delay)
                {
                    lock (_writer)
                    {
                        _writer.WriteLine(_errors);
                        _writer.Flush();
                        _sw.Restart();
                        _errors = "";
                    }
                }
            }
        }
    }
}
