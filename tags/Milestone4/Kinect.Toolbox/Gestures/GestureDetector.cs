using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using Microsoft.Kinect;

namespace Kinect.Toolbox
{
    public abstract class GestureDetector
    {
       
        public int MinimalPeriodBetweenGestures { get; set; }

        readonly List<Entry> entries = new List<Entry>();

        public event Action<string> OnGestureDetected;

        DateTime lastGestureDate = DateTime.Now;

        readonly int windowSize;

        Canvas displayCanvas;
        Color displayColor;

        protected GestureDetector(int windowSize = 20)
        {
            this.windowSize = windowSize;
            MinimalPeriodBetweenGestures = 0;
        }

        protected List<Entry> Entries
        {
            get { return entries; }
        }

        public int WindowSize
        {
            get { return windowSize; }
        }

        public virtual void Add(SkeletonPoint position, KinectSensor sensor)
        {
            Entry newEntry = new Entry {Position = position.ToVector3(), Time = DateTime.Now};
            Entries.Add(newEntry);

            if (displayCanvas != null)
            {
                newEntry.DisplayEllipse = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    StrokeThickness = 2.0,
                    Stroke = new SolidColorBrush(displayColor),
                    StrokeLineJoin = PenLineJoin.Round
                };

                Vector2 vector2 = Tools.Convert(sensor, position);

                float x = (float)(vector2.X * displayCanvas.ActualWidth);
                float y = (float)(vector2.Y * displayCanvas.ActualHeight);

                Canvas.SetLeft(newEntry.DisplayEllipse, x - newEntry.DisplayEllipse.Width / 2);
                Canvas.SetTop(newEntry.DisplayEllipse, y - newEntry.DisplayEllipse.Height / 2);

                displayCanvas.Children.Add(newEntry.DisplayEllipse);
            }

            if (Entries.Count > WindowSize)
            {
                Entry entryToRemove = Entries[0];
                
                if (displayCanvas != null)
                {
                    displayCanvas.Children.Remove(entryToRemove.DisplayEllipse);
                }

                Entries.Remove(entryToRemove);
            }

            LookForGesture();
        }

        protected void RaiseGestureDetected(string gesture)
        {
            if (DateTime.Now.Subtract(lastGestureDate).TotalMilliseconds > MinimalPeriodBetweenGestures)
            {
                if (OnGestureDetected != null)
                {
                    OnGestureDetected(gesture);
                }

                lastGestureDate = DateTime.Now;
            }

            Entries.ForEach(e=>
                                {
                                    if (displayCanvas != null)
                                        displayCanvas.Children.Remove(e.DisplayEllipse);
                                });
            Entries.Clear();
        }

        protected abstract void LookForGesture();

        public void TraceTo(Canvas canvas, Color color)
        {
            displayCanvas = canvas;
            displayColor = color;
        }
    }
}