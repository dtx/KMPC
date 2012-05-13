using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KMPC
{
    public static class ControlFunction
    {
        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        static public IntPtr playerHandle;

        static public void Execute(string command)
        {
            command = command.Trim();
            if (!command.Equals(""))
            {
                SetForegroundWindow(playerHandle);
                SendKeys.SendWait(KeyPressInterpreter(command));
            }
        }

        static private string KeyPressInterpreter(string control_str)
        {
            bool contains_modifier = false;
            char[] delimiterChars = { '+' };
            StringBuilder sb = new StringBuilder();

            if (control_str != null)
            {
                if (control_str.Contains("Shift"))
                {
                    sb.Append("+");
                    contains_modifier = true;
                }
                if (control_str.Contains("Control"))
                {
                    sb.Append("^");
                    contains_modifier = true;
                }
                if (control_str.Contains("Alt"))
                {
                    sb.Append("%");
                    contains_modifier = true;
                }
                if (contains_modifier == true)
                {
                    sb.Append("(");
                }

                string[] words = control_str.Split(delimiterChars);

                foreach (string str in words)
                {
                    string s = str.Trim();
                    if (s.Length == 1)
                    {
                        sb.Append(s.ToLower());
                    }
                    else if (s.Equals("Left") || s.Equals("Right") || s.Equals("Up") || s.Equals("Down") || (s.Substring(0, 1).Equals("F")))
                    {
                        sb.Append("{" + s.ToUpper() + "}");
                    }
                    else if (s.Contains("Space"))
                    {
                        sb.Append(" ");
                    }
                    else if (s.Equals("Return"))
                    {
                        sb.Append("{ENTER}");
                    }
                }
                if (contains_modifier == true)
                {
                    sb.Append(")");
                }

                return sb.ToString();
            }

            return null;
        }
    
    }
}
