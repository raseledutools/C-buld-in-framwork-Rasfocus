using System;
using System.Windows.Forms;

namespace FocusBlocker.Core
{
    public class FocusTimer
    {
        private readonly Timer _t = new() { Interval = 1000 };
        private int _secsLeft;

        public event Action<int>? Tick;
        public event Action? Finished;
        public bool IsRunning => _t.Enabled;

        public FocusTimer() => _t.Tick += (_, _) =>
        {
            Tick?.Invoke(--_secsLeft);
            if (_secsLeft <= 0) { _t.Stop(); Finished?.Invoke(); }
        };

        public void Start(int minutes) { _secsLeft = minutes * 60; _t.Start(); }
        public void Stop()  => _t.Stop();
        public void Reset(int minutes) { _t.Stop(); _secsLeft = minutes * 60; }
    }
}
