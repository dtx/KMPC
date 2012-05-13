using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO;

namespace KMPC
{
    public partial class Form1 : Form
    {
        //public event DisplayImageEventHandler ImageReadyForDisplay;
        private bool _capturing;
        private Image _finderHome;
        private Image _finderGone;
        private Cursor _cursorDefault;
        private Cursor _cursorFinder;
        private IntPtr _hPreviousWindow;
       
        public Form1()
        {
            InitializeComponent();

            _cursorDefault = Cursor.Current;
            _cursorFinder = EmbeddedResources.LoadCursor(EmbeddedResources.Finder);
            _finderHome = EmbeddedResources.LoadImage(EmbeddedResources.FinderHome);
            _finderGone = EmbeddedResources.LoadImage(EmbeddedResources.FinderGone);

            pictureBox1.Image = _finderHome;
            pictureBox1.MouseDown += new MouseEventHandler(OnFinderToolMouseDown);
            button_ok.Click += new EventHandler(OnButtonOKClicked);
            button_cancel.Click += new EventHandler(OnButtonCancelClicked);
            textBox_play.KeyDown += new KeyEventHandler(textBox_play_KeyDown);
            textBox_stop.KeyDown += new KeyEventHandler(textBox_stop_KeyDown);
            textBox_full.KeyDown += new KeyEventHandler(textBox_full_KeyDown);
            textBox_mute.KeyDown += new KeyEventHandler(textBox_mute_KeyDown);
            textBox_fwd.KeyDown += new KeyEventHandler(textBox_fwd_KeyDown);
            textBox_bwd.KeyDown += new KeyEventHandler(textBox_bwd_KeyDown);
            textBox_vup.KeyDown += new KeyEventHandler(textBox_vup_KeyDown);
            textBox_vdown.KeyDown += new KeyEventHandler(textBox_vdown_KeyDown);
            textBox_class_name.TextChanged += new EventHandler(OnTextBoxHandleTextChanged);

            this.AcceptButton = button_ok;
            this.CancelButton = button_cancel;
        
        }

        void textBox_vdown_KeyDown(object sender, KeyEventArgs e)
        {
            textBox_KeyDown(sender, e, textBox_vdown);
        }

        void textBox_vup_KeyDown(object sender, KeyEventArgs e)
        {
            textBox_KeyDown(sender, e, textBox_vup);
        }

        void textBox_bwd_KeyDown(object sender, KeyEventArgs e)
        {
            textBox_KeyDown(sender, e, textBox_bwd);
        }

        void textBox_fwd_KeyDown(object sender, KeyEventArgs e)
        {
            textBox_KeyDown(sender, e, textBox_fwd);
        }

        void textBox_mute_KeyDown(object sender, KeyEventArgs e)
        {
            textBox_KeyDown(sender, e, textBox_mute);
        }

        void textBox_full_KeyDown(object sender, KeyEventArgs e)
        {
            textBox_KeyDown(sender, e, textBox_full);
        }

        void textBox_stop_KeyDown(object sender, KeyEventArgs e)
        {
            textBox_KeyDown(sender, e, textBox_stop);
        }

        void textBox_play_KeyDown(object sender, KeyEventArgs e)
        {
            textBox_KeyDown(sender, e, textBox_play);
        }

        void textBox_KeyDown(object sender, KeyEventArgs e, TextBox textBox)
        {
            this.KeyPreview = true;
           
            //System.Diagnostics.Debug.WriteLine(e.Modifiers.ToString() + " + " + e.KeyData.ToString());

		    StringBuilder sb = new StringBuilder();
            if (e.Modifiers != Keys.None)
			    sb.Append(e.Modifiers.ToString().Replace(",", " +"));

		    string key = null;
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Return || e.KeyCode == Keys.Space || (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z) || (e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F24))
		    {
			    key = e.KeyCode.ToString();
		    }
		    else if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
		    {
			    key = e.KeyCode.ToString();
			    key = key.Substring(1, key.Length - 1);
		    }

		    if (key != null)
		    {
			    if (sb.Length > 0)
				    sb.Append(" + ");
			    sb.Append(key);
		    }

		    textBox.Text = sb.ToString();

		    e.SuppressKeyPress = true;
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        if (components != null)
        //        {
        //            components.Dispose();

        //            if (_capturing)
        //                this.CaptureMouse(false);
        //        }
        //    }
        //    base.Dispose(disposing);
        //}

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                /*
                 * stop capturing events as soon as the user releases the left mouse button
                 * */
                case (int)Win32.WindowMessages.WM_LBUTTONUP:
                    this.CaptureMouse(false);
                    break;
                /*
                 * handle all the mouse movements
                 * */
                case (int)Win32.WindowMessages.WM_MOUSEMOVE:
                    this.HandleMouseMovements();
                    break;
            };

