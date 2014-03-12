using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace JetBrains.DotPeek.Plugins.Console.Controls
{
    public class ConsoleControl
        : RichTextBox, INotifyPropertyChanged
    {
        public delegate void CommandEnteredEventHandler(object sender, CommandEnteredEventArgs e);
        public event CommandEnteredEventHandler CommandEntered;

        public delegate void AutoCompletionRequestedEventHandler(object sender, CommandEnteredEventArgs e);
        public event AutoCompletionRequestedEventHandler AutoCompletionRequested;

        public List<string> History { get; private set; }
        private readonly StringBuilder _lineBuffer;
        private readonly StringBuilder _echoBuffer;
        private int _positionInHistory;

        public ConsoleControl()
        {
            History = new List<string>();
            _lineBuffer = new StringBuilder(4096);
            _echoBuffer = new StringBuilder(4096);
            _positionInHistory = 0;

            IsUndoEnabled = false;
            AcceptsReturn = false;
            AcceptsTab = false;
            EnableInput(true);

            DataObject.AddPastingHandler(this, OnPaste);

            TextChanged += (s, e) => ScrollToEnd();
        }

        public override void EndInit()
        {
            base.EndInit();

            ResetColor();
        }

        public static readonly DependencyProperty CurrentForegroundProperty = DependencyProperty.Register("CurrentForeground", typeof(Brush), typeof(RichTextBox));

        public Brush CurrentForeground
        {
            get { return (Brush)base.GetValue(CurrentForegroundProperty); }
            set { base.SetValue(CurrentForegroundProperty, value); }
        }

        public void Write(string value)
        {
            EnableInput(false);
            var tr = new TextRange(Document.ContentEnd, Document.ContentEnd);
            tr.Text = value;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, CurrentForeground);
            EnableInput(true);
        }

        public void WriteLine()
        {
            WriteLine("");
        }

        public void WriteLine(string value)
        {
            EnableInput(false);
            Write(value + "\r");
            EnableInput(true);
        }

        public void Clear()
        {
            Document.Blocks.Clear();
        }

        public void ResetColor()
        {
            CurrentForeground = Foreground;
        }

        public void MoveCaretToEnd()
        {
            CaretPosition = Document.ContentEnd;
        }

        private bool _isInputEnabled;
        protected void EnableInput(bool enable)
        {
            _isInputEnabled = enable;
            IsReadOnly = !enable;

            if (_isInputEnabled && _echoBuffer.Length > 0)
            {
                var echo = _echoBuffer.ToString();
                _echoBuffer.Clear();
                Write(echo);
            }
        }

        protected virtual void OnCommandEntered(CommandEnteredEventArgs e)
        {
            if (CommandEntered != null)
            {
                CommandEntered(this, e);
            }
        }
        
        protected virtual void OnAutoCompletionRequested(CommandEnteredEventArgs e)
        {
            if (AutoCompletionRequested != null)
            {
                AutoCompletionRequested(this, e);
            }
        }

        protected void InvokeCommandEntered()
        {
            var command = _lineBuffer.ToString().Replace("\r", "").Replace("\n", "");
            _lineBuffer.Clear();

            History.Add(command);
            _positionInHistory = History.Count - 1;
            
            if (_isInputEnabled) WriteLine();

            OnCommandEntered(new CommandEnteredEventArgs { Command = command });
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            // Make sure we are typing at the end
            MoveCaretToEnd();

            // Forcing an enter? // TODO: Make this an option
            if (e.Key == Key.Enter && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftShift)))
            {
                ProcessTextInput("\r");
                WriteLine();
                e.Handled = true;
            }

            // If input has not yet been discarded, test the key for special inputs.
            // ENTER => flush buffer to history
            // TAB => autocompletion requested // TODO: make this an option
            // UP => back in history // TODO: make this an option
            // DOWN => forward in history // TODO: make this an option
            if (!e.Handled)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        InvokeCommandEntered();
                        break;
                    case Key.Tab:
                        var partialCommand = _isInputEnabled
                            ? _lineBuffer.ToString().Replace("\r", "").Replace("\n", "")
                            : _echoBuffer.ToString().Replace("\r", "").Replace("\n", "");

                        OnAutoCompletionRequested(new CommandEnteredEventArgs { Command = partialCommand });
                        break;
                    case Key.Up:
                        ProcessTextInput(History[_positionInHistory--], true);
                        if (_positionInHistory < 0)
                        {
                            _positionInHistory = 0;
                        }
                        e.Handled = true;
                        break;
                    case Key.Down: // TODO: fix
                        _positionInHistory++;
                        if (_positionInHistory >= History.Count)
                        {
                            _positionInHistory = History.Count - 1;
                            ProcessTextInput("", true);
                        }
                        else
                        {
                            ProcessTextInput(History[_positionInHistory], true);
                        }
                        e.Handled = true;
                        break;
                    case Key.Back:
                        if (_lineBuffer.Length > 0)
                        {
                            _lineBuffer.Remove(_lineBuffer.Length - 1, 1);
                        }
                        else
                        {
                            e.Handled = true;
                        }
                        break;
                    case Key.Space:
                        ProcessTextInput(" ", false);
                        break;
                }
            }

            base.OnPreviewKeyDown(e);
		}

        public void AutoCompletionFinished(string autoCompletedLine, bool replaceLine)
        {
            ProcessTextInput(autoCompletedLine, replaceLine);
        }

        protected void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.SourceDataObject.GetDataPresent(DataFormats.Text, true))
            {
                var text = e.SourceDataObject.GetData(DataFormats.Text) as string;
                if (text != null)
                {
                    text = text.Replace("\r\n", "\r");

                    ProcessTextInput(text);
                    Write(text);
                }
            }
            e.CancelCommand();
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            e.Handled = ProcessTextInput(e.Text);

            base.OnTextInput(e);
        }

        private bool ProcessTextInput(string text, bool replaceBuffers = false)
        {
            // Replace buffers?
            if (replaceBuffers)
            {
                var bufferLength = _lineBuffer.Length;
                _lineBuffer.Clear();
                if (_echoBuffer.Length >= bufferLength)
                {
                    _echoBuffer.Remove(_echoBuffer.Length - bufferLength, bufferLength);
                }
                var tr = new TextRange(CaretPosition.GetPositionAtOffset(-1 * bufferLength), CaretPosition);
                if (tr.Text.StartsWith("\r"))
                {
                    tr.Text = "\r" + text;
                } 
                else if (tr.Text.StartsWith(" "))
                {
                    tr.Text = " " + text;
                }
                else
                {
                    tr.Text = text;
                }
            }

            // Append to buffer
            _lineBuffer.Append(text);

            // Is input enabled?
            if (!_isInputEnabled)
            {
                _echoBuffer.Append(text);
                return true;
            }
            return false;
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
