/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：ui/UiControls.cs
 * 作者：zhenhua
 * 说明：自绘控件集合：FlatButton / CardPanel / ToggleSwitch / MetricView / StatusPill / HairLine / NavList 等。
 * 存在的意义是让五个模块看起来像同一个产品，而不是各自画一画的五套控件。
 */
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace WinDisplay.Ui
{
    /// <summary>
    /// 扁平圆角按钮。四种变体对应四种语气：主行动 / 普通 / 危险 / 文字。
    /// 自己接管绘制（不调 base.OnPaint），所以不会出现系统默认那层灰底。
    /// </summary>
    internal class FlatButton : Button
    {
        private bool _hover;
        private bool _pressed;

        public FlatButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = UiTheme.Body;
            Cursor = Cursors.Hand;
            Margin = new Padding(0, 0, UiTheme.GapS, 0);
            MinimumSize = new Size(64, UiTheme.MinButtonHeight);
            Padding = new Padding(12, 0, 12, 0);
            Height = UiTheme.MinButtonHeight;
            // 按文字长短取宽：写死宽度迟早会把「恢复 100%」这种标签切掉半截
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        private ButtonVariant _variant = ButtonVariant.Secondary;

        public ButtonVariant Variant
        {
            get => _variant;
            set { _variant = value; Invalidate(); }
        }

        private int _radius = UiTheme.RadiusButton;

        public int CornerRadius
        {
            get => _radius;
            set { _radius = Math.Max(0, value); Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; base.OnMouseEnter(e); Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; base.OnMouseLeave(e); Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; base.OnMouseDown(e); Invalidate(); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; base.OnMouseUp(e); Invalidate(); }



        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color parentBack = UiTheme.EffectiveBack(this);
            using (var b = new SolidBrush(parentBack))
            {
                g.FillRectangle(b, ClientRectangle);   // 盖住圆角外面，免得露出底色方角
            }
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            bool disabled = !Enabled;
            Color back, fore, border = Color.Transparent;
            bool outline = false;

            switch (Variant)
            {
                case ButtonVariant.Primary:
                    back = disabled ? UiTheme.SurfaceMuted : _pressed ? UiTheme.AccentPressed : _hover ? UiTheme.AccentHover : UiTheme.Accent;
                    fore = disabled ? UiTheme.TextDisabled : UiTheme.TextOnAccent;
                    break;
                case ButtonVariant.Danger:
                    back = disabled ? UiTheme.SurfaceMuted : _pressed ? UiTheme.DangerPressed : _hover ? UiTheme.DangerHover : UiTheme.Danger;
                    fore = disabled ? UiTheme.TextDisabled : UiTheme.TextOnAccent;
                    break;
                case ButtonVariant.Ghost:
                    back = disabled ? parentBack : _pressed ? UiTheme.AccentSoft : _hover ? UiTheme.SurfaceHover : parentBack;
                    fore = disabled ? UiTheme.TextDisabled : UiTheme.AccentText;
                    break;
                default: // Secondary
                    back = disabled ? UiTheme.SurfaceMuted : _pressed ? UiTheme.AccentSoft : _hover ? UiTheme.SurfaceHover : UiTheme.Surface;
                    fore = disabled ? UiTheme.TextDisabled : UiTheme.TextPrimary;
                    outline = true;
                    border = disabled ? UiTheme.Border : _pressed ? UiTheme.Accent : _hover ? UiTheme.BorderStrong : UiTheme.Border;
                    break;
            }

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = UiTheme.RoundedRect(rect, CornerRadius))
            {
                if (!disabled || Variant != ButtonVariant.Ghost)
                {
                    using var fb = new SolidBrush(back);
                    g.FillPath(fb, path);
                }
                if (outline)
                {
                    using var pen = new Pen(border, 1f);
                    g.DrawPath(pen, path);
                }
            }

            // 键盘焦点：里圈一条 1px 高亮，鼠标用户看不见，键盘用户看得见
            if (Focused && ShowFocusCues)
            {
                using var ring = UiTheme.RoundedRect(new Rectangle(2, 2, Width - 5, Height - 5), Math.Max(0, CornerRadius - 2));
                using var rp = new Pen(UiTheme.Accent, 1f) { DashStyle = DashStyle.Dot };
                g.DrawPath(rp, ring);
            }

            var textRect = new Rectangle(Padding.Left, 0, Width - Padding.Horizontal, Height);
            TextRenderer.DrawText(g, Text, Font, textRect, fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
              | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        }
    }

    /// <summary>
    /// 白色圆角卡片，可带标题与说明。内容通过 DisplayRectangle 让开标题区，
    /// 所以卡片里放 Dock=Fill 的面板照样对齐。
    /// </summary>
    internal class CardPanel : Panel
    {
        private string _title = string.Empty;
        private string _description = string.Empty;
        private Control? _headerAction;

        public CardPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = UiTheme.Surface;
            Padding = new Padding(UiTheme.GapL);
            Margin = new Padding(0, 0, 0, UiTheme.GapM);
        }

        public int CornerRadius { get; set; } = UiTheme.RadiusCard;

        public string Title
        {
            get => _title;
            set { _title = value ?? string.Empty; Invalidate(); PerformLayout(); }
        }

        /// <summary>标题下面一行小字，用来说明这一组是干什么的、有什么副作用。</summary>
        public string Description
        {
            get => _description;
            set { _description = value ?? string.Empty; Invalidate(); PerformLayout(); }
        }

        /// <summary>标题行右侧的小控件（例如「已选 3 项」），右上角对齐。</summary>
        public Control? HeaderAction
        {
            get => _headerAction;
            set
            {
                if (_headerAction != null) Controls.Remove(_headerAction);
                _headerAction = value;
                if (_headerAction != null)
                {
                    _headerAction.Margin = new Padding(0);
                    Controls.Add(_headerAction);
                }
                Invalidate();
                PerformLayout();
            }
        }

        private bool HasHeader => !string.IsNullOrEmpty(_title) || !string.IsNullOrEmpty(_description);

        public int HeaderHeight
        {
            get
            {
                if (!HasHeader) return 0;
                int h = UiTheme.GapM + 19;                       // 上内边距 + 标题行
                if (!string.IsNullOrEmpty(_description)) h += 16;
                return h + UiTheme.GapM;                          // 标题与内容之间的间隔
            }
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                Rectangle r = base.DisplayRectangle;
                if (!HasHeader) return r;
                int hh = HeaderHeight;
                return new Rectangle(r.Left, r.Top + hh, Math.Max(0, r.Width), Math.Max(0, r.Height - hh));
            }
        }

        /// <summary>
        /// 卡片被放进 AutoSize 行时要能把自己量出来。Panel 默认只问子控件的首选尺寸，
        /// 而 Dock=Fill 的 FlowLayoutPanel 没有自主尺寸，量出来是 0 ——
        /// 结果卡片矮成一截，按钮全画到卡片外面被裁掉。
        /// 这里改成「在给定宽度下，问子控件换行之后真正需要多高」。
        /// </summary>
        public override Size GetPreferredSize(Size proposedSize)
        {
            Size baseSize = base.GetPreferredSize(proposedSize);
            if (proposedSize.Width <= 0) return baseSize;

            Rectangle dr = DisplayRectangle;
            int content = 0;
            foreach (Control c in Controls)
            {
                if (c == _headerAction || !c.Visible) continue;
                int avail = Math.Max(1, dr.Width - c.Margin.Horizontal);
                Size pref = c.GetPreferredSize(new Size(avail, 0));
                content = Math.Max(content, pref.Height + c.Margin.Vertical);
            }

            int height = dr.Top + content + Padding.Bottom;
            // 重写 GetPreferredSize 会绕过框架自动加的 MinimumSize，这里自己兜住
            height = Math.Max(height, MinimumSize.Height);
            return new Size(Math.Max(baseSize.Width, proposedSize.Width),
                            Math.Max(height, HeaderHeight + Padding.Vertical));
        }

        private int _lastSelfHeight;
        private bool _nudging;
        private bool _autoSizeSynced;

        /// <summary>
        /// AutoSize 行里的卡片必须自己报尺寸：框架只向 AutoSize=true 的子控件调 GetPreferredSize，
        /// 否则拿到的是卡片「当前高度」——内容长高了行也不跟着长，按钮全画到卡片外面被裁掉。
        /// Percent 行里相反，必须不开 AutoSize：开了会被缩到内容首选高度，装表格/列表那几张就压成一小条。
        /// 设计器是先 Controls.Add 再加 RowStyles，所以这个判断只能放在 OnLayout 里做。
        /// </summary>
        private void SyncAutoSize()
        {
            bool want = InAutoSizeRow();
            if (AutoSize == want && _autoSizeSynced) return;
            _autoSizeSynced = true;
            AutoSize = want;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }


        /// <summary>卡片是不是坐在一个 AutoSize 行里（跳行、Percent 行、非表格容器都算否）。</summary>
        private bool InAutoSizeRow()
        {
            if (Parent is not TableLayoutPanel tlp) return false;
            TableLayoutPanelCellPosition pos = tlp.GetPositionFromControl(this);
            if (pos.Row < 0 || pos.Row >= tlp.RowStyles.Count) return false;
            if (tlp.GetRowSpan(this) > 1) return false;   // 注意 TableLayoutPanelCellPosition 没有 RowSpan 这个属性
            return tlp.RowStyles[pos.Row].SizeType == SizeType.AutoSize;
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (_headerAction != null && HasHeader)
            {
                int w = _headerAction.PreferredSize.Width;
                int h = Math.Max(_headerAction.PreferredSize.Height, 20);
                _headerAction.SetBounds(Width - Padding.Right - w, UiTheme.GapM + 1, w, h);
            }

            SyncAutoSize();

            // 内容换行后卡片需要的高度变了，但外层 TableLayoutPanel 不一定愿意重量一次 AutoSize 行：
            // Form_net 的设备卡片就是这么卡在单行高度上，第二行小字被裁在卡片外。自己发现差值就把祖先敲一下。
            // Percent 行（表格、列表那种允许拉伸的）不插手，那里高度归父容器。
            if (_nudging || Width <= 0) return;
            Control parent = Parent!;      // 卡片还没入容器时无事可做
            if (!InAutoSizeRow()) return;
            int wanted = GetPreferredSize(new Size(Width, 0)).Height;
            if (wanted <= 0 || wanted == _lastSelfHeight) return;
            _lastSelfHeight = wanted;
            _nudging = true;
            try
            {
                if (Math.Abs(Height - wanted) > 1) Height = wanted;
                parent.SuspendLayout();
                parent.ResumeLayout(true);
            }
            finally
            {
                _nudging = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = UiTheme.RoundedRect(rect, CornerRadius))
            {
                using (var fb = new SolidBrush(BackColor)) g.FillPath(fb, path);
                using var pen = new Pen(UiTheme.Border, 1f);
                g.DrawPath(pen, path);
            }

            if (!HasHeader) return;

            int y = UiTheme.GapM;
            if (!string.IsNullOrEmpty(_title))
            {
                TextRenderer.DrawText(g, _title, UiTheme.CardTitle,
                    new Rectangle(Padding.Left, y, Width - Padding.Horizontal, 19), UiTheme.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                y += 19;
            }
            if (!string.IsNullOrEmpty(_description))
            {
                TextRenderer.DrawText(g, _description, UiTheme.Small,
                    new Rectangle(Padding.Left, y, Width - Padding.Horizontal, 16), UiTheme.TextSecondary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                y += 16;
            }
            using (var lp = new Pen(UiTheme.Border, 1f))
            {
                g.DrawLine(lp, Padding.Left, y + UiTheme.GapM - 1, Width - Padding.Right, y + UiTheme.GapM - 1);
            }
        }
    }

    /// <summary>开关（代替复选框的勾），适合「开/关」语义，例如是否填充 0。</summary>
    internal class ToggleSwitch : CheckBox
    {
        private const int PillWidth = 40;
        private const int PillHeight = 20;
        private const int Gap = 8;
        private bool _hover;

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Font = UiTheme.Body;
            Cursor = Cursors.Hand;
            AutoSize = true;
            // 点击区至少 28 高：药丸本身 20，上下各留 4，文字也顺带居中
            MinimumSize = new Size(0, UiTheme.MinButtonHeight);
            Margin = new Padding(0, 0, UiTheme.GapM, 0);
            Padding = new Padding(0, 4, 0, 4);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int w = PillWidth + Gap;
            if (!string.IsNullOrEmpty(Text))
            {
                Size t = TextRenderer.MeasureText(Text, Font);
                w += t.Width + 2;
            }
            return new Size(w, Math.Max(UiTheme.MinButtonHeight, Math.Max(PillHeight + Padding.Vertical, Height)));
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; base.OnMouseEnter(e); Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; base.OnMouseLeave(e); Invalidate(); }
        protected override void OnCheckedChanged(EventArgs e) { base.OnCheckedChanged(e); Invalidate(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color parentBack = UiTheme.EffectiveBack(this);
            using (var b = new SolidBrush(parentBack)) g.FillRectangle(b, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int top = (Height - PillHeight) / 2;
            var pill = new Rectangle(0, top, PillWidth - 1, PillHeight - 1);
            Color track = !Enabled
                ? UiTheme.Border
                : Checked ? UiTheme.Accent : _hover ? UiTheme.BorderStrong : UiTheme.Border;

            using (var path = UiTheme.RoundedRect(pill, PillHeight / 2))
            {
                using (var fb = new SolidBrush(track)) g.FillPath(fb, path);
            }

            int knob = PillHeight - 6;
            int kx = Checked ? pill.Right - knob - 2 : pill.Left + 2;
            var knobRect = new Rectangle(kx, pill.Top + (pill.Height - knob) / 2, knob, knob);
            using (var kb = new SolidBrush(Enabled ? Color.White : UiTheme.SurfaceMuted))
            {
                g.FillEllipse(kb, knobRect);
            }
            using (var kp = new Pen(Color.FromArgb(40, 0, 0, 0), 1f))
            {
                g.DrawEllipse(kp, knobRect);
            }

            if (Focused && ShowFocusCues)
            {
                using var rp = new Pen(UiTheme.Accent, 1f) { DashStyle = DashStyle.Dot };
                g.DrawRectangle(rp, -2, top - 3, PillWidth + 3, PillHeight + 5);
            }

            if (!string.IsNullOrEmpty(Text))
            {
                TextRenderer.DrawText(g, Text, Font,
                    new Rectangle(PillWidth + Gap, 0, Math.Max(0, Width - PillWidth - Gap), Height),
                    Enabled ? UiTheme.TextPrimary : UiTheme.TextDisabled,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
            }
        }
    }

    /// <summary>大号数字 + 单位 + 上方小标题，用来看总量、总数这类关键指标。</summary>
    internal class MetricView : Control
    {
        private string _value = "0";
        private string _unit = string.Empty;
        private string _caption = string.Empty;

        public MetricView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Height = 58;
            Width = 160;
            Margin = new Padding(0, 0, UiTheme.GapL, 0);
        }

        public string Value { get => _value; set { _value = value ?? string.Empty; Invalidate(); } }
        public string Unit { get => _unit; set { _unit = value ?? string.Empty; Invalidate(); } }
        public string Caption { get => _caption; set { _caption = value ?? string.Empty; Invalidate(); } }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color parentBack = UiTheme.EffectiveBack(this);
            using (var b = new SolidBrush(parentBack)) g.FillRectangle(b, ClientRectangle);
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int y = 0;
            if (!string.IsNullOrEmpty(_caption))
            {
                TextRenderer.DrawText(g, _caption, UiTheme.Caption,
                    new Rectangle(0, y, Width, 15), UiTheme.TextTertiary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                y += 17;
            }

            Size vs = TextRenderer.MeasureText(_value, UiTheme.Metric);
            TextRenderer.DrawText(g, _value, UiTheme.Metric,
                new Rectangle(0, y, Math.Min(vs.Width + 2, Width), 28), UiTheme.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);

            if (!string.IsNullOrEmpty(_unit))
            {
                int ux = Math.Min(vs.Width + 4, Math.Max(0, Width - 20));
                TextRenderer.DrawText(g, _unit, UiTheme.MetricUnit,
                    new Rectangle(ux, y + 8, Math.Max(0, Width - ux), 16), UiTheme.TextSecondary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
            }
        }
    }

    /// <summary>状态胶囊：一个小圆点 + 文案，背景是浅色。用来标注「已停止 / 需管理员 / 共 12 项」。</summary>
    internal class StatusPill : Control
    {
        private const int PadX = 9;
        private const int Dot = 7;

        public StatusPill()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = UiTheme.SurfaceMuted;
            ForeColor = UiTheme.TextSecondary;
            Font = UiTheme.Small;
            DotColor = UiTheme.TextTertiary;
            Height = 22;
            Margin = new Padding(0, 0, UiTheme.GapS, 0);
        }

        public Color DotColor { get; set; }

        /// <summary>为 true 时宽度随文字自动变（放进 FlowLayoutPanel 时用）。</summary>
        public bool FitText { get; set; } = true;

        public void SetState(string text, Color dot, Color back, Color fore)
        {
            DotColor = dot;
            BackColor = back;
            ForeColor = fore;
            Text = text;
        }

        [System.Diagnostics.CodeAnalysis.AllowNull]
        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                if (FitText)
                {
                    Size t = TextRenderer.MeasureText(Text ?? string.Empty, Font);
                    Size = new Size(t.Width + Dot + 5 + PadX * 2, 22);
                }
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color parentBack = UiTheme.EffectiveBack(this);
            using (var b = new SolidBrush(parentBack)) g.FillRectangle(b, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = UiTheme.RoundedRect(rect, Height / 2))
            {
                using (var fb = new SolidBrush(BackColor)) g.FillPath(fb, path);
            }
            UiTheme.FillDot(g, DotColor, PadX, (Height - Dot) / 2, Dot);
            TextRenderer.DrawText(g, Text, Font,
                new Rectangle(PadX + Dot + 5, 0, Math.Max(0, Width - PadX * 2 - Dot - 5), Height),
                Enabled ? ForeColor : UiTheme.TextDisabled,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        }
    }

    /// <summary>1px 分隔线，深色/浅色背景都能用。</summary>
    internal class HairLine : Panel
    {
        public HairLine()
        {
            Height = 1;
            BackColor = UiTheme.Border;
            Margin = new Padding(0, UiTheme.GapS, 0, UiTheme.GapS);
        }
    }

    /// <summary>自绘进度条：细圆角轨道 + 主色填充。原生 ProgressBar 改不了样式。</summary>
    internal class UiProgressBar : ProgressBar
    {
        public UiProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Height = 10;
            Minimum = 0;
            Maximum = 100;
        }

        public Color FillColor { get; set; } = UiTheme.Accent;

        /// <summary>
        /// ProgressBar 没有 ValueChanged 事件，自绘就看不到进度变化，
        /// 所以把 Value 封一层：赋值后立即重画。写代码时照旧 bar.Value = 42 即可。
        /// </summary>
        public new int Value
        {
            get => base.Value;
            set
            {
                if (base.Value == value) return;
                base.Value = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color parentBack = UiTheme.EffectiveBack(this);
            using (var b = new SolidBrush(parentBack)) g.FillRectangle(b, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var track = new Rectangle(0, (Height - 8) / 2, Width - 1, 7);
            using (var path = UiTheme.RoundedRect(track, 4))
            {
                using var tb = new SolidBrush(UiTheme.SurfaceHover);
                g.FillPath(tb, path);
            }

            int span = Math.Max(1, Maximum - Minimum);
            double frac = Math.Clamp((Value - Minimum) / (double)span, 0d, 1d);
            if (frac <= 0) return;

            int w = Math.Max(8, (int)Math.Round((Width - 1) * frac));
            var fill = new Rectangle(0, track.Top, w, track.Height);
            using (var path = UiTheme.RoundedRect(fill, 4))
            {
                using var fb = new SolidBrush(FillColor);
                g.FillPath(fb, path);
            }
        }
    }

    /// <summary>页面标题区：大标题 + 一行说明 + 右侧动作区，底部一条分隔线。</summary>
    internal class PageHeader : Panel
    {
        private readonly FlowLayoutPanel _actions = new FlowLayoutPanel();

        public PageHeader()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = UiTheme.WindowBackground;
            Height = 56;
            Dock = DockStyle.Fill;
            Margin = new Padding(0, 0, 0, UiTheme.GapM);
            _actions.WrapContents = false;
            _actions.AutoSize = true;
            _actions.BackColor = Color.Transparent;
            _actions.Margin = new Padding(0);
            _actions.Visible = false;   // 没有动作按钮时不占位，也别出现在控件树里
            Controls.Add(_actions);
        }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>标题右侧放按钮/胶囊（自动从右往左排）。</summary>
        public void AddAction(params Control[] controls)
        {
            foreach (Control c in controls)
            {
                _actions.Controls.Add(c);
            }
            _actions.Visible = _actions.Controls.Count > 0;
            PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (!_actions.Visible) return;
            // FlowLayoutPanel 自己会算首选宽度（AutoMargin + 子控件首选尺寸），直接问它
            Size pref = _actions.GetPreferredSize(new Size(int.MaxValue, Height));
            int w = Math.Min(pref.Width + UiTheme.GapL, Width);
            _actions.SetBounds(Math.Max(0, Width - w), Math.Max(0, (Height - Math.Min(pref.Height, Height)) / 2), w, Math.Min(pref.Height, Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (var b = new SolidBrush(BackColor)) g.FillRectangle(b, ClientRectangle);
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // 标题左边一条主色竖条，给每页一个明确的「你现在在哪」
            using (var ab = new SolidBrush(UiTheme.Accent))
            {
                g.FillRectangle(ab, 0, 4, 3, Math.Max(8, Height - 20));
            }

            int ty = string.IsNullOrEmpty(Description) ? (Height - 22) / 2 : 4;
            TextRenderer.DrawText(g, Title, UiTheme.PageTitle,
                new Rectangle(12, ty, Math.Max(0, Width - 24), 24), UiTheme.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);

            if (!string.IsNullOrEmpty(Description))
            {
                TextRenderer.DrawText(g, Description, UiTheme.Small,
                    new Rectangle(12, ty + 24, Math.Max(0, Width - 24), 16), UiTheme.TextSecondary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
            }

            using (var p = new Pen(UiTheme.Border, 1f))
            {
                g.DrawLine(p, 0, Height - 1, Width, Height - 1);
            }
        }
    }

    /// <summary>深色侧边导航：一项一个模块，选中项是主色圆角块。</summary>
    internal class NavList : Control
    {
        public sealed class NavItem
        {
            public NavItem(string text, object? tag = null, string? description = null)
            {
                Text = text;
                Tag = tag;
                Description = description;
            }

            public string Text { get; }
            public string? Description { get; }
            public object? Tag { get; }
        }

        private const int MinItemHeight = 40;
        private const int ItemGap = 8;      // 条目之间：原来只给 4px，两行挤在一起看不出是分开的条目
        private const int TopInset = 10;    // 第一条与 brand 块之间留一段，不要顶在块上
        private const int SidePadding = 12;
        private const int TextPadX = 14;
        private const int TextPadY = 7;     // 文字到圆角块上、下边缘的最小距离（下边距不能是 0）
        private const int DescGap = 3;      // 标题行与说明行之间
        private int _hoverIndex = -1;
        private int _selected = -1;
        private int _lineTitle = -1;
        private int _lineDesc = -1;

        public NavList()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                   | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = UiTheme.Sidebar;
            Font = UiTheme.Body;
            Width = 216;
            TabStop = true;
        }

        public List<NavItem> Items { get; } = new List<NavItem>();

        public event EventHandler? SelectedIndexChanged;

        /// <summary>请求切换页面；由宿主返回 false 时可以阻止切换（例如某个任务正在跑）。</summary>
        public Func<int, bool>? ConfirmChange;

        public int SelectedIndex
        {
            get => _selected;
            set
            {
                if (value < 0 || value >= Items.Count) return;
                if (value == _selected) return;
                if (ConfirmChange != null && !ConfirmChange(value)) return;
                _selected = value;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public NavItem? SelectedItem => _selected >= 0 && _selected < Items.Count ? Items[_selected] : null;

        public void Add(string text, object? tag = null, string? description = null)
        {
            Items.Add(new NavItem(text, tag, description));
            Invalidate();
            OnResize(EventArgs.Empty);
        }

        private static int LineHeight(Font f) => TextRenderer.MeasureText("Ag", f).Height;

        /// <summary>标题行高。Font 会被窗体的 AutoScaleMode.Font 按 DPI 缩放，所以行高跟着字体走，不写死 40。</summary>
        private int TitleLine => _lineTitle > 0 ? _lineTitle : (_lineTitle = LineHeight(Font));

        private int DescLine => _lineDesc > 0 ? _lineDesc : (_lineDesc = LineHeight(UiTheme.Caption));

        protected override void OnFontChanged(EventArgs e)
        {
            _lineTitle = -1;
            _lineDesc = -1;
            base.OnFontChanged(e);
            Invalidate();
        }

        /// <summary>
        /// 条目高度 = 上下文字留白 + （单行：标题）/（两行：标题 + 说明）。
        /// 以前不管几行都是 40px，带说明的条目里小字底边只剩 3px，字像是贴在下边缘上。
        /// </summary>
        private int ItemHeightAt(int index)
        {
            int block = TitleLine;
            if (!string.IsNullOrEmpty(Items[index].Description)) block += DescGap + DescLine;
            return Math.Max(MinItemHeight, block + TextPadY * 2);
        }

        private int TopAt(int index)
        {
            int y = TopInset;
            for (int i = 0; i < index; i++) y += ItemHeightAt(i) + ItemGap;
            return y;
        }

        private Rectangle ItemRect(int index) =>
            new Rectangle(SidePadding, TopAt(index), Math.Max(0, Width - SidePadding * 2), ItemHeightAt(index));

        /// <summary>整列需要多高，给上层 AutoSize 用。</summary>
        public override Size GetPreferredSize(Size proposedSize)
        {
            int h = TopInset;
            for (int i = 0; i < Items.Count; i++)
            {
                if (i > 0) h += ItemGap;
                h += ItemHeightAt(i);
            }
            return new Size(proposedSize.Width > 0 ? proposedSize.Width : 216, Math.Max(0, h + TopInset));
        }

        private int HitTest(Point p)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (ItemRect(i).Contains(p)) return i;
            }
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int h = HitTest(e.Location);
            if (h != _hoverIndex)
            {
                _hoverIndex = h;
                Cursor = h >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (_hoverIndex != -1)
            {
                _hoverIndex = -1;
                Invalidate();
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            int h = HitTest(e.Location);
            if (h >= 0) SelectedIndex = h;
            base.OnMouseDown(e);
        }

        protected override bool IsInputKey(Keys keyData) =>
            keyData is Keys.Up or Keys.Down or Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown
                || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up when Items.Count > 0:
                    SelectedIndex = Math.Max(0, _selected - 1); e.Handled = true; break;
                case Keys.Down when Items.Count > 0:
                    SelectedIndex = Math.Min(Items.Count - 1, _selected + 1); e.Handled = true; break;
                case Keys.Home when Items.Count > 0:
                    SelectedIndex = 0; e.Handled = true; break;
                case Keys.End when Items.Count > 0:
                    SelectedIndex = Items.Count - 1; e.Handled = true; break;
            }
            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (var b = new SolidBrush(BackColor)) g.FillRectangle(b, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            for (int i = 0; i < Items.Count; i++)
            {
                Rectangle r = ItemRect(i);
                if (r.Top > Height) break;
                bool selected = i == _selected;
                if (selected)
                {
                    using var path = UiTheme.RoundedRect(new Rectangle(r.Left, r.Top, r.Width - 1, r.Height - 1), 8);
                    using var fb = new SolidBrush(UiTheme.SidebarActive);
                    g.FillPath(fb, path);
                }
                else if (i == _hoverIndex)
                {
                    using var path = UiTheme.RoundedRect(new Rectangle(r.Left, r.Top, r.Width - 1, r.Height - 1), 8);
                    using var fb = new SolidBrush(UiTheme.SidebarHover);
                    g.FillPath(fb, path);
                }

                var fore = selected ? Color.White : UiTheme.TextOnSidebar;
                var nav = Items[i];
                bool twoLine = !string.IsNullOrEmpty(nav.Description);

                // 两行文字作为一个整块在条目里垂直居中，这样上、下留白是对称的（以前是顶在 r.Top+5，
                // 小字画到 r.Top+23..+37，底边只剩 3px）
                int titleTop, titleH, descTop, descH;
                if (twoLine)
                {
                    int block = TitleLine + DescGap + DescLine;
                    int y = r.Top + Math.Max(TextPadY, (r.Height - block) / 2);
                    titleTop = y;
                    titleH = TitleLine;
                    descTop = y + TitleLine + DescGap;
                    descH = DescLine;
                }
                else
                {
                    titleTop = r.Top;
                    titleH = r.Height;
                    descTop = descH = 0;
                }

                TextRenderer.DrawText(g, nav.Text, selected ? UiTheme.BodyBold : Font,
                    new Rectangle(r.Left + TextPadX, titleTop, r.Width - TextPadX * 2, titleH), fore,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                if (twoLine)
                {
                    TextRenderer.DrawText(g, nav.Description, UiTheme.Caption,
                        new Rectangle(r.Left + TextPadX, descTop, r.Width - TextPadX * 2, descH),
                        selected ? Color.FromArgb(225, 255, 255, 255) : UiTheme.TextOnSidebarDim,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                }
            }
        }
    }

    /// <summary>
    /// 模块页面的统一骨架：顶部标题区 + 内容区（+ 可选底栏）。
    /// 只有一层 Dock=Fill，不依赖多个 Dock 控件的 Z 序，跨 DPI 也稳。
    /// </summary>
    internal sealed class UiPage
    {
        public TableLayoutPanel Root { get; }
        public PageHeader Header { get; }
        public TableLayoutPanel Content { get; }

        private UiPage(TableLayoutPanel root, PageHeader header, TableLayoutPanel content)
        {
            Root = root;
            Header = header;
            Content = content;
        }

        /// <summary>把 parent 上已有的控件搬进内容区，并装上统一骨架。</summary>
        public static UiPage Build(Control parent, string title, string description)
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(UiTheme.PagePadding, UiTheme.GapL, UiTheme.PagePadding, UiTheme.PagePadding),
                BackColor = UiTheme.WindowBackground,
                Margin = new Padding(0)
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize, 0f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var header = new PageHeader { Title = title, Description = description };

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 0,
                Padding = new Padding(0),
                Margin = new Padding(0),
                BackColor = Color.Transparent,
                AutoScroll = true
            };

            var page = new UiPage(root, header, content);

            // 原来挂在窗体上的控件搬进内容区，最后一格允许拉伸
            var existing = new Control[parent.Controls.Count];
            parent.Controls.CopyTo(existing, 0);
            foreach (Control c in existing)
            {
                parent.Controls.Remove(c);
                c.Dock = DockStyle.Fill;
                page.AddCard(c, stretch: existing[^1] == c);
            }

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(content, 0, 1);
            parent.Controls.Add(root);
            parent.BackColor = UiTheme.WindowBackground;
            return page;
        }

        /// <summary>往内容区底部追加一张卡片（内容区只有一列，所以控件序号就是行号）。</summary>
        public void AddCard(Control card, bool stretch = false)
        {
            int row = Content.Controls.Count;
            Content.Controls.Add(card, 0, row);
            Content.RowStyles.Add(new RowStyle(stretch ? SizeType.Percent : SizeType.AutoSize, stretch ? 100f : 0f));
            Content.RowCount = row + 1;
        }
    }
}