            base.WndProc(ref m);
        }

        private void OnFinderToolMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.CaptureMouse(true);
        }

        //protected virtual void OnImageReadyForDisplay(Image image)
        //{
        //    try
        //    {
        //        if (this.ImageReadyForDisplay != null)
        //            this.ImageReadyForDisplay(image, false, PictureBoxSizeMode.CenterImage);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //    }
        //}

        //private string GetWindowText(IntPtr hWnd)
        //{
        //    StringBuilder sb = new StringBuilder(256);
        //    Win32.GetWindowText(hWnd, sb, 256);
        //    return sb.ToString();
        //}

        private string GetClassName(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            Win32.GetClassName(hWnd, sb, 256);
            return sb.ToString();
        }

        private void CaptureMouse(bool captured)
        {
            // if we're supposed to capture the window
            if (captured)
            {
                // capture the mouse movements and send them to ourself
                Win32.SetCapture(this.Handle);

                // set the mouse cursor to our finder cursor
                Cursor.Current = _cursorFinder;

                // change the image to the finder gone image
                pictureBox1.Image = _finderGone;
            }
            // otherwise we're supposed to release the mouse capture
            else
            {
                // so release it
                Win32.ReleaseCapture();

                // put the default cursor back
                Cursor.Current = _cursorDefault;

                // change the image back to the finder at home image
                pictureBox1.Image = _finderHome;

                // and finally refresh any window that we were highlighting
                if (_hPreviousWindow != IntPtr.Zero)
                {
                    WindowHighlighter.Refresh(_hPreviousWindow);
                    _hPreviousWindow = IntPtr.Zero;
                }
            }

            // save our capturing state
            _capturing = captured;
        }

        ///// <summary>
        ///// Handles all mouse move messages sent to the Spy Window
        ///// </summary>
        private void HandleMouseMovements()
        {
            // if we're not capturing, then bail out
            if (!_capturing)
                return;

            try
            {
                // capture the window under the cursor's position
                IntPtr hWnd = Win32.WindowFromPoint(Cursor.Position);

                // if the window we're over, is not the same as the one before, and we had one before, refresh it
                if (_hPreviousWindow != IntPtr.Zero && _hPreviousWindow != hWnd)
                    WindowHighlighter.Refresh(_hPreviousWindow);

                // if we didn't find a window.. that's pretty hard to imagine. lol
                if (hWnd == IntPtr.Zero)
                {
                    //_textBoxHandle.Text = null;
                    textBox_class_name.Text = null;
                    //_textBoxText.Text = null;
                    //_textBoxStyle.Text = null;
                    //_textBoxRect.Text = null;
                }
                else
                {
                    // save the window we're over
                    _hPreviousWindow = hWnd;

                    // handle
                    //_textBoxHandle.Text = string.Format("{0}", hWnd.ToInt32().ToString());

                    // class
                    if (Win32.GetParent(hWnd) == (IntPtr)0)
                    {
                        textBox_class_name.Text = this.GetClassName(hWnd);
                    }
                    else
                    {
                        textBox_class_name.Text = this.GetClassName(Win32.GetParent(hWnd));
                    }

                    // caption
                    //_textBoxText.Text = this.GetWindowText(hWnd);

                    Win32.Rect rc = new Win32.Rect();
                    Win32.GetWindowRect(hWnd, ref rc);

                    // rect
                    //_textBoxRect.Text = string.Format("[{0} x {1}], ({2},{3})-({4},{5})", rc.right - rc.left, rc.bottom - rc.top, rc.left, rc.top, rc.right, rc.bottom);

                    // highlight the window
                    WindowHighlighter.Highlight(hWnd);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Occurs when the OK button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonOKClicked(object sender, EventArgs e)
        {
            
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load("data.xml");
            }
            catch (Exception)
            {
                doc.LoadXml("<item></item>");
            }

            XmlElement newElem = doc.CreateElement("player");
            newElem.InnerXml = "<class_name></class_name><play></play><stop></stop><full></full><fwd></fwd><bwd></bwd><mute></mute><vup></vup><vdown></vdown>";
            newElem["class_name"].InnerText = textBox_class_name.Text;
            newElem["play"].InnerText = textBox_play.Text;
            newElem["stop"].InnerText = textBox_stop.Text;
            newElem["full"].InnerText = textBox_full.Text;
            newElem["fwd"].InnerText = textBox_fwd.Text;
            newElem["bwd"].InnerText = textBox_bwd.Text;
            newElem["mute"].InnerText = textBox_mute.Text;
            newElem["vup"].InnerText = textBox_vup.Text;
            newElem["vdown"].InnerText = textBox_vdown.Text;

            doc.DocumentElement.AppendChild(newElem);
            doc.PreserveWhitespace = true;

            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter("data.xml", null);
            writer.Formatting = Formatting.Indented;
            
            doc.WriteTo(writer);
            writer.Close();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Occurs when the Cancel button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonCancelClicked(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Occurs when the user wants to capture the window that we've captured
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void OnButtonCaptureClicked(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // parse the window handle
        //        int handle = int.Parse(_textBoxHandle.Text);

        //        // capture that window
        //        Image image = ScreenCapturing.GetWindowCaptureAsBitmap(handle);

        //        // fire our image read event, which the main window will display for us
        //        this.OnImageReadyForDisplay(image);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //    }
        //}

        /// <summary>
        /// Occurs when the handle textbox changes, to determine if the capture button is enabled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextBoxHandleTextChanged(object sender, EventArgs e)
        {
            //_buttonCapture.Enabled = (_textBoxHandle.Text != null && _textBoxHandle.Text != string.Empty);
        }
    }
}
