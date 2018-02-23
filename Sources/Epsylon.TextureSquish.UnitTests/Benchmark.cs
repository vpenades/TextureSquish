
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MathNet.Numerics.Statistics;

using static System.FormattableString;

namespace Epsylon.TextureSquish.UnitTests
{
    class BenchMark
    {
        private readonly List<BenchmarkReport> _Reports = new List<BenchmarkReport>();

        public int DefaultRepetitions { get; set; }

        

        public void Repeat(string name, Action action)
        {
            Repeat(name, DefaultRepetitions, action);
        }

        public void Repeat(string name, int count, Action action)
        {
            using (var ctx = new BenchmarkContext(name, r => _Reports.Add(r)))
            {
                ctx.Repeat(count, action);
            }
        }

        public BenchmarkReport GetReport(string name)
        {
            return _Reports.FirstOrDefault(item => item.Name == name);
        }

        public override string ToString()
        {
            return BenchmarkReport.ToString(_Reports);
        }

        public string ToCSV()
        {
            var sb = new StringBuilder();

            var header = string.Empty;
            foreach(var r in _Reports)
            {
                if (header.Length > 0) header += ", ";
                header += $"\"{r.Name}\"";
            }
            sb.AppendLine(header);

            int idx = 0;

            while(true)
            {
                var line = string.Empty;
                bool hasValues = false;

                foreach(var r in _Reports)
                {
                    if (line.Length > 0) line += ", ";

                    bool hasValue = r.PartialTimes.Count > idx;

                    if (hasValue)
                    {
                        hasValues = true;
                        line += Invariant($"{r.PartialTimes[idx].TotalSeconds}");
                    }
                    else line += " "; // required to add some content
                }

                if (hasValues == false) break;

                sb.AppendLine(line);
                ++idx;
            }

            return sb.ToString();
        }

    }


    class BenchmarkContext : IDisposable
    {
        internal BenchmarkContext(string name,Action<BenchmarkReport> reportAction)
        {
            _Name = name;
            _ReportAction = reportAction;

            _Stopwatch = new System.Diagnostics.Stopwatch();
            _Stopwatch.Restart();
        }

        public void Dispose()
        {
            _Stopwatch.Stop();

            var totalTime = _Stopwatch.Elapsed;

            _ReportAction?.Invoke(new BenchmarkReport(_Name, totalTime,_PartialTimes.ToArray() ) );
        }

        private readonly string _Name;
        private readonly System.Diagnostics.Stopwatch _Stopwatch;
        private readonly Action<BenchmarkReport> _ReportAction;

        private readonly List<TimeSpan> _PartialTimes = new List<TimeSpan>();

        public void Repeat(int count, Action action)
        {
            for(int i=0; i < count; ++i)
            {
                var tstart = _Stopwatch.Elapsed;

                action.Invoke();

                var tend = _Stopwatch.Elapsed;

                _PartialTimes.Add(tend - tstart);
            }
        }
    }    

    class BenchmarkReport
    {
        public BenchmarkReport(string name, TimeSpan total, TimeSpan[] partials)
        {
            _Name = name;
            _TotalTime = total;
            _PartialTimes = partials ?? (new TimeSpan[] { _TotalTime });
        }

        private readonly string _Name;

        private readonly TimeSpan _TotalTime;
        private readonly TimeSpan[] _PartialTimes;

        public string Name => _Name;

        public TimeSpan TotalTime => _TotalTime;

        public IReadOnlyList<TimeSpan> PartialTimes => _PartialTimes;

        public TimeSpan AverageTime => new TimeSpan((long)_PartialTimes.Select(item => item.Ticks).Average());

        public override string ToString()
        {
            return $"{_Name} took {_TotalTime} to do {_PartialTimes.Length} Average: {AverageTime}";
        }

        public static string ToString(IEnumerable<BenchmarkReport> reports)
        {
            var sb = new StringBuilder();

            foreach (var r in reports.OrderBy(item => item.AverageTime))
            {
                sb.AppendLine(r.ToString());
            }

            return sb.ToString();
        }
    }
}
