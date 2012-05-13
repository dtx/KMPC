using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace KMPC
{
    public static class ControlFunction
    {
        static public readonly string _shift = "Shift";
        static public readonly string _control = "Control";
        static public readonly string _alt = "Alt";
        static public readonly string _left = "Left";
        static public readonly string _right = "Right";
        static public readonly string _up = "Up";
        static public readonly string _down = "Down";
        static public readonly string _space = "Space";
        static public readonly string _return = "Return";
        static public readonly string _plus = "+";
        static public readonly string _caret = "^";
        static public readonly string _percent = "%";
        static public readonly string _f = "F";
        static public readonly string _leftbracket = "(";
        static public readonly string _rightbracket = ")";
        static public readonly string _leftbrace = "{";
        static public readonly string _rightbrace = "}";
        static public readonly string _enter = "{ENTER}";

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        static public IntPtr playerHandle;

        static public bool Execute(string command)
        {
            if (command != null)
            {
                command = command.Trim();
                if (!command.Equals(string.Empty))
                {
                    SetForegroundWindow(playerHandle);
                    SendKeys.SendWait(KeyPressInterpreter(command));
                    return true;
                }
            }
            return false;
        }

        static private string KeyPressInterpreter(string control_str)
        {
            bool contains_modifier = false;
            char[] delimiterChars = { '+' };
            StringBuilder sb = new StringBuilder();

            if (control_str != null)
            {
                if (control_str.Contains(_shift))
                {
                    sb.Append(_plus);
                    contains_modifier = true;
                }
                if (control_str.Contains(_control))
                {
                    sb.Append(_caret);
                    contains_modifier = true;
                }
                if (control_str.Contains(_alt))
                {
                    sb.Append(_percent);
                    contains_modifier = true;
                }
                if (contains_modifier == true)
                {
                    sb.Append(_leftbracket);
                }

                string[] words = control_str.Split(delimiterChars);

                foreach (string str in words)
                {
                    string s = str.Trim();
                    if (s.Length == 1)
                    {
                        sb.Append(s.ToLower());
                    }
                    else if (s.Equals(_left) || s.Equals(_right) || s.Equals(_up) || s.Equals(_down) || (s.Substring(0, 1).Equals(_f)))
                    {
                        sb.Append(_leftbrace + s.ToUpper() + _rightbrace);
                    }
                    else if (s.Contains(_space))
                    {
                        sb.Append(" ");
                    }
                    else if (s.Equals(_return))
                    {
                        sb.Append(_enter);
                    }
                }
                if (contains_modifier == true)
                {
                    sb.Append(_rightbracket);
                }

                return sb.ToString();
            }

            return null;
        }
    
    }
}
