using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TuringMachine.Models;

namespace TuringMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Global Variables;
        #region
        private bool _compiled = false;
        private object _lock = new object();
        private int _head = 0;
        private int _codeLine = 0;
        private int _prevCodeLine = 0;
        private List<int> _codeLines = new List<int>();
        private List<TuringSyntaxError> _errors = new List<TuringSyntaxError>();
        ObservableCollection<TuringSyntaxError> _errorsItemSource = new ObservableCollection<TuringSyntaxError>();
        private double _editorHeight = 0;
        private ObservableCollection<string> _strip = new ObservableCollection<string>();
        private List<string> _lines = new List<string>();
        private string[] _cmd = { };
        private string[] _prevCmd = { };
        bool _halted = false;
        private DispatcherTimer _timer = new DispatcherTimer();

        public double ScreenHeight
        {
            get { return System.Windows.SystemParameters.PrimaryScreenHeight; }
        }

        public double ScreenWidth
        {
            get { return System.Windows.SystemParameters.PrimaryScreenWidth; }
        }

        public double MinimizedHeight { get; set; }

        public double MinimizedWidth { get; set; }
        #endregion

        // Window Initialize;
        public MainWindow()
        {
            InitializeComponent();
        }

        // Finde Next Command;
        private void FindNextCommand(string[] cmd)
        {
            // Check If Command Is Last;
            if (cmd[4].Contains("halt"))
            {
                _halted = true;
            }
            else
            {
                _cmd = new string[] { };
                for (int x = 0; x < _lines.Count; x++)
                {
                    string[] newCmd = _lines[x].Split(' ');

                    // Find Next Command;
                    if (cmd[4] == newCmd[0] && (newCmd[1] == _strip[_head] || newCmd[1] == "*" || (newCmd[1] == "_" && _strip[_head] == " ")))
                    {
                        if (_cmd.Count() > 0)
                        {
                            if (newCmd[1] == "*" && (_cmd[1] != "_" || _strip[_head] != " ") && _cmd[1] != _strip[_head])
                            {
                                _cmd = newCmd;
                                _codeLine = _codeLines[x];
                            }
                        }
                        else
                        {
                            if (newCmd[1] == "*")
                            {
                                _cmd = newCmd;
                                _codeLine = _codeLines[x];
                            }
                        }

                        if (newCmd[1] == _strip[_head] || newCmd[1] == "_" && _strip[_head] == " ")
                        {
                            _cmd = newCmd;
                            _codeLine = _codeLines[x];
                        }
                    }
                }

                if (_cmd.Count() == 0)
                {
                    _errors.Add(new TuringSyntaxError
                    {
                        Line = string.Format("State: {0}; Condition: ({1}) On Line: {2}", cmd[4], _strip[_head], _codeLine.ToString()),
                        ErrorMessage = "Error Type: State Not Found",
                    });
                }
            }
        }

        // Show Errors;
        private void ShowErrors()
        {
            // Fill Errors ItemControl (ErrorsList);
            foreach (var error in _errors)
            {
                _errorsItemSource.Add(error);
            }
            ErrorsList.ItemsSource = _errorsItemSource; // Show Errors List;

            // Animate RichTextBox (Editor);
            Editor.Height = _editorHeight;
            Editor.VerticalAlignment = VerticalAlignment.Top;
            DoubleAnimation animation = new DoubleAnimation(_editorHeight / 3, new Duration(TimeSpan.FromSeconds(0.3)));
            animation.Completed += (_sender, _e) =>
            {
                ErrorsScrollBar.Visibility = Visibility.Visible;
                _errors.Clear();
            };
            Editor.BeginAnimation(HeightProperty, animation);
        }

        // Turing Machine Compiler;
        private void Compile()
        {
            // Parse Condition Chars From Strip;
            _strip.Clear();
            if (Strip.Text.Length == 0)
            {
                _strip.Add(" ");
            }
            else
            {
                for (int x = 0; x < Strip.Text.Length; x++)
                {
                    _strip.Add(Strip.Text[x].ToString());
                }
            }

            TextRange textRange = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            List<string> lines = new List<string>();

            // Read Lines From Editor;
            using (StringReader strReader = new StringReader(textRange.Text))
            {
                string line;
                _lines = new List<string>();
                while ((line = strReader.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        lines.Add(line);
                    }
                }
            }

            // Parse Commands From Source Code;
            int codeLine = 0;
            foreach (var line in lines)
            {
                codeLine++;

                if (line.IndexOf(";") != -1)
                {
                    string comment = line.Substring(line.IndexOf(";"));
                    string code = line.Remove(line.IndexOf(";"));

                    if (string.IsNullOrEmpty(code))
                    {
                        continue;
                    }
                    else
                    {
                        _lines.Add(code);
                        _codeLines.Add(codeLine);
                    }
                }
                else
                {
                    string code = line;

                    if (string.IsNullOrEmpty(code))
                    {
                        continue;
                    }
                    else
                    {
                        _lines.Add(code);
                        _codeLines.Add(codeLine);
                    }
                }
            }

            if (_lines.Count > 0)
            {
                foreach (var line in _lines)
                {
                    _cmd = line.Split(' ');
                    if (_cmd.Count() == 5)
                    {
                        if ((_cmd[0] == "0" && (_cmd[1] == _strip[_head] || _cmd[1] == "*") || (_cmd[1] == "_" && _strip[_head] == " ")))
                        {
                            _compiled = true;
                            _errors.Clear();
                            break;
                        }
                        else
                        {
                            _errors.Add(new TuringSyntaxError
                            {
                                ErrorMessage = "Program Is Empty",
                            });
                            _cmd = new string[] { };
                        }
                    }
                    else
                    {
                        _errors.Add(new TuringSyntaxError
                        {
                            ErrorMessage = "Entry Command Not Found",
                        });
                        _cmd = new string[] { };
                    }
                }
            }
            else
            {
                _errors.Add(new TuringSyntaxError
                {
                    ErrorMessage = "Entry Command Not Found",
                });
                _cmd = new string[] { };
            }

            textRange.Text = string.Join("\n", lines);
        }

        // Execute Command;
        private void ExecuteCommand(string[] command)
        {
            string conditionalSymbol = command[1];
            string newSymbol = command[2];
            string direction = command[3];

            // Change Symbol;
            #region
            if (newSymbol != "*" && conditionalSymbol.Length == 1 && newSymbol.Length == 1)
            {
                if (newSymbol == "_")
                {
                    _strip[_head] = " ";
                }
                else
                {
                    _strip[_head] = newSymbol;
                }
            }
            else if (conditionalSymbol.Length != 1 || newSymbol.Length != 1)
            {
                _errors.Add(new TuringSyntaxError
                {
                    Line = _codeLine.ToString(),
                    ErrorMessage = "Condition or New Symbol`s Length Should Be 1",
                });
            }
            #endregion

            // Check Direction;
            #region
            if (direction == "l") // If Direction Is Left;
            {
                if (_head == 0)
                {
                    List<string> newString = new List<string> { " " };
                    newString.AddRange(_strip);

                    _strip.Clear();
                    newString.ForEach(x =>
                    {
                        _strip.Add(x);
                    });
                }
                else
                {
                    _head--;
                }
            }
            else if (direction == "r") // If Direction Is Right;
            {
                if (_head >= _strip.Count - 1)
                {
                    _strip.Add(" ");
                }
                _head++;
            }
            else if (direction != "l" && direction != "r" && direction != "*") // Check Direction Syntax;
            {
                _errors.Add(new TuringSyntaxError
                {
                    Line = _codeLine.ToString(),
                    ErrorMessage = "Direction Should be 'l','r' or'*'",
                });
            }
            #endregion


            _prevCmd = _cmd;
            _prevCodeLine = _codeLine;
            FindNextCommand(_cmd);
        }

        // Get String Width;
        private Size StringSize(object control, string chars)
        {
            FormattedText formattedText = null;
            if (control is TextBox)
            {
                TextBox textControl = (TextBox)control;
                formattedText = new FormattedText(
                chars,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(textControl.FontFamily, textControl.FontStyle, textControl.FontWeight, textControl.FontStretch),
                textControl.FontSize,
                Brushes.Black);
            }
            else if (control is RichTextBox)
            {
                RichTextBox textControl = (RichTextBox)control;
                formattedText = new FormattedText(
                chars,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(textControl.FontFamily, textControl.FontStyle, textControl.FontWeight, textControl.FontStretch),
                textControl.FontSize,
                Brushes.Black);
            }

            return new Size(formattedText.Width, formattedText.Height);
        }

        // Generate Random String For Strip;
        private void RandomStrip()
        {
            Strip.Clear();
            Random random = new Random();
            int stringLength = random.Next(10, 20);
            string currentSymbol = string.Empty;
            for (int x = 0; x < stringLength; x++)
            {
                currentSymbol = random.Next(2).ToString();
                Strip.Text += currentSymbol;
            }

            Strip.Text = "1" + Strip.Text;
        }

        // Event Methods;
        #region

        // Window Loaded (Setting Editor`s Height For Animation);
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RandomStrip();
            Editor.Height = Editor.ActualHeight;
            _editorHeight = Editor.ActualHeight;
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += (s, eArgs) =>
            {
                ExecuteCommand(_cmd);
                DoubleAnimation headAnimation = new DoubleAnimation
                {
                    Duration = TimeSpan.FromMilliseconds(100),
                };

                if (_errors.Count == 0)
                {
                    for (int x = 0; x < StripSymbols.Items.Count; x++)
                    {
                        DependencyObject content = StripSymbols.ItemContainerGenerator.ContainerFromIndex(x);
                        for (int y = 0; y < VisualTreeHelper.GetChildrenCount(content); y++)
                        {
                            Label label = (Label)VisualTreeHelper.GetChild(content, y);

                            if (label.Content == _strip[_head] || _strip[_head] == " ")
                            {
                                Point labelPosition = label.TransformToAncestor(StripSymbols).Transform(new Point(0, 0));
                                headAnimation.To = labelPosition.X;
                            }
                        }
                    }


                    TranslateTransform headTransform = new TranslateTransform
                    {
                        X = Head.RenderTransform.Value.OffsetX,
                    };
                    Head.RenderTransform = headTransform;
                    headTransform.BeginAnimation(TranslateTransform.XProperty, headAnimation);
                    CurrentState.Content = _cmd[0];
                    CurrentCondition.Content = _strip[_head];

                    if (_halted)
                    {
                        _timer.Stop();
                    }
                }
                else
                {
                    _timer.Stop();
                    ShowErrors();
                }
            };
        }

        // Window State Changed;
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                Maximize(MaximizeButton, null);
            }
        }

        // Window DragMove;
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && ActualWidth != ScreenWidth && ActualHeight != ScreenHeight)
            {
                this.DragMove();
            }
        }

        // Application Close Method;
        private void Close(object sender, RoutedEventArgs e)
        {
            DoubleAnimation closeAnimation = new DoubleAnimation();
            closeAnimation.To = 0;
            closeAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            closeAnimation.Completed += (s, eArgs) => this.Close();
            this.BeginAnimation(UIElement.OpacityProperty, closeAnimation);
        }

        // Window Maximize Method; (Change Window Height and Width to Screen or Back to Normal Size);
        private void Maximize(object sender, RoutedEventArgs e)
        {
            Button _this = (Button)sender;

            // Check Window State;
            if (this.Height == ScreenHeight) // If Window Is Maximized
            {
                DoubleAnimation maximizeAnimation = new DoubleAnimation();
                maximizeAnimation.To = 0;
                maximizeAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.3));
                maximizeAnimation.Completed += (s, eArgs) =>
                {
                    this.Height = MinimizedHeight;
                    this.Width = MinimizedWidth;
                    this.Left = (ScreenWidth - MinimizedWidth) / 2;
                    this.Top = (ScreenHeight - MinimizedHeight) / 2;
                    this.BeginAnimation(FrameworkElement.OpacityProperty, null);
                    this.Opacity = 1;
                    _this.Content = "\uf096";
                };
                this.BeginAnimation(FrameworkElement.OpacityProperty, null);
                this.BeginAnimation(FrameworkElement.OpacityProperty, maximizeAnimation);
            }
            else // If Window Is Normal;
            {
                MinimizedHeight = this.ActualHeight;
                MinimizedWidth = this.ActualWidth;
                DoubleAnimation maximizeAnimation = new DoubleAnimation();
                maximizeAnimation.To = 0;
                maximizeAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.3));
                maximizeAnimation.Completed += (s, eArgs) =>
                {
                    this.Height = ScreenHeight;
                    this.Width = ScreenWidth;
                    this.Left = 0;
                    this.Top = 0;
                    this.BeginAnimation(FrameworkElement.OpacityProperty, null);
                    this.Opacity = 1;
                    _this.Content = "\uf24d";

                };
                this.BeginAnimation(FrameworkElement.OpacityProperty, maximizeAnimation);
            }
        }

        // Window Minimze Method;
        private void Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Run Program;
        private void Run_Click(object sender, RoutedEventArgs e)
        {
            if (_compiled)
            {
                _timer.Start();
            }
            else
            {
                Compile();
                Run.IsEnabled = false;
                Editor.IsReadOnly = true;
                _editorHeight = Editor.ActualHeight;

                if (_lines.Count > 0 && _errors.Count == 0)
                {
                    Strip.Visibility = Visibility.Collapsed;
                    StripSymbols.Visibility = Visibility.Visible;
                    StripSymbols.ItemsSource = _strip;
                    _timer.Start();
                }
                else
                {
                    ShowErrors();
                }
            }
        }

        // Reset Necessary Variables And Control Properties;
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            RandomStrip();
            _timer.Stop();
            CurrentState.Content = "0";
            CurrentCondition.Content = "1";
            _strip.Clear();
            _cmd = new string[] { };
            _head = 0;
            _codeLine = 0;
            _errorsItemSource.Clear();
            _errors.Clear();
            ErrorsScrollBar.Visibility = Visibility.Collapsed;
            Editor.Document.Blocks.Clear();
            Editor.IsReadOnly = false;
            Run.IsEnabled = true;
            _halted = false;
            _compiled = false;
            Strip.Visibility = Visibility.Visible;
            StripSymbols.Visibility = Visibility.Collapsed;

            DoubleAnimation headAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(100),
            };
            TranslateTransform headTransform = new TranslateTransform();
            Head.RenderTransform = headTransform;
            headTransform.BeginAnimation(TranslateTransform.XProperty, headAnimation);

            DoubleAnimation animation = new DoubleAnimation(_editorHeight, new Duration(TimeSpan.FromSeconds(0.3)));
            Editor.BeginAnimation(HeightProperty, animation);
        }

        // Run Next Command And Stop;
        private void Debug_Click(object sender, RoutedEventArgs e)
        {
            Strip.Visibility = Visibility.Collapsed;
            StripSymbols.Visibility = Visibility.Visible;
            StripSymbols.ItemsSource = _strip;

            if (_compiled)
            {
                if (_halted)
                {
                    MessageBox.Show("Completed");
                    return;
                }
                else
                {
                    ExecuteCommand(_cmd);
                    DoubleAnimation headAnimation = new DoubleAnimation
                    {
                        Duration = TimeSpan.FromMilliseconds(100),
                    };

                    if (_errors.Count == 0)
                    {
                        for (int x = 0; x < StripSymbols.Items.Count; x++)
                        {
                            DependencyObject content = StripSymbols.ItemContainerGenerator.ContainerFromIndex(x);
                            for (int y = 0; y < VisualTreeHelper.GetChildrenCount(content); y++)
                            {
                                Label label = (Label)VisualTreeHelper.GetChild(content, y);

                                if (label.Content == _strip[_head] || _strip[_head] == " ")
                                {
                                    Point labelPosition = label.TransformToAncestor(StripSymbols).Transform(new Point(0, 0));
                                    headAnimation.To = labelPosition.X;
                                }
                            }
                        }

                        
                        TranslateTransform headTransform = new TranslateTransform
                        {
                            X= Head.RenderTransform.Value.OffsetX,
                        };
                        Head.RenderTransform = headTransform;
                        headTransform.BeginAnimation(TranslateTransform.XProperty, headAnimation);
                        CurrentState.Content = _cmd[0];
                        CurrentCondition.Content = _strip[_head];
                    }
                    else
                    {
                        ShowErrors();
                        return;
                    }
                }

            }
            else
            {
                Compile();

                if (_errors.Count > 0)
                {
                    ShowErrors();
                }
            }

        }

        // Open Web Page About Syntax;
        private void Syntax(object sender, RoutedEventArgs e)
        {
            Process.Start("http://morphett.info/turing/turing.html");
        }
        #endregion
    }
}
