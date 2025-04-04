using System;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace kalkulator_zegar
{
    public partial class Form1 : Form
    {
        private System.Timers.Timer timer;
        private bool isAnalog = false;
        private Color clockColor = Color.Black;

        private Button btnToggleMode;
        private Button btnChangeColor;

        private Button[] calcButtons;
        private TextBox calcDisplay;
        private string calcInput = "";

        public Form1()
        {
            InitializeComponent();
            InitializeClock();
            InitializeButtons();
            InitializeCalculator();
            SetButtonPositions();
        }

        private void InitializeClock()
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += TimerElapsed;
            timer.Start();
            this.DoubleBuffered = true;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate { this.Invalidate(); });
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(this.BackColor);

            if (isAnalog)
                DrawAnalogClock(g);
            else
                DrawDigitalClock(g);
        }

        private void DrawDigitalClock(Graphics g)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            using (Font font = new Font("Arial", 24))
            using (Brush brush = new SolidBrush(clockColor))
            {
                g.DrawString(time, font, brush, new PointF(this.ClientSize.Width - 150, 20));
            }
        }

        private void DrawAnalogClock(Graphics g)
        {
            int centerX = this.ClientSize.Width - 75;
            int centerY = 75;
            int radius = 50;
            DateTime now = DateTime.Now;

            g.DrawEllipse(new Pen(clockColor, 3), centerX - radius, centerY - radius, radius * 2, radius * 2);
            DrawClockHand(g, now.Hour % 12 * 30 + now.Minute / 2, radius * 0.5f, 6, centerX, centerY);
            DrawClockHand(g, now.Minute * 6, radius * 0.7f, 4, centerX, centerY);
            DrawClockHand(g, now.Second * 6, radius * 0.9f, 2, centerX, centerY);
        }

        private void DrawClockHand(Graphics g, float angle, float length, int thickness, int centerX, int centerY)
        {
            double rad = Math.PI * angle / 180;
            int x = centerX + (int)(length * Math.Sin(rad));
            int y = centerY - (int)(length * Math.Cos(rad));
            g.DrawLine(new Pen(clockColor, thickness), centerX, centerY, x, y);
        }

        private void btnToggleMode_Click(object sender, EventArgs e)
        {
            isAnalog = !isAnalog;
            this.Invalidate();
        }

        private void btnChangeColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    clockColor = colorDialog.Color;
                    this.Invalidate();
                }
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            SetButtonPositions();
        }

        private void SetButtonPositions()
        {
            int clockX = this.ClientSize.Width - 100;
            int clockY = 100;

            btnToggleMode.Location = new Point(clockX - 25, clockY + 50);
            btnChangeColor.Location = new Point(clockX - 25, clockY + 90);

            for (int i = 0; i < calcButtons.Length; i++)
            {
                int row = i / 4;
                int col = i % 4;
                calcButtons[i].Location = new Point(20 + col * 60, 180 + row * 60);
            }
        }

        private void InitializeButtons()
        {
            btnToggleMode = new Button
            {
                Text = "Przełącz tryb",
                Size = new Size(120, 30)
            };
            btnToggleMode.Click += new EventHandler(this.btnToggleMode_Click);
            this.Controls.Add(btnToggleMode);

            btnChangeColor = new Button
            {
                Text = "Zmień kolor",
                Size = new Size(120, 30)
            };
            btnChangeColor.Click += new EventHandler(this.btnChangeColor_Click);
            this.Controls.Add(btnChangeColor);
        }
        //*************** KALKULATOR ***************************
        private void InitializeCalculator()
        {
            calcDisplay = new TextBox
            {
                Size = new Size(250, 40),
                Location = new Point(20, 140),
                Text = "",
                ReadOnly = true,
                Font = new Font("Arial", 16)
            };
            this.Controls.Add(calcDisplay);

            string[] buttonLabels = new string[]
            {
                "x^2", "CE", "C", "sqrt",
                "7", "8", "9", "/",
                "4", "5", "6", "*",
                "1", "2", "3", "-",
                "0", ",", "=", "+"
            };

            calcButtons = new Button[20];
            for (int i = 0; i < buttonLabels.Length; i++)
            {
                calcButtons[i] = new Button
                {
                    Text = buttonLabels[i],
                    Size = new Size(50, 50)
                };
                calcButtons[i].Click += new EventHandler(CalcButton_Click);
                this.Controls.Add(calcButtons[i]);
            }

            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
        }

        private void CalcButton_Click(object sender, EventArgs e)
        {
            string buttonText = ((Button)sender).Text;
            HandleCalcInput(buttonText);
        }

        private void HandleCalcInput(string input)
        {
            if (input == "=")
            {
                try
                {
                    calcInput = calcInput.Replace(',', '.');
                    calcInput = new System.Data.DataTable().Compute(calcInput, null).ToString();
                    calcInput = calcInput.Replace('.', ',');
                    calcDisplay.Text = calcInput;
                }
                catch
                {
                    calcDisplay.Text = "Błąd!";
                    calcInput = "";
                }
            }
            else if (input == "CE")
            {
                calcInput = "";
                calcDisplay.Text = "";
            }
            else if(input == "C")
            {
                if (calcInput.Length > 0)
                {
                    calcInput = calcInput.Substring(0, calcInput.Length - 1);
                    calcDisplay.Text = calcInput;
                }
            }
            else if (input == "x^2")
            {
                if (double.TryParse(calcInput, System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("pl-PL"), out double num))
                {
                
                    calcInput = (num*num).ToString("G", new System.Globalization.CultureInfo("pl-PL"));
                    calcDisplay.Text = calcInput;
                }
                else
                {
                    calcDisplay.Text = "Błąd!";
                }
            }
            else if (input == "sqrt")
            {
                if (double.TryParse(calcInput, System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("pl-PL"), out double num))
                {
                    calcInput = Math.Sqrt(num).ToString("G", new System.Globalization.CultureInfo("pl-PL"));
                    calcDisplay.Text = calcInput;
                }
                else
                {
                    calcDisplay.Text = "Błąd!";
                }
            }

            else
            {
                calcInput += input;
                calcDisplay.Text = calcInput;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift && e.KeyCode == Keys.D8)
            {
                HandleCalcInput("*");
                return;
            }
            else if (e.Shift && e.KeyCode == Keys.D9)
            {
                HandleCalcInput("(");
                return;
            }
            else if (e.Shift && e.KeyCode == Keys.D0)
            {
                HandleCalcInput(")");
                return;
            }

            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            {
                char keyChar = (char)(e.KeyCode - Keys.D0 + '0');
                HandleCalcInput(keyChar.ToString());
            }
            else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
            {
                char keyChar = (char)(e.KeyCode - Keys.NumPad0 + '0');
                HandleCalcInput(keyChar.ToString());
            }
            else if (e.KeyCode == Keys.Oemcomma || e.KeyCode == Keys.Decimal)
            {
                if (!calcInput.Contains(","))
                {
                    
                    HandleCalcInput(",");
                }
            }
            else if (e.KeyCode == Keys.Add)
            {
                HandleCalcInput("+");
            }
            else if (e.KeyCode == Keys.Subtract)
            {
                HandleCalcInput("-");
            }
            else if (e.KeyCode == Keys.Multiply)
            {
                HandleCalcInput("*");
            }
            else if (e.KeyCode == Keys.Divide)
            {
                HandleCalcInput("/");
            }
            else if (e.KeyCode == Keys.Enter)
            {
                HandleCalcInput("=");
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (calcInput.Length > 0)
                {
                    calcInput = calcInput.Substring(0, calcInput.Length - 1);
                    calcDisplay.Text = calcInput;
                }
            }

           
        }
    }
}
