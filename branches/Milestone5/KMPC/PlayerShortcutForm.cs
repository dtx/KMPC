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
    public partial class PlayerShortcutForm : Form
    {
        private bool _capturing;
        private Image _finderHome;
        private Image _finderGone;
        private Cursor _cursorDefault;
        private Cursor _cursorFinder;
        private IntPtr _hPreviousWindow;
        private List<string> _playerlist = new List<string>();

        public PlayerShortcutForm()
        {
            InitializeComponent();
            _playerlist.Add("");
            try
            {
                XmlReader reader = XmlReader.Create("data.xml");
                while (reader.Read())
                {
                    if (reader.Name == "class_name")
                    {
                        _playerlist.Add(reader.ReadElementContentAsString());
                    }
                }
                reader.Close();
            }
            catch (FileNotFoundException)
            {
                //we don't do anything at runtime if there is no data.xml
            }

            playlist_dropbox.Items.AddRange(_playerlist.ToArray());

            _cursorDefault = Cursor.Current;
            _cursorFinder = EmbeddedResources.LoadCursor(EmbeddedResources.Finder);
            _finderHome = EmbeddedResources.LoadImage(EmbeddedResources.FinderHome);
            _finderGone = EmbeddedResources.LoadImage(EmbeddedResources.FinderGone);

            uxFindPictureBox.Image = _finderHome;
            uxFindPictureBox.MouseDown += OnFinderToolMouseDown;
            button_ok.Click += OnButtonOKClicked;
            button_cancel.Click += OnButtonCancelClicked;
            textBox_play.KeyDown += textBox_KeyDown;
            textBox_stop.KeyDown += textBox_KeyDown;
            textBox_full.KeyDown += textBox_KeyDown;
            textBox_mute.KeyDown += textBox_KeyDown;
            textBox_fwd.KeyDown += textBox_KeyDown;
            textBox_bwd.KeyDown += textBox_KeyDown;
            textBox_vup.KeyDown += textBox_KeyDown;
            textBox_vdown.KeyDown += textBox_KeyDown;

            //this.AcceptButton = button_ok;
            //this.CancelButton = button_cancel;
        }

        /// <summary>
        /// Check key presses. Convert them into a string and show in a textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="textBox"></param>
        void textBox_KeyDown(object sender, KeyEventArgs e)
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

		    this.Text = sb.ToString();

		    e.SuppressKeyPress = true;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                /*
                 * stop capturing events as soon as the user releases the left mouse button
                 * */
                case (int)Win32.WindowMessages.WM_LBUTTONUP:
                    this.SetMouseCapture(false);
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
                this.SetMouseCapture(true);
        }

        /// <summary>
        /// Get classname of a window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        private string GetClassName(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            Win32.GetClassName(hWnd, sb, 256);
            return sb.ToString();
        }

        private void SetMouseCapture(bool captured)
        {
            // if we're supposed to capture the window
            if (captured)
            {
                // capture the mouse movements and send them to ourself
                Win32.SetCapture(this.Handle);

                // set the mouse cursor to our finder cursor
                Cursor.Current = _cursorFinder;

                // change the image to the finder gone image
                uxFindPictureBox.Image = _finderGone;
            }
            // otherwise we're supposed to release the mouse capture
            else
            {
                // so release it
                Win32.ReleaseCapture();

                // put the default cursor back
                Cursor.Current = _cursorDefault;

                // change the image back to the finder at home image
                uxFindPictureBox.Image = _finderHome;

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
                    textBox_class_name.Text = null;
                }
                else
                {
                    // save the window we're over
                    _hPreviousWindow = hWnd;

                    // class
                    if (Win32.GetParent(hWnd) == (IntPtr)0)
                    {
                        textBox_class_name.Text = this.GetClassName(hWnd);
                    }
                    else
                    {
                        textBox_class_name.Text = this.GetClassName(Win32.GetParent(hWnd));
                    }

                    Win32.Rect rc = new Win32.Rect();
                    Win32.GetWindowRect(hWnd, ref rc);

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
            bool delete_player = false;

            if (textBox_class_name.Text.Trim().Equals(""))
            {
                System.Windows.Forms.MessageBox.Show("Please select a player window.");
                return;
            }

            if (checkBox_delete.Checked == false)
            {
                if (CheckEmptyTextBox())
                {
                    if (MessageBox.Show("Some shortcuts are empty. Are you sure you want to save?", "Empty Shortcuts!!!", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return;
                    }
                }
            }
            else
            {
                if (MessageBox.Show("Are you sure to delete?", "Delete Shortcuts!!!", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
                else
                {
                    delete_player = true;
                }
            }

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load("data.xml");
            }
            catch (FileNotFoundException)
            {
                doc.LoadXml("<item></item>");
            }

            try
            {
                XmlReader reader = XmlReader.Create("data.xml");

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "class_name")
                    {
                        if (textBox_class_name.Text.Trim().Equals(reader.ReadElementContentAsString()))
                        {
                            if ((delete_player == false) && (MessageBox.Show("Same media player is recorded before. Do you want to replace?", "Same player found", MessageBoxButtons.YesNo) == DialogResult.No))
                            {
                                return;
                            }
                            else
                            {
                                XmlNode node = doc.SelectSingleNode(String.Format("/item/player[class_name='{0}']", textBox_class_name.Text.Trim()));
                                node.ParentNode.RemoveChild(node);
                                break;
                            }
                        }
                    }
                }
                reader.Close();
            }
            catch (FileNotFoundException)
            {
                //we don't do anything at runtime if there is no data.xml
            }

            if (delete_player == false)
            {
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
            }

            doc.PreserveWhitespace = true;
            
            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter("data.xml", null);
            writer.Formatting = Formatting.Indented;
            
            doc.WriteTo(writer);
            writer.Close();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private bool CheckEmptyTextBox()
        {
            var textboxes = new [] {textBox_play, textBox_stop, textBox_full, textBox_mute, textBox_bwd, textBox_fwd, textBox_vdown, textBox_vup};
            return textboxes.Any(txt => txt.Text.Trim().Equals(String.Empty));
            //return textBox_play.Text.Trim().Equals("") || textBox_stop.Text.Trim().Equals("") || textBox_full.Text.Trim().Equals("") || textBox_mute.Text.Trim().Equals("")
            //                    || textBox_bwd.Text.Trim().Equals("") || textBox_fwd.Text.Trim().Equals("") || textBox_vdown.Text.Trim().Equals("") || textBox_vup.Text.Trim().Equals("");
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
        /// Change the text in different textboxes according to selected index from the dropdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playlist_dropbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkBox_delete.Checked = false;
            if (_playerlist[playlist_dropbox.SelectedIndex].Equals(String.Empty))
            {
                checkBox_delete.Enabled = false;
                textBox_class_name.Text = String.Empty;
                textBox_play.Text = String.Empty;
                textBox_stop.Text = String.Empty;
                textBox_full.Text = String.Empty;
                textBox_fwd.Text = String.Empty;
                textBox_bwd.Text = String.Empty;
                textBox_mute.Text = String.Empty;
                textBox_vup.Text = String.Empty;
                textBox_vdown.Text = String.Empty;
                return;
            }
            else
            {
                checkBox_delete.Enabled = true;
            }

            XmlReader reader = XmlReader.Create("data.xml");

            while (reader.Read())
            {
                if ((reader.Name == "class_name")&&(_playerlist[playlist_dropbox.SelectedIndex] == reader.ReadElementContentAsString()))
                {
                    textBox_class_name.Text = _playerlist[playlist_dropbox.SelectedIndex];
                    reader.ReadToFollowing("play");
                    textBox_play.Text = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("stop");
                    textBox_stop.Text = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("full");
                    textBox_full.Text = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("fwd");
                    textBox_fwd.Text = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("bwd");
                    textBox_bwd.Text = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("mute");
                    textBox_mute.Text = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("vup");
                    textBox_vup.Text = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("vdown");
                    textBox_vdown.Text = reader.ReadElementContentAsString();
                    break;
                }
            }
            reader.Close();
        }
    }
}
