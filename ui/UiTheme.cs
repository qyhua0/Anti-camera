/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：ui/UiTheme.cs
 * 作者：zhenhua
 * 说明：全局设计系统：配色、字体、圆角、间距，以及把默认控件刷成这套样式的工具方法。
 * 所有颜色只在这里定义一次，界面代码请引用这里的常量，不要在别处写 Color.FromArgb。
 */
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace WinDisplay.Ui
{
    /// <summary>界面变体：主行动 / 普通 / 危险 / 无边框文字。</summary>
    public enum ButtonVariant
    {
        /// <summary>主行动：实心蓝底白字（如「开始遮挡」「立即清理」），一个界面里最多放一个。</summary>
        Primary,
        /// <summary>普通：白底描边，次级动作（如「取消」「刷新列表」）。</summary>
        Secondary,
        /// <summary>危险：红底白字，动作不可逆或会删数据（如「彻底擦除」）。</summary>
        Danger,
        /// <summary>无边框文字：平时只有文字，悬停才出现底色，用于卡片内的轻量操作。</summary>
        Ghost
    }

    /// <summary>
    /// 全局设计系统：配色、字体、圆角、间距，以及把默认控件刷成这套样式的工具方法。
    /// 目的是让每个窗体看起来像同一个产品，而不是五个各自画一画的窗体。
    /// 所有颜色只在这里定义一次；界面代码请引用这里的常量，不要在别处写 Color.FromArgb。
    /// </summary>
    internal static class UiTheme
    {
        // ---------- 调色板 ----------
        /// <summary>页面底色（窗体与 body 容器的 BackColor），比纯白暗一档，避免大面积白光刺眼。</summary>
        public static readonly Color WindowBackground = FromHex("#F4F6F9");
        /// <summary>面色：卡片、输入框、菜单等浮在页面底色之上的那块白。</summary>
        public static readonly Color Surface = Color.White;
        /// <summary>暗面色：列表交替行、表头、只读区块的底，比 Surface 低一层。</summary>
        public static readonly Color SurfaceMuted = FromHex("#F7F9FC");
        /// <summary>悬停底色：导航项、列表行、菜单项 hover 时铺的那层浅蓝灰。</summary>
        public static readonly Color SurfaceHover = FromHex("#EEF3FB");
        /// <summary>常规 1px 描边：卡片边框、输入框边框、分隔线。</summary>
        public static readonly Color Border = FromHex("#DFE5EE");
        /// <summary>加重描边：按钮边框、菜单项高亮边、需要一眼看到的分隔。</summary>
        public static readonly Color BorderStrong = FromHex("#C6D1E0");

        /// <summary>一级文字：标题、数值、需要强调的正文。</summary>
        public static readonly Color TextPrimary = FromHex("#16202B");
        /// <summary>二级文字：默认 Label 色，一般说明与字段名。</summary>
        public static readonly Color TextSecondary = FromHex("#4B5A6E");
        /// <summary>次要说明文字。再浅一点在页面底色 #F4F6F9 上就掉到 4.5 以下了，别继续调淡。</summary>
        public static readonly Color TextTertiary = FromHex("#5F6E83");
        /// <summary>禁用态文字。只用于真的点不动的控件，别拿它当次要文字用（对比度已不达标）。</summary>
        public static readonly Color TextDisabled = FromHex("#9AA6B6");
        /// <summary>实心 Accent / Danger 按钮上的文字色，与两种底色对比都够 4.5。</summary>
        public static readonly Color TextOnAccent = Color.White;

        /// <summary>主行动色：实心按钮、选中态、进度条填充。</summary>
        public static readonly Color Accent = FromHex("#2563EB");
        /// <summary>主行动按钮悬停态（比 Accent 深一档）。</summary>
        public static readonly Color AccentHover = FromHex("#1D4ED8");
        /// <summary>主行动按钮按下态（三档里最深，用于给出按压反馈）。</summary>
        public static readonly Color AccentPressed = FromHex("#1B44B8");
        /// <summary>浅蓝底：选中行、浅色标签、提示条；上面的文字请用 AccentText。</summary>
        public static readonly Color AccentSoft = FromHex("#EAF1FE");
        /// <summary>蓝色小字专用（比 Accent 深），9pt 以下蓝字一律用它而不是 Accent。</summary>
        public static readonly Color AccentText = FromHex("#1D4ED8");

        /// <summary>成功色：绿色状态圆点、成功图标；小字请用 SuccessText。</summary>
        public static readonly Color Success = FromHex("#15803D");
        /// <summary>绿色圆点用 Success，绿色小字要用这个（Success 在 SuccessSoft 上只有 4.48:1）。</summary>
        public static readonly Color SuccessText = FromHex("#12662F");
        /// <summary>浅绿底：成功提示条、已完成标签的底色。</summary>
        public static readonly Color SuccessSoft = FromHex("#E9F5EC");
        /// <summary>警示色：琥珀圆点与图形元素；小字请用 WarningText。</summary>
        public static readonly Color Warning = FromHex("#B45309");
        /// <summary>浅琥珀底：需要注意但不阻止操作的提示条底色。</summary>
        public static readonly Color WarningSoft = FromHex("#FBF1E2");
        /// <summary>琥珀色只用作底色/圆点；作为 9pt 小字对比度差一点，文字请用 WarningText。</summary>
        public static readonly Color WarningText = FromHex("#8A4A08");
        /// <summary>危险色：Danger 变体按钮底色（「彻底擦除」这类），也是错误图标色。</summary>
        public static readonly Color Danger = FromHex("#C0392F");
        /// <summary>危险按钮悬停态。</summary>
        public static readonly Color DangerHover = FromHex("#A62F26");
        /// <summary>危险按钮按下态。</summary>
        public static readonly Color DangerPressed = FromHex("#8E281F");
        /// <summary>浅红底：危险操作确认条、不可清理项的提示底色。</summary>
        public static readonly Color DangerSoft = FromHex("#FBECEA");
        /// <summary>红色小字专用，红字在浅红底上比 Danger 更稳。</summary>
        public static readonly Color DangerText = FromHex("#A62F26");

        /// <summary>左侧导航栏（深色），和浅色内容区形成层次。</summary>
        public static readonly Color Sidebar = FromHex("#1B2430");
        /// <summary>导航项悬停底色（在 Sidebar 上比面色提亮一档，不是压暗）。</summary>
        public static readonly Color SidebarHover = FromHex("#26313F");
        /// <summary>导航项选中态底色，与 Accent 同色以呼应主行动色。</summary>
        public static readonly Color SidebarActive = FromHex("#2563EB");
        /// <summary>导航项正常文字色。</summary>
        public static readonly Color TextOnSidebar = FromHex("#DCE4EE");
        /// <summary>导航项次要文字色（模块副标题、未选中项说明），深色底专用，别搬到浅色区用。</summary>
        public static readonly Color TextOnSidebarDim = FromHex("#9FADBE");

        // ---------- 间距刻度（4 的倍数，别随手写奇数）----------
        /// <summary>极小间距 4px：图标与文字之间、同一行内两个紧挨的元素之间。</summary>
        public const int GapXS = 4;
        /// <summary>小间距 8px：按钮内边距、成组控件的横向间距。</summary>
        public const int GapS = 8;
        /// <summary>常规间距 12px：卡片内相邻行、表单字段之间。</summary>
        public const int GapM = 12;
        /// <summary>大间距 16px：卡片与卡片之间。</summary>
        public const int GapL = 16;
        /// <summary>超大间距 24px：页面区块之间、左右两列之间。</summary>
        public const int GapXL = 24;
        /// <summary>页边距 20px：body 容器四周留白，所有模块一致。</summary>
        public const int PagePadding = 20;
        /// <summary>卡片圆角半径 8px，传给 RoundedRect 使用。</summary>
        public const int RadiusCard = 8;
        /// <summary>按钮圆角半径 6px，比卡片小一档以拉开层次。</summary>
        public const int RadiusButton = 6;
        /// <summary>按钮最小高度 28px：再矮点击热区就不够，中文还会贴住上下边。</summary>
        public const int MinButtonHeight = 28;

        // ---------- 字体 ----------
        /// <summary>字体族名的惰性缓存；null 表示还没查过系统已装字体，首次读 FontFamily 时填充。</summary>
        private static string? _family;

        /// <summary>
        /// 中文字体族：优先微软雅黑 UI（比雅黑更适合界面），退化到雅黑 / Segoe UI。
        /// 只枚举一次系统字体并缓存，因此运行期新装字体不会被认到，重启才生效。
        /// </summary>
        public static string FontFamily
        {
            get
            {
                if (_family == null)
                {
                    _family = "Segoe UI";
                    var want = new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Segoe UI" };
                    var have = new HashSet<string>(System.Drawing.FontFamily.Families.Select(f => f.Name));
                    foreach (string name in want)
                    {
                        if (have.Contains(name))
                        {
                            _family = name;
                            break;
                        }
                    }
                }
                return _family;
            }
        }

        /// <summary>按主题字体族造一个字体，size 单位是磅（pt）；这些字体属性都是进程级单例，别 Dispose。</summary>
        private static Font Make(float size, FontStyle style = FontStyle.Regular) => new Font(FontFamily, size, style);

        /// <summary>品牌标题 13.5pt 粗体：顶部 brand 区的「x-保护」。</summary>
        public static Font Brand { get; } = Make(13.5f, FontStyle.Bold);
        /// <summary>页面主标题 15.5pt 粗体：PageHeader 用，一个页面只出现一次。</summary>
        public static Font PageTitle { get; } = Make(15.5f, FontStyle.Bold);
        /// <summary>卡片标题 10.5pt 粗体：CardPanel 左上角的区块名。</summary>
        public static Font CardTitle { get; } = Make(10.5f, FontStyle.Bold);
        /// <summary>正文基准 9.5pt：控件默认字体、按钮、标签一律用它。</summary>
        public static Font Body { get; } = Make(9.5f);
        /// <summary>正文加粗 9.5pt：需要强调但还不到卡片标题量级的文字。</summary>
        public static Font BodyBold { get; } = Make(9.5f, FontStyle.Bold);
        /// <summary>小号 9pt：列表行、表格单元格等密集区域。</summary>
        public static Font Small { get; } = Make(9f);
        /// <summary>脚注 8.25pt：单位、辅助说明。太小，必须配 TextSecondary 或更深的文字色。</summary>
        public static Font Caption { get; } = Make(8.25f);
        /// <summary>大数字 21pt 粗体：MetricView 的主数值，配合 MetricUnit 排成一行。</summary>
        public static Font Metric { get; } = Make(21f, FontStyle.Bold);
        /// <summary>大数字后面的单位（GB、个、%）9.5pt 粗体。</summary>
        public static Font MetricUnit { get; } = Make(9.5f, FontStyle.Bold);
        /// <summary>等宽字体：IP、MAC、路径、日志。固定 Consolas，不走中文字体族。</summary>
        public static Font Mono { get; } = new Font("Consolas", 9f);

        /// <summary>把窗体的默认字体设成主题字体，须在创建任何控件之前调用。</summary>
        public static void ApplyDefaultFont()
        {
            try { Application.SetDefaultFont(Body); }
            catch
            {
                // 老框架或已有控件句柄时会抛，不影响功能，控件继续用原字体
            }
        }

        /// <summary>
        /// 由 #RRGGBB 文本构造颜色，只支持 6 位十六进制，Alpha 恒为不透明。
        /// 只在上面调色板字段的初始化里用，界面代码请引用现成常量，不要在绘制路径里反复解析。
        /// </summary>
        /// <param name="hex">形如 "#2563EB" 的色值，前导 # 可省；位数不足会抛 FormatException。</param>
        /// <returns>不透明的 RGB 颜色。</returns>
        public static Color FromHex(string hex)
        {
            hex = hex.TrimStart('#');
            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// WCAG 相对亮度对比度，区间 1~21；分子分母会自动换位，所以与参数顺序无关。
        /// 小字要求 ≥ 4.5，大号字和图形元素 ≥ 3。自检值：#64748b 对 #ffffff 必须得 4.76。
        /// </summary>
        /// <param name="fore">前景色（文字或图形颜色）。</param>
        /// <param name="back">背景色；半透明底色先用 EffectiveBack 求出真实颜色。</param>
        /// <returns>对比度倍数，1 表示与背景完全一致（等于看不见）。</returns>
        public static double Contrast(Color fore, Color back)
        {
            double lum(Color c)
            {
                double Chan(byte v)
                {
                    double s = v / 255.0;
                    return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
                }
                return 0.2126 * Chan(c.R) + 0.7152 * Chan(c.G) + 0.0722 * Chan(c.B);
            }
            double a = lum(fore), b = lum(back);
            if (a < b) { double t = a; a = b; b = t; }
            return (a + 0.05) / (b + 0.05);
        }

        /// <summary>
        /// 沿父链找到真正垫在底下的背景色（半透明/未设置时一路往上问）。
        /// Alpha 大于 200 才算不透明；整条链都透明时兜底返回 WindowBackground。
        /// </summary>
        /// <param name="c">要取底色的控件，从它自己开始往上遍历 Parent。</param>
        /// <returns>一个不透明背景色，用于按对比度挑文字色。</returns>
        public static Color EffectiveBack(Control c)
        {
            Control? cur = c;
            while (cur != null)
            {
                Color back = cur.BackColor;
                if (back.A > 200) return back;
                cur = cur.Parent;
            }
            return WindowBackground;
        }

        /// <summary>
        /// 圆角矩形路径（内缩 0.5px 让 1px 描边落在像素正中，边缘更实）。
        /// radius 为 0 或尺寸退化时自动退化成普通矩形，宽高还会夹到非负，调用方不必自己判空。
        /// </summary>
        /// <param name="r">目标矩形，一般是控件的 ClientRectangle。</param>
        /// <param name="radius">圆角半径（像素）；直径取 radius*2-1，并自动夹到短边长度以内。</param>
        /// <param name="inset">四边内缩量（像素）；1px 描边用默认 0.5，不描边可传 0。</param>
        /// <returns>闭合的 GraphicsPath，调用方负责 Dispose。</returns>
        public static GraphicsPath RoundedRect(Rectangle r, int radius, double inset = 0.5)
        {
            var path = new GraphicsPath();
            int d = Math.Max(0, radius * 2 - 1);
            float x = r.Left + (float)inset, y = r.Top + (float)inset;
            float w = Math.Max(0, r.Width - 1 - 2 * (float)inset);
            float h = Math.Max(0, r.Height - 1 - 2 * (float)inset);
            if (d <= 0 || w <= 0 || h <= 0)
            {
                path.AddRectangle(new RectangleF(x, y, Math.Max(0, w), Math.Max(0, h)));
                return path;
            }
            d = Math.Min(d, (int)Math.Min(w, h));
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// 实心圆点（状态指示用，代替容易显示成方框的图标字体）。
        /// 内部临时打开抗锯齿再还原原值，调用处不用管 SmoothingMode。
        /// </summary>
        /// <param name="g">目标 Graphics。</param>
        /// <param name="color">圆点颜色，一般取 Success / Warning / Danger。</param>
        /// <param name="x">外接矩形左上角 X（像素）。</param>
        /// <param name="y">外接矩形左上角 Y（像素）。</param>
        /// <param name="diameter">直径（像素），状态灯常用 8。</param>
        public static void FillDot(Graphics g, Color color, int x, int y, int diameter)
        {
            var old = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var b = new SolidBrush(color);
            g.FillEllipse(b, x, y, diameter, diameter);
            g.SmoothingMode = old;
        }

        // ---------- 把默认控件刷成主题样式 ----------

        /// <summary>
        /// 递归刷样式：用于那些还没逐块重排的窗体/控件，保证不会出现「一半现代一半 Windows95」。
        /// 已经是主题控件（FlatButton/CardPanel 等）自己会画，跳过。
        /// </summary>
        /// <param name="root">递归起点，通常是窗体本身；要在 InitializeComponent 之后调用，否则新建控件不受影响。</param>
        public static void ApplyControlTree(Control root)
        {
            StyleOne(root);
            foreach (Control c in root.Controls)
            {
                ApplyControlTree(c);
            }
        }

        /// <summary>
        /// 单个控件的样式映射：按类型分支刷配色与字体，命中即返回，所以分支顺序有意义——
        /// 派生类型必须排在基类之前（LinkLabel 在 Label 前），主题自绘控件一律原样跳过。
        /// </summary>
        /// <param name="c">待刷样的控件；ProgressBar 这类无法改样的控件不做处理。</param>
        private static void StyleOne(Control c)
        {
            switch (c)
            {
                case Form f:
                    f.BackColor = WindowBackground;
                    f.Font = Body;
                    return;
                case NavList:
                case CardPanel:
                case FlatButton:
                case ToggleSwitch:
                case UiProgressBar:
                case MetricView:
                case StatusPill:
                case HairLine:
                case PageHeader:
                    return;
                case TabPage tp:
                    tp.BackColor = WindowBackground;
                    tp.UseVisualStyleBackColor = false;
                    return;
                case GroupBox gb:
                    gb.ForeColor = TextPrimary;
                    gb.Font = CardTitle;
                    return;
                case LinkLabel link:   // LinkLabel 派生自 Label，必须排在 Label 前面
                    link.LinkColor = AccentText;
                    link.ActiveLinkColor = AccentHover;
                    link.BackColor = Color.Transparent;
                    link.Font = Body;
                    return;
                case Label lbl:
                    if (lbl.ForeColor == SystemColors.ControlText)
                    {
                        lbl.ForeColor = lbl.Font.Size >= 12 ? TextPrimary : TextSecondary;
                    }
                    lbl.BackColor = Color.Transparent;
                    return;
                case Button btn:
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = Surface;
                    btn.ForeColor = TextPrimary;
                    btn.Font = Body;
                    btn.Cursor = Cursors.Hand;
                    return;
                case CheckBox chk:
                    chk.ForeColor = TextPrimary;
                    chk.BackColor = Color.Transparent;
                    chk.Font = Body;
                    return;
                case RadioButton radio:
                    radio.ForeColor = TextPrimary;
                    radio.BackColor = Color.Transparent;
                    return;
                case TextBox tb:
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    tb.BackColor = Surface;
                    tb.ForeColor = TextPrimary;
                    tb.Font = Body;
                    return;
                case NumericUpDown up:
                    up.BorderStyle = BorderStyle.FixedSingle;
                    up.BackColor = Surface;
                    up.ForeColor = TextPrimary;
                    up.Font = Body;
                    return;
                case ComboBox combo:
                    combo.FlatStyle = FlatStyle.Flat;      // ComboBox 既没有 FlatAppearance 也没有 BorderStyle
                    combo.BackColor = Surface;
                    combo.ForeColor = TextPrimary;
                    combo.Font = Body;
                    return;
                case ListView lv:
                    ApplyListView(lv);
                    return;
                case DataGridView grid:
                    ApplyGrid(grid);
                    return;
                case ProgressBar:
                    // 原生进度条样式改不了，重排时应替换成 UiProgressBar
                    return;
                case Panel p:
                    if (p.BackColor == SystemColors.Control) p.BackColor = Surface;
                    return;
                default:
                    return;
            }
        }

        /// <summary>
        /// ListView 统一外观：无边框白底、小字号、整行选中、不显示网格线。
        /// 交替底色不在这里做，填完 Items 之后另外调 StyleListRows。
        /// </summary>
        /// <param name="lv">要刷样的列表，View 建议设为 Details。</param>
        public static void ApplyListView(ListView lv)
        {
            lv.BorderStyle = BorderStyle.None;
            lv.BackColor = Surface;
            lv.ForeColor = TextPrimary;
            lv.Font = Small;
            lv.FullRowSelect = true;
            lv.GridLines = false;
            lv.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lv.LabelEdit = false;
            // 不自绘：Details 视图的 owner-draw 在勾选框/列宽上有若干未文档化的边界情况，
            // 交替底色和「灰行」用原生 Item 配色实现，稳且够看。
            lv.Invalidate();
        }

        /// <summary>
        /// 重画交替底色与灰行。填充完 Items、或勾选状态变化之后调一次。
        /// 约定：ListViewItem.Tag 以 "info" 开头 = 只读参考项（不可清理），画成灰色。
        /// </summary>
        /// <param name="lv">已填充 Items 的列表。</param>
        public static void StyleListRows(ListView lv)
        {
            StyleListRows(lv, static it => it.Tag is string s && s.StartsWith("info", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 「只读行」的判据由调用方给：有的列表 Tag 上挂的是业务对象，不是字符串。
        /// 传 null 表示不灰任何行，只做交替底色。
        /// </summary>
        /// <param name="lv">已填充 Items 的列表。</param>
        /// <param name="isMuted">判定某行是否只读参考项；可为 null。</param>
        public static void StyleListRows(ListView lv, Func<ListViewItem, bool>? isMuted)
        {
            lv.BeginUpdate();
            try
            {
                for (int i = 0; i < lv.Items.Count; i++)
                {
                    ListViewItem it = lv.Items[i];
                    bool muted = isMuted?.Invoke(it) ?? false;
                    it.BackColor = i % 2 == 1 ? SurfaceMuted : Surface;
                    it.ForeColor = muted ? TextTertiary : TextPrimary;
                }
            }
            finally
            {
                lv.EndUpdate();
            }
        }

        /// <summary>
        /// 列表为空时在列表里显示一行灰色提示，避免出现「白屏」。
        /// 这一行是普通 ListViewItem（不可交互），有真实数据时直接 Items.Clear 覆盖即可。
        /// </summary>
        /// <param name="lv">目标列表。</param>
        /// <param name="text">提示文案，例如「未发现可清理的文件」。</param>
        public static void ShowListEmptyHint(ListView lv, string text)
        {
            lv.Items.Clear();
            // ListViewItem 没有 Enabled；Tag 以 info 开头就是「只读提示行」，StyleListRows 会把它刷成灰色
            var item = new ListViewItem(text) { Tag = "info" };
            lv.Items.Add(item);
            StyleListRows(lv);
        }

        /// <summary>
        /// DataGridView 统一外观：关掉系统视觉样式，32px 表头、24px 行高、交替底色、整行单选。
        /// 只改观感，不碰数据源、列定义和编辑行为。
        /// </summary>
        /// <param name="grid">要刷样的表格；建列之后再调一次效果最稳。</param>
        public static void ApplyGrid(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Border;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = SurfaceMuted;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
            grid.ColumnHeadersDefaultCellStyle.Font = Small;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceMuted;
            grid.ColumnHeadersHeight = 32;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.RowTemplate.Height = 24;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.Font = Small;
            grid.DefaultCellStyle.SelectionBackColor = AccentSoft;
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.AlternatingRowsDefaultCellStyle.BackColor = SurfaceMuted;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = AccentSoft;
            grid.RowHeadersDefaultCellStyle.BackColor = SurfaceMuted;
            grid.RowHeadersDefaultCellStyle.SelectionBackColor = SurfaceMuted;
            grid.RowHeadersWidth = 32;
            grid.AllowUserToResizeRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
        }

        /// <summary>菜单：换掉默认的 Office 蓝渲染器，间距也放宽一点；配色与字体跟主题对齐。</summary>
        /// <param name="menu">下拉菜单，ContextMenuStrip 或 ToolStripDropDown 都可以。</param>
        public static void ApplyMenu(ToolStripDropDown menu)
        {
            menu.Renderer = new FlatMenuRenderer();
            menu.BackColor = Surface;
            menu.Font = Body;
        }

        /// <summary>菜单渲染器：文字、箭头、分隔线全部走调色板，去掉系统默认的蓝色高亮块与渐变。</summary>
        private sealed class FlatMenuRenderer : ToolStripProfessionalRenderer
        {
            /// <summary>配色交给 FlatMenuColors，这里只负责画文字和线。</summary>
            public FlatMenuRenderer() : base(new FlatMenuColors()) { }

            /// <summary>菜单项文字：禁用项用 TextDisabled，保证仍可辨认，不用系统那层带阴影的灰。</summary>
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Enabled ? TextPrimary : TextDisabled;
                e.TextFont = e.Item.Font ?? Body;
                base.OnRenderItemText(e);
            }

            /// <summary>子菜单箭头颜色与说明文字一致。</summary>
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = TextSecondary;
                base.OnRenderArrow(e);
            }

            /// <summary>分隔线：只画一条 1px 中线，左右各留 8px，比系统默认的凹槽细得多。</summary>
            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                using var p = new Pen(Border);
                int y = e.Item.Bounds.Height / 2;
                e.Graphics.DrawLine(p, 8, y, e.Item.Bounds.Right - 8, y);
            }
        }

        /// <summary>
        /// 菜单配色表：把所有「渐变起止」压成同一个纯色，ProfessionalRenderer 就不会画出系统那种上浅下深渐变，
        /// 于是菜单看起来就是一块平整的白面板。
        /// </summary>
        private sealed class FlatMenuColors : ProfessionalColorTable
        {
            /// <summary>菜单项悬停底色（三个值同色，取消高亮渐变）。</summary>
            public override Color MenuItemSelected => SurfaceHover;
            /// <summary>悬停渐变起点，与终点同色即无渐变。</summary>
            public override Color MenuItemSelectedGradientBegin => SurfaceHover;
            /// <summary>悬停渐变终点。</summary>
            public override Color MenuItemSelectedGradientEnd => SurfaceHover;
            /// <summary>悬停项的描边色。</summary>
            public override Color MenuItemBorder => BorderStrong;
            /// <summary>菜单外框描边色。</summary>
            public override Color MenuBorder => Border;
            /// <summary>菜单条按下时的底色起点（保持面色，不做凹陷效果）。</summary>
            public override Color MenuItemPressedGradientBegin => Surface;
            /// <summary>菜单条按下时的底色终点。</summary>
            public override Color MenuItemPressedGradientEnd => Surface;
            /// <summary>菜单条自身底色起点。</summary>
            public override Color MenuStripGradientBegin => Surface;
            /// <summary>菜单条自身底色终点。</summary>
            public override Color MenuStripGradientEnd => Surface;
            /// <summary>下拉面板背景色。</summary>
            public override Color ToolStripDropDownBackground => Surface;
            /// <summary>左侧图标留白区底色起点。</summary>
            public override Color ImageMarginGradientBegin => Surface;
            /// <summary>左侧图标留白区底色中段。</summary>
            public override Color ImageMarginGradientMiddle => Surface;
            /// <summary>左侧图标留白区底色终点。</summary>
            public override Color ImageMarginGradientEnd => Surface;
            /// <summary>分隔线暗色，与亮色取同一个值，避免系统画成双线。</summary>
            public override Color SeparatorDark => Border;
            /// <summary>分隔线亮色。</summary>
            public override Color SeparatorLight => Border;
            /// <summary>ToolStrip 边框色。</summary>
            public override Color ToolStripBorder => Border;
        }

        // ---------- 数字/文案 ----------

        /// <summary>把字节数写成「1.4 GB」这种界面文案。</summary>
        /// <param name="bytes">字节数；负数按 0 处理，最大支持到 TB 一档。</param>
        /// <returns>形如「512 B」「1.4 GB」的文案；大于 100 时不带小数。</returns>
        public static string FormatSize(long bytes)
        {
            if (bytes < 0) bytes = 0;
            if (bytes < 1024) return bytes + " B";
            double v = bytes;
            string[] units = { "KB", "MB", "GB", "TB" };
            int u = -1;
            do
            {
                v /= 1024.0;
                u++;
            }
            while (v >= 1024.0 && u < units.Length - 1);
            return v.ToString(v >= 100 ? "0" : "0.0") + " " + units[u];
        }

        /// <summary>拆成数值与单位，配合 <see cref="MetricView"/> 用。</summary>
        /// <param name="bytes">字节数，同 FormatSize。</param>
        /// <param name="value">数值部分（已去空格），可直接喂给 MetricView 的大数字。</param>
        /// <param name="unit">单位部分，如 B、KB、GB；数值为 0 时仍带单位。</param>
        public static void FormatSizeParts(long bytes, out string value, out string unit)
        {
            string s = FormatSize(bytes);
            int i = s.Length;
            while (i > 0 && (s[i - 1] == ' ' || char.IsLetter(s[i - 1]))) i--;
            value = s.Substring(0, i).Trim();
            unit = s.Substring(i).Trim();
        }

    }
}
