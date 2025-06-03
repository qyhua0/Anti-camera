using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDisplay
{
    public partial class Form2 : Form
    {
        private readonly System.Windows.Forms.Timer _timer;
        private bool _isHighContrast = true;
        private readonly Random _random = new Random();
        private readonly double _baseFlickerFrequencyHz = 45; // 基础频率，错开常见摄像头帧率
        private readonly bool _useColorFlicker = true; // 使用颜色切换
        private readonly bool _useRandomFlicker = true; // 启用随机频率
        private readonly Color _color1 = Color.Black; // 高对比度颜色1
        private readonly Color _color2 = Color.White; // 高对比度颜色2
        private readonly double _minOpacity = 0.98; // 最小透明度
        private readonly double _maxOpacity = 1.0; // 最大透明度

        public Form2()
        {
            InitializeComponent();

            // 设置窗体属性
            FormBorderStyle = FormBorderStyle.None; // 无边框
            WindowState = FormWindowState.Maximized; // 全屏
            TopMost = true; // 置顶
            BackColor = _color1; // 初始背景色
            Opacity = _maxOpacity; // 初始透明度
            TransparencyKey = BackColor; // 使背景透明，显示底层内容

            // 初始化定时器
            _timer = new System.Windows.Forms.Timer
            {
                Interval = GetRandomInterval() // 初始随机间隔
            };
            _timer.Tick += Timer_Tick;

            // 启动闪烁
            StartFlicker();

            // 注册键盘事件
            KeyDown += MainForm_KeyDown;
        }

        private int GetRandomInterval()
        {
            // 随机频率在 60-90Hz 之间，增加干扰效果
            if (_useRandomFlicker)
            {
                double randomHz = _baseFlickerFrequencyHz + _random.Next(-15, 15);
                return (int)(1000.0 / randomHz / 2); // 每半周期切换（毫秒）
            }
            return (int)(1000.0 / _baseFlickerFrequencyHz / 2);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 更新定时器间隔（随机频率）
            if (_useRandomFlicker)
            {
                _timer.Interval = GetRandomInterval();
            }

            if (_useColorFlicker)
            {
                // 切换高对比度颜色
                BackColor = _isHighContrast ? _color1 : _color2;
                TransparencyKey = BackColor; // 保持透明效果
            }
            else
            {
                // 切换透明度
                Opacity = _isHighContrast ? _minOpacity : _maxOpacity;
            }
            _isHighContrast = !_isHighContrast;

            // 可选：添加随机噪声图案
            Invalidate(); // 触发重绘
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // 绘制随机噪声（可选，增加干扰）
            using (var brush = new SolidBrush(Color.FromArgb(_random.Next(0, 50), _random.Next(0, 50), _random.Next(0, 50))))
            {
                for (int i = 0; i < 10; i++)
                {
                    int x = _random.Next(0, Width);
                    int y = _random.Next(0, Height);
                    e.Graphics.FillRectangle(brush, x, y, 50, 50); // 绘制随机小方块
                }
            }
        }


        private void StartFlicker()
        {
            _timer.Start();
        }

        private void StopFlicker()
        {
            _timer.Stop();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // 按 Esc 键退出程序
            if (e.KeyCode == Keys.Escape)
            {
                StopFlicker();
                Close();
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
