using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using FocusBlocker.Core;

namespace FocusBlocker.Forms
{
    // ═══════════════════════════════════════════════════════════════
    //  Colour palette
    // ═══════════════════════════════════════════════════════════════
    static class Pal
    {
        public static readonly Color Teal      = Color.FromArgb(0, 168, 150);
        public static readonly Color TealDark  = Color.FromArgb(0, 130, 115);
        public static readonly Color TealLight = Color.FromArgb(178, 237, 232);
        public static readonly Color White     = Color.White;
        public static readonly Color OffWhite  = Color.FromArgb(245, 250, 250);
        public static readonly Color Surface   = Color.FromArgb(255, 255, 255);
        public static readonly Color Border    = Color.FromArgb(210, 235, 233);
        public static readonly Color TextDark  = Color.FromArgb(30,  50,  50);
        public static readonly Color TextMid   = Color.FromArgb(90, 120, 118);
        public static readonly Color Danger    = Color.FromArgb(229, 57, 53);
        public static readonly Color Success   = Color.FromArgb(0, 168, 150);
    }

    // ═══════════════════════════════════════════════════════════════
    //  Rounded Panel
    // ═══════════════════════════════════════════════════════════════
    class Card : Panel
    {
        private int _radius;
        public Card(int radius = 16) { _radius = radius; DoubleBuffered = true; }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = RoundRect(ClientRectangle, _radius);
            using var brush = new SolidBrush(BackColor);
            e.Graphics.FillPath(brush, path);
            using var pen = new Pen(Pal.Border, 1.5f);
            e.Graphics.DrawPath(pen, path);
        }
        static GraphicsPath RoundRect(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, rad, rad, 180, 90);
            p.AddArc(r.Right - rad, r.Y, rad, rad, 270, 90);
            p.AddArc(r.Right - rad, r.Bottom - rad, rad, rad, 0, 90);
            p.AddArc(r.X, r.Bottom - rad, rad, rad, 90, 90);
            p.CloseFigure(); return p;
        }
        protected override CreateParams CreateParams
        { get { var cp = base.CreateParams; cp.ExStyle |= 0x20; return cp; } }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Teal pill button
    // ═══════════════════════════════════════════════════════════════
    class TealButton : Button
    {
        private bool _danger;
        private bool _outline;
        public TealButton(bool danger = false, bool outline = false)
        { _danger = danger; _outline = outline; SetStyle(StyleFlags(), true); DoubleBuffered = true; }
        private ControlStyles StyleFlags() =>
            ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer;
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(1, 1, Width - 2, Height - 2);
            Color bg = _outline ? Pal.White : (_danger ? Pal.Danger : Pal.Teal);
            Color fg = _outline ? Pal.Teal : Pal.White;
            using var path = Pill(r);
            using var brush = new SolidBrush(bg);
            e.Graphics.FillPath(brush, path);
            if (_outline) { using var pen = new Pen(Pal.Teal, 1.8f); e.Graphics.DrawPath(pen, path); }
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            using var fgBrush = new SolidBrush(fg);
            e.Graphics.DrawString(Text, font, fgBrush, ClientRectangle, sf);
        }
        static GraphicsPath Pill(Rectangle r)
        { var p = new GraphicsPath(); p.AddArc(r.X, r.Y, r.Height, r.Height, 90, 180); p.AddArc(r.Right - r.Height, r.Y, r.Height, r.Height, 270, 180); p.CloseFigure(); return p; }
        protected override void OnMouseEnter(EventArgs e) { Cursor = Cursors.Hand; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { Invalidate(); base.OnMouseLeave(e); }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Arc timer control
    // ═══════════════════════════════════════════════════════════════
    class ArcTimer : Control
    {
        public float Progress { get; set; } = 1f;   // 0..1
        public string TimeText { get; set; } = "25:00";
        public string SubText  { get; set; } = "Focus";
        public ArcTimer() { SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true); }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var g = e.Graphics;
            int margin = 18, thick = 14;
            var rect = new Rectangle(margin, margin, Width - margin * 2, Height - margin * 2);

            // Track
            using (var p = new Pen(Pal.TealLight, thick)) { p.StartCap = p.EndCap = LineCap.Round; g.DrawArc(p, rect, -90, 360); }
            // Arc
            float sweep = 360f * Math.Max(0, Math.Min(1, Progress));
            if (sweep > 0)
            using (var p = new Pen(Pal.Teal, thick)) { p.StartCap = p.EndCap = LineCap.Round; g.DrawArc(p, rect, -90, sweep); }

            // Time text
            using var bigFont = new Font("Segoe UI", 26f, FontStyle.Bold);
            using var smallFont = new Font("Segoe UI", 10f);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var centre = new Rectangle(0, -12, Width, Height);
            g.DrawString(TimeText, bigFont, new SolidBrush(Pal.TextDark), centre, sf);
            var sub = new Rectangle(0, 22, Width, Height);
            g.DrawString(SubText, smallFont, new SolidBrush(Pal.TextMid), sub, sf);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Schedule row panel
    // ═══════════════════════════════════════════════════════════════
    class ScheduleRow : Panel
    {
        public ScheduleEntry Entry { get; }
        public event Action<ScheduleEntry>? DeleteClicked;

        public ScheduleRow(ScheduleEntry entry)
        {
            Entry = entry;
            Height = 56; Dock = DockStyle.Top;
            BackColor = Pal.White; Padding = new Padding(12, 0, 8, 0);

            var days   = string.Join(", ", entry.Days.Select(d => d.ToString()[..2]));
            var times  = $"{entry.StartTime:hh\\:mm} – {entry.EndTime:hh\\:mm}";
            var sites  = $"{entry.Sites.Count} site(s)";

            var lblName = new Label { Text = entry.Name, Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Pal.TextDark, AutoSize = false, Location = new Point(12, 8), Size = new Size(220, 20) };
            var lblInfo = new Label { Text = $"{days}  ·  {times}  ·  {sites}",
                Font = new Font("Segoe UI", 8.5f), ForeColor = Pal.TextMid,
                AutoSize = false, Location = new Point(12, 30), Size = new Size(280, 18) };

            var chk = new CheckBox { Checked = entry.Enabled, Location = new Point(300, 16), Size = new Size(20, 20) };
            chk.CheckedChanged += (_, _) => { entry.Enabled = chk.Checked; ScheduleManager.Save(); };

            var btnDel = new TealButton(danger: true) { Text = "✕", Size = new Size(36, 28), Location = new Point(330, 14) };
            btnDel.Click += (_, _) => DeleteClicked?.Invoke(entry);

            Controls.AddRange(new Control[] { lblName, lblInfo, chk, btnDel });

            // separator
            var sep = new Panel { Height = 1, Dock = DockStyle.Bottom, BackColor = Pal.Border };
            Controls.Add(sep);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Main Form
    // ═══════════════════════════════════════════════════════════════
    public class MainForm : Form
    {
        // ── layout ──
        private Panel      _sidebar   = null!;
        private Panel      _content   = null!;
        private Panel      _pageTimer = null!;
        private Panel      _pageBlock = null!;
        private Panel      _pageSched = null!;

        // ── timer ──
        private ArcTimer   _arc       = null!;
        private TealButton _btnStart  = null!;
        private TealButton _btnStop   = null!;
        private NumericUpDown _nudMin = null!;
        private readonly FocusTimer _timer = new();
        private int _totalSecs;

        // ── block ──
        private FlowLayoutPanel _chipBox  = null!;
        private TextBox          _txtSite  = null!;
        private Label            _lblBlockStatus = null!;

        // ── schedule ──
        private Panel            _schedList    = null!;
        private Label            _lblNextSched = null!;

        // ── tray ──
        private NotifyIcon _tray = null!;

        // ── nav buttons ──
        private Button _navTimer = null!, _navBlock = null!, _navSched = null!;

        public MainForm()
        {
            InitForm();
            BuildSidebar();
            BuildContent();
            WireTimer();
            LoadBlocked();
            ScheduleManager.Init();
            RefreshScheduleList();
            ShowPage(_pageTimer);
        }

        // ─────────────────────────────────── form shell ──────────
        void InitForm()
        {
            Text = "FocusBlocker"; Size = new Size(740, 560);
            MinimumSize = Size; FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false; StartPosition = FormStartPosition.CenterScreen;
            BackColor = Pal.OffWhite; Font = new Font("Segoe UI", 9.5f);

            _tray = new NotifyIcon { Text = "FocusBlocker", Icon = SystemIcons.Application, Visible = true };
            var cm = new ContextMenuStrip();
            cm.Items.Add("Open", null, (_, _) => { Show(); WindowState = FormWindowState.Normal; });
            cm.Items.Add("Exit",  null, (_, _) => { _tray.Visible = false; Application.Exit(); });
            _tray.ContextMenuStrip = cm;
            _tray.DoubleClick += (_, _) => { Show(); WindowState = FormWindowState.Normal; };
            Resize += (_, _) => { if (WindowState == FormWindowState.Minimized) Hide(); };
        }

        // ─────────────────────────────────── sidebar ─────────────
        void BuildSidebar()
        {
            _sidebar = new Panel { Width = 180, Dock = DockStyle.Left, BackColor = Pal.Teal };

            // logo
            var logo = new Label { Text = "🎯 Focus", Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Pal.White, Location = new Point(20, 28), AutoSize = true };

            var sub = new Label { Text = "Blocker", Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(180, 255, 255), Location = new Point(20, 62), AutoSize = true };

            _navTimer = NavBtn("⏱  Timer",   100);
            _navBlock = NavBtn("🚫  Blocker", 150);
            _navSched = NavBtn("📅  Schedule", 200);

            _navTimer.Click += (_, _) => ShowPage(_pageTimer);
            _navBlock.Click += (_, _) => ShowPage(_pageBlock);
            _navSched.Click += (_, _) => ShowPage(_pageSched);

            var ver = new Label { Text = "v2.0", Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(150, 255, 255), Location = new Point(20, 500), AutoSize = true };

            _sidebar.Controls.AddRange(new Control[] { logo, sub, _navTimer, _navBlock, _navSched, ver });
            Controls.Add(_sidebar);
        }

        Button NavBtn(string text, int y)
        {
            var b = new Button { Text = text, Location = new Point(0, y), Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat, ForeColor = Pal.White,
                Font = new Font("Segoe UI", 10f), TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Pal.TealDark;
            b.FlatAppearance.MouseDownBackColor = Pal.TealDark;
            b.MouseEnter += (_, _) => b.BackColor = Pal.TealDark;
            b.MouseLeave += (_, _) => b.BackColor = Color.Transparent;
            return b;
        }

        // ─────────────────────────────────── content area ────────
        void BuildContent()
        {
            _content = new Panel { Dock = DockStyle.Fill, BackColor = Pal.OffWhite };
            Controls.Add(_content);

            BuildTimerPage();
            BuildBlockPage();
            BuildSchedulePage();
        }

        void ShowPage(Panel page)
        {
            _pageTimer.Visible = _pageBlock.Visible = _pageSched.Visible = false;
            page.Visible = true;
            // highlight nav
            foreach (Button b in new[] { _navTimer, _navBlock, _navSched })
                b.BackColor = Color.Transparent;
            var active = page == _pageTimer ? _navTimer : page == _pageBlock ? _navBlock : _navSched;
            active.BackColor = Pal.TealDark;
        }

        // ═══════════════════════════════════ TIMER PAGE ══════════
        void BuildTimerPage()
        {
            _pageTimer = new Panel { Dock = DockStyle.Fill, BackColor = Pal.OffWhite, Visible = false };

            var card = new Card { Location = new Point(40, 30), Size = new Size(460, 420), BackColor = Pal.White };

            var title = new Label { Text = "Focus Session", Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Pal.TextDark, Location = new Point(24, 20), AutoSize = true };

            _arc = new ArcTimer { Location = new Point(80, 55), Size = new Size(300, 300) };

            var lblMin = new Label { Text = "Duration (min)", Font = new Font("Segoe UI", 9f),
                ForeColor = Pal.TextMid, Location = new Point(60, 365), AutoSize = true };

            _nudMin = new NumericUpDown { Location = new Point(190, 360), Size = new Size(70, 28),
                Minimum = 1, Maximum = 180, Value = 25,
                BackColor = Pal.OffWhite, ForeColor = Pal.TextDark, Font = new Font("Segoe UI", 10f) };

            _btnStart = new TealButton { Text = "▶  Start", Size = new Size(110, 36), Location = new Point(270, 358) };
            _btnStop  = new TealButton(danger: true) { Text = "■  Stop", Size = new Size(110, 36),
                Location = new Point(270, 358), Visible = false };

            card.Controls.AddRange(new Control[] { title, _arc, lblMin, _nudMin, _btnStart, _btnStop });
            _pageTimer.Controls.Add(card);
            _content.Controls.Add(_pageTimer);
        }

        void WireTimer()
        {
            _btnStart.Click += (_, _) =>
            {
                _totalSecs = (int)_nudMin.Value * 60;
                _timer.Start((int)_nudMin.Value);
                _btnStart.Visible = false; _btnStop.Visible = true;
                _arc.SubText = "Focus"; _arc.Progress = 1f; _arc.Invalidate();
            };
            _btnStop.Click += (_, _) =>
            {
                _timer.Stop();
                _arc.TimeText = $"{(int)_nudMin.Value:D2}:00";
                _arc.Progress = 1f; _arc.SubText = "Ready"; _arc.Invalidate();
                _btnStop.Visible = false; _btnStart.Visible = true;
            };
            _timer.Tick += secs =>
            {
                _arc.TimeText = $"{secs/60:D2}:{secs%60:D2}";
                _arc.Progress = _totalSecs > 0 ? (float)secs / _totalSecs : 0f;
                _arc.Invalidate();
            };
            _timer.Finished += () =>
            {
                _arc.TimeText = "Done!"; _arc.SubText = "🎉"; _arc.Progress = 0f; _arc.Invalidate();
                _btnStop.Visible = false; _btnStart.Visible = true;
                _tray.ShowBalloonTip(4000, "FocusBlocker", "Session complete! Great work 🎉", ToolTipIcon.Info);
                MessageBox.Show("Focus session complete! 🎉", "FocusBlocker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        // ═══════════════════════════════════ BLOCK PAGE ══════════
        void BuildBlockPage()
        {
            _pageBlock = new Panel { Dock = DockStyle.Fill, BackColor = Pal.OffWhite, Visible = false };

            var card = new Card { Location = new Point(40, 30), Size = new Size(460, 420), BackColor = Pal.White };

            var title = new Label { Text = "Website Blocker", Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Pal.TextDark, Location = new Point(24, 20), AutoSize = true };

            var lblHint = new Label { Text = "Sites are blocked system-wide via hosts file.",
                Font = new Font("Segoe UI", 9f), ForeColor = Pal.TextMid,
                Location = new Point(24, 50), Size = new Size(400, 20) };

            // input row
            _txtSite = new TextBox { Location = new Point(24, 80), Size = new Size(300, 30),
                Font = new Font("Segoe UI", 10f), BackColor = Pal.OffWhite, ForeColor = Pal.TextDark,
                BorderStyle = BorderStyle.FixedSingle };
            _txtSite.SetWatermark("e.g. youtube.com");

            var btnAdd = new TealButton { Text = "+ Add", Size = new Size(100, 30), Location = new Point(332, 80) };
            btnAdd.Click += (_, _) => AddChip();
            _txtSite.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) AddChip(); };

            // chip container (scrollable)
            _chipBox = new FlowLayoutPanel { Location = new Point(24, 124), Size = new Size(412, 200),
                AutoScroll = true, BackColor = Pal.OffWhite, FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true, Padding = new Padding(4) };

            _lblBlockStatus = new Label { Location = new Point(24, 336), Size = new Size(412, 24),
                Font = new Font("Segoe UI", 9f), ForeColor = Pal.TextMid, Text = "Not blocking" };

            var btnBlock   = new TealButton { Text = "🔒 Block Now",  Size = new Size(130, 36), Location = new Point(24, 366) };
            var btnUnblock = new TealButton(outline: true) { Text = "🔓 Unblock All", Size = new Size(130, 36), Location = new Point(166, 366) };

            btnBlock.Click   += BtnBlock_Click;
            btnUnblock.Click += BtnUnblock_Click;

            card.Controls.AddRange(new Control[] { title, lblHint, _txtSite, btnAdd, _chipBox, _lblBlockStatus, btnBlock, btnUnblock });
            _pageBlock.Controls.Add(card);
            _content.Controls.Add(_pageBlock);
        }

        void AddChip()
        {
            var site = _txtSite.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(site)) return;
            if (_chipBox.Controls.OfType<Panel>().Any(p => p.Tag?.ToString() == site)) { _txtSite.Clear(); return; }

            var chip = new Panel { Size = new Size(0, 28), BackColor = Pal.TealLight,
                Tag = site, Margin = new Padding(4, 4, 0, 0) };
            var lbl = new Label { Text = site, Font = new Font("Segoe UI", 9f), ForeColor = Pal.TealDark,
                AutoSize = true, Location = new Point(8, 5) };
            chip.Width = lbl.PreferredWidth + 36;
            var btnX = new Label { Text = "✕", Font = new Font("Segoe UI", 8f), ForeColor = Pal.Teal,
                Location = new Point(chip.Width - 20, 6), Size = new Size(16, 16), Cursor = Cursors.Hand };
            btnX.Click += (_, _) => { _chipBox.Controls.Remove(chip); chip.Dispose(); };

            chip.Controls.Add(lbl); chip.Controls.Add(btnX);
            _chipBox.Controls.Add(chip);
            _txtSite.Clear();
        }

        void LoadBlocked()
        {
            try
            {
                foreach (var s in HostsBlocker.GetBlocked())
                {
                    _txtSite.Text = s; AddChip();
                }
                _txtSite.Clear();
                int count = _chipBox.Controls.Count;
                if (count > 0) SetBlockStatus(count, active: true);
            }
            catch { }
        }

        void BtnBlock_Click(object? s, EventArgs e)
        {
            var sites = _chipBox.Controls.OfType<Panel>().Select(p => p.Tag!.ToString()!).ToList();
            if (!sites.Any()) { MessageBox.Show("Add sites first."); return; }
            try { HostsBlocker.BlockSites(sites); SetBlockStatus(sites.Count, active: true); }
            catch (UnauthorizedAccessException) { AdminWarning(); }
        }

        void BtnUnblock_Click(object? s, EventArgs e)
        {
            try { HostsBlocker.UnblockAll(); SetBlockStatus(0, active: false); }
            catch (UnauthorizedAccessException) { AdminWarning(); }
        }

        void SetBlockStatus(int count, bool active)
        {
            _lblBlockStatus.Text      = active ? $"✅  Blocking {count} site(s)" : "Not blocking";
            _lblBlockStatus.ForeColor = active ? Pal.Danger : Pal.TextMid;
        }

        // ═══════════════════════════════════ SCHEDULE PAGE ═══════
        void BuildSchedulePage()
        {
            _pageSched = new Panel { Dock = DockStyle.Fill, BackColor = Pal.OffWhite, Visible = false };

            var card = new Card { Location = new Point(40, 30), Size = new Size(460, 420), BackColor = Pal.White };

            var title = new Label { Text = "Block Schedule", Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Pal.TextDark, Location = new Point(24, 20), AutoSize = true };

            _lblNextSched = new Label { Location = new Point(24, 50), Size = new Size(412, 20),
                Font = new Font("Segoe UI", 9f), ForeColor = Pal.TextMid };

            var btnNew = new TealButton { Text = "+ New Schedule", Size = new Size(150, 32), Location = new Point(286, 16) };
            btnNew.Click += (_, _) => OpenNewScheduleDialog();

            // scrollable list
            _schedList = new Panel { Location = new Point(0, 80), Size = new Size(460, 330),
                AutoScroll = true, BackColor = Pal.White };

            card.Controls.AddRange(new Control[] { title, _lblNextSched, btnNew, _schedList });
            _pageSched.Controls.Add(card);
            _content.Controls.Add(_pageSched);

            // refresh every minute
            var refresh = new Timer { Interval = 60_000 }; refresh.Tick += (_, _) => RefreshNextLabel(); refresh.Start();
        }

        void RefreshScheduleList()
        {
            _schedList.Controls.Clear();
            foreach (var entry in ScheduleManager.Entries.AsEnumerable().Reverse())
            {
                var row = new ScheduleRow(entry);
                row.DeleteClicked += e =>
                {
                    ScheduleManager.Entries.Remove(e);
                    ScheduleManager.Save();
                    RefreshScheduleList();
                };
                _schedList.Controls.Add(row);
            }
            RefreshNextLabel();
        }

        void RefreshNextLabel()
        {
            var active = ScheduleManager.ActiveScheduleName();
            _lblNextSched.Text = string.IsNullOrEmpty(active)
                ? "No active schedule right now."
                : $"🔴  Active now: {active}";
            _lblNextSched.ForeColor = string.IsNullOrEmpty(active) ? Pal.TextMid : Pal.Danger;
        }

        void OpenNewScheduleDialog()
        {
            var dlg = new NewScheduleForm();
            if (dlg.ShowDialog() == DialogResult.OK && dlg.Result != null)
            {
                ScheduleManager.Entries.Add(dlg.Result);
                ScheduleManager.Save();
                ScheduleManager.Evaluate();
                RefreshScheduleList();
            }
        }

        // ─────────────────────────── helpers ─────────────────────
        static void AdminWarning() =>
            MessageBox.Show("Run FocusBlocker as Administrator to modify the hosts file.",
                "Permission Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        protected override void OnFormClosing(FormClosingEventArgs e) { _tray.Visible = false; base.OnFormClosing(e); }
    }

    // ═══════════════════════════════════════════════════════════════
    //  New Schedule Dialog
    // ═══════════════════════════════════════════════════════════════
    class NewScheduleForm : Form
    {
        public ScheduleEntry? Result { get; private set; }

        private TextBox _txtName = null!;
        private CheckedListBox _clbDays = null!;
        private DateTimePicker _dtpStart = null!, _dtpEnd = null!;
        private TextBox _txtSites = null!;

        public NewScheduleForm()
        {
            Text = "New Schedule"; Size = new Size(400, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            BackColor = Pal.White; Font = new Font("Segoe UI", 9.5f);

            int y = 16;
            Controls.Add(Lbl("Schedule name", 16, y)); y += 22;
            _txtName = Txt(16, y, 350); Controls.Add(_txtName); y += 36;

            Controls.Add(Lbl("Days", 16, y)); y += 22;
            _clbDays = new CheckedListBox { Location = new Point(16, y), Size = new Size(350, 100),
                BackColor = Pal.OffWhite, ForeColor = Pal.TextDark, BorderStyle = BorderStyle.FixedSingle };
            foreach (DayOfWeek d in Enum.GetValues(typeof(DayOfWeek))) _clbDays.Items.Add(d, false);
            Controls.Add(_clbDays); y += 110;

            Controls.Add(Lbl("Start time", 16, y));
            Controls.Add(Lbl("End time", 200, y)); y += 22;
            _dtpStart = new DateTimePicker { Format = DateTimePickerFormat.Time, ShowUpDown = true,
                Location = new Point(16, y), Size = new Size(160, 28) };
            _dtpEnd = new DateTimePicker { Format = DateTimePickerFormat.Time, ShowUpDown = true,
                Location = new Point(200, y), Size = new Size(160, 28) };
            _dtpEnd.Value = DateTime.Today.AddHours(17);
            Controls.Add(_dtpStart); Controls.Add(_dtpEnd); y += 38;

            Controls.Add(Lbl("Sites to block (one per line)", 16, y)); y += 22;
            _txtSites = new TextBox { Location = new Point(16, y), Size = new Size(350, 70),
                Multiline = true, ScrollBars = ScrollBars.Vertical,
                BackColor = Pal.OffWhite, ForeColor = Pal.TextDark };
            Controls.Add(_txtSites); y += 80;

            var btnOk = new TealButton { Text = "Save", Size = new Size(100, 32), Location = new Point(16, y) };
            btnOk.Click += BtnOk_Click;
            var btnCancel = new TealButton(outline: true) { Text = "Cancel", Size = new Size(100, 32), Location = new Point(126, y) };
            btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
            Controls.Add(btnOk); Controls.Add(btnCancel);
        }

        void BtnOk_Click(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text)) { MessageBox.Show("Enter a name."); return; }
            var days = _clbDays.CheckedItems.Cast<DayOfWeek>().ToArray();
            if (days.Length == 0) { MessageBox.Show("Select at least one day."); return; }
            var sites = _txtSites.Lines.Select(l => l.Trim().ToLower()).Where(l => !string.IsNullOrEmpty(l)).ToList();
            if (sites.Count == 0) { MessageBox.Show("Add at least one site."); return; }

            Result = new ScheduleEntry
            {
                Name = _txtName.Text.Trim(), Days = days,
                StartTime = _dtpStart.Value.TimeOfDay, EndTime = _dtpEnd.Value.TimeOfDay,
                Sites = sites, Enabled = true
            };
            DialogResult = DialogResult.OK;
        }

        static Label Lbl(string t, int x, int y) =>
            new() { Text = t, Location = new Point(x, y), AutoSize = true, ForeColor = Pal.TextMid };
        static TextBox Txt(int x, int y, int w) =>
            new() { Location = new Point(x, y), Size = new Size(w, 28),
                BackColor = Pal.OffWhite, ForeColor = Pal.TextDark };
    }

    // ═══════════════════════════════════════════════════════════════
    //  Watermark extension
    // ═══════════════════════════════════════════════════════════════
    static class TextBoxExt
    {
        private const int EM_SETCUEBANNER = 0x1501;
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr h, int msg, int wp, string lp);
        public static void SetWatermark(this TextBox tb, string text) =>
            SendMessage(tb.Handle, EM_SETCUEBANNER, 0, text);
    }
}
