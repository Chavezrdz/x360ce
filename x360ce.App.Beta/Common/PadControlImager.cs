﻿using JocysCom.ClassLibrary.Controls;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;
using System.Windows.Media;
using x360ce.Engine;

namespace x360ce.App.Controls
{
    public partial class PadControlImager : IDisposable
    {

        public static object imagesLock = new object();

        static Bitmap topImage;
        //static Bitmap frontImage;
        //static Bitmap topDisabledImage;
        //static Bitmap frontDisabledImage;

        //static ImageSource _TopImage;
        //static ImageSource _FrontImage;
        //static ImageSource _TopDisabledImage;
        //static ImageSource _FrontDisabledImage;

        public PadControlImager()
        {
            locations.Add(GamepadButtonFlags.Y, new Point(196, 29));
            lock (imagesLock)
            {
                var a = GetType().Assembly;
                // Create images.
                if (topImage == null)
                {
                    var keys = JocysCom.ClassLibrary.Helper.GetResourceKeys(a);
                    //var ti = JocysCom.ClassLibrary.Helper.GetResourceValue("images/xboxcontrollertop.png", a);
                    //topImage = new Bitmap(ti);
                    //var ti2 = JocysCom.ClassLibrary.Helper.GetResourceValue("images/xboxcontrollerfront.png", a);
                    //frontImage = new Bitmap(ti2);
                    //topDisabledImage = AppHelper.GetDisabledImage(topImage);
                    //frontDisabledImage = AppHelper.GetDisabledImage(frontImage);
                    // WPF.
                    //_TopImage = ControlsHelper.GetImageSource(topImage);
                    //_FrontImage = ControlsHelper.GetImageSource(frontImage);
                    //_TopDisabledImage = ControlsHelper.GetImageSource(topDisabledImage);
                    //_FrontDisabledImage = ControlsHelper.GetImageSource(frontDisabledImage);
                }
                // Other.
                markB = JocysCom.ClassLibrary.Helper.FindResource<Bitmap>("Images.MarkButton.png", a);
                markA = JocysCom.ClassLibrary.Helper.FindResource<Bitmap>("Images.MarkAxis.png", a);
                markC = JocysCom.ClassLibrary.Helper.FindResource<Bitmap>("Images.MarkController.png", a);
                //float rH = topDisabledImage.HorizontalResolution;
                //float rV = topDisabledImage.VerticalResolution;
                // Make sure resolution is same everywhere so images won't be resized.
                //markB.SetResolution(rH, rV);
                //markA.SetResolution(rH, rV);
                //markC.SetResolution(rH, rV);
                //Recorder = new Recorder(rH, rV);
                Recorder = new Recorder(0, 0);
            }
        }

        public Recorder Recorder;

        // Green round button image.
        public Bitmap markB;
        // Green cross axis image.
        public Bitmap markA;
        // Green round controller/player number image.
        public Bitmap markC;

        public PadItem_General_XboxImageControl ImageControl;

        Dictionary<GamepadButtonFlags, Point> locations = new Dictionary<GamepadButtonFlags, Point>();

        // Background Images.
        public System.Windows.Controls.Image Top;
        public System.Windows.Controls.Image Front;

        // Axis status Borders.
        public System.Windows.Controls.Border LeftThumbAxisStatus;
        public System.Windows.Controls.Border RightThumbAxisStatus;
        public System.Windows.Controls.Border LeftTriggerAxisStatus;
        public System.Windows.Controls.Border RightTriggerAxisStatus;

        public System.Windows.Shapes.Path DPadUpStatus;

        //      public void SetImages(bool enabled)
        //{
        //	////Top.Source = enabled ? _TopImage : _TopDisabledImage;
        //	////Front.Source = enabled ? _FrontImage : _FrontDisabledImage;
        //	////var show = enabled ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        //	////LeftThumbAxisStatus.Visibility = show;
        //	////RightThumbAxisStatus.Visibility = show;
        //	////LeftTriggerAxisStatus.Visibility = show;
        //	////RightTriggerAxisStatus.Visibility = show;
        //}

        public void DrawController(PaintEventArgs e, MapTo mappedTo)
        {
            // Controller (Player) index indicator coordinates.
            var pads = new Point[4];
            pads[0] = new Point(116, 35);
            pads[1] = new Point(139, 35);
            pads[2] = new Point(116, 62);
            pads[3] = new Point(139, 62);
            // Display controller index light.
            int mW = -markC.Width / 2;
            int mH = -markC.Height / 2;
            var index = (int)mappedTo - 1;
            e.Graphics.DrawImage(markC, pads[index].X + mW, pads[index].Y + mH);
        }

        public bool ShowRightThumbButtons;
        public bool ShowLeftThumbButtons;
        public bool ShowDPadButtons;
        public bool ShowMainButtons;
        public bool ShowMenuButtons;
        public bool ShowTriggerButtons;
        public bool ShowShoulderButtons;

        bool on = false;
        bool recording = false;

        public void DrawState(ImageInfo ii, Gamepad gp)
        {
            // Trigger axis state with "•" yellow circle.
            if (ii.Code == MapCode.LeftTrigger || ii.Code == MapCode.RightTrigger)
            {
                var isLeft = ii.Code == MapCode.LeftTrigger;
                var control = isLeft ? LeftTriggerAxisStatus : RightTriggerAxisStatus;
                var h = (float)(((System.Windows.FrameworkElement)control.Parent).Height - control.Height);
                var y = isLeft ? gp.LeftTrigger : gp.RightTrigger;
                var b = ConvertHelper.ConvertRangeF(y, byte.MinValue, byte.MaxValue, 0, h);
                var m = control.Margin;
                control.Margin = new System.Windows.Thickness(m.Left, m.Top, m.Right, b);
                on = y > 0;
                recording = false;
            }
            // Show stick axis state with "•" yellow circle.
            else if (ii.Code == MapCode.LeftThumbButton || ii.Code == MapCode.RightThumbButton)
            {
                var isLeft = ii.Code == MapCode.LeftThumbButton;
                var control = isLeft ? LeftThumbAxisStatus : RightThumbAxisStatus;
                var w = (float)(((System.Windows.FrameworkElement)control.Parent).Width - control.Width);
                var x = isLeft ? gp.LeftThumbX : gp.RightThumbX;
                var y = isLeft ? gp.LeftThumbY : gp.RightThumbY;
                var l = ConvertHelper.ConvertRangeF(x, short.MinValue, short.MaxValue, -w, w);
                var t = ConvertHelper.ConvertRangeF(y, short.MinValue, short.MaxValue, w, -w);
                var m = control.Margin;
                control.Margin = new System.Windows.Thickness(l, t, m.Right, m.Bottom);
            }
            // If D-Pad.
            else if (ii.Code == MapCode.DPad)
            {
                on =
                    gp.Buttons.HasFlag(GamepadButtonFlags.DPadUp) ||
                    gp.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) ||
                    gp.Buttons.HasFlag(GamepadButtonFlags.DPadRight) ||
                    gp.Buttons.HasFlag(GamepadButtonFlags.DPadDown);
                recording = false;

            }
            // If button is not specified then...
            else if (ii.Button == GamepadButtonFlags.None)
            {
                var t = 2000;
                // This is axis.
                short value = 0;
                if (ii.Code == MapCode.LeftThumbAxisX)
                    value = gp.LeftThumbX;
                else if (ii.Code == MapCode.LeftThumbAxisY)
                    value = gp.LeftThumbY;
                else if (ii.Code == MapCode.RightThumbAxisX)
                    value = gp.RightThumbX;
                else if (ii.Code == MapCode.RightThumbAxisY)
                    value = gp.RightThumbY;
                // Check when value is on.
                on = value < -t || value > t;
                if (ii.Code == MapCode.LeftThumbRight)
                    on = gp.LeftThumbX > t;
                if (ii.Code == MapCode.LeftThumbLeft)
                    on = gp.LeftThumbX < -t;
                if (ii.Code == MapCode.LeftThumbUp)
                    on = gp.LeftThumbY > t;
                if (ii.Code == MapCode.LeftThumbDown)
                    on = gp.LeftThumbY < -t;
                if (ii.Code == MapCode.RightThumbRight)
                    on = gp.RightThumbX > t;
                if (ii.Code == MapCode.RightThumbLeft)
                    on = gp.RightThumbX < -t;
                if (ii.Code == MapCode.RightThumbUp)
                    on = gp.RightThumbY > t;
                if (ii.Code == MapCode.RightThumbDown)
                    on = gp.RightThumbY < -t;
            }
            else
            {
                // Check when value is on.
                on = gp.Buttons.HasFlag(ii.Button);
                recording = false;
            }


            var isRecordingItem = Recorder.Recording && ii.Code == Recorder.CurrentMap.Code;
            // If record then...
            if (Recorder.Recording)
            {
                MapCode? redirect = null;
                if (Recorder.CurrentMap.Code == MapCode.RightThumbAxisX)
                    redirect = MapCode.RightThumbRight;
                if (Recorder.CurrentMap.Code == MapCode.RightThumbAxisY)
                    redirect = MapCode.RightThumbUp;
                if (Recorder.CurrentMap.Code == MapCode.LeftThumbAxisX)
                    redirect = MapCode.LeftThumbRight;
                if (Recorder.CurrentMap.Code == MapCode.LeftThumbAxisY)
                    redirect = MapCode.LeftThumbUp;
                if (redirect.HasValue)
                {
                    MapCode recordingCode = ii.Code;
                    recordingCode = redirect.Value;
                    // Skip if redirected control.
                    if (ii.Code == recordingCode)
                        return;
                }
            }
            // If record is in progress then...
            if (isRecordingItem)
            {
                // ImageControl.SetImage(recordingCode, NavImageType.Record, Recorder.DrawRecordingImage);
                recording = true;
                on = true;
            }
            //else if (
            //	 ShowLeftThumbButtons && SettingsConverter.LeftThumbCodes.Contains(ii.Code) ||
            //	 ShowRightThumbButtons && SettingsConverter.RightThumbCodes.Contains(ii.Code) ||
            //	 ShowDPadButtons && SettingsConverter.DPadCodes.Contains(ii.Code) ||
            //	 ShowMainButtons && SettingsConverter.MainButtonCodes.Contains(ii.Code) ||
            //	 ShowMenuButtons && SettingsConverter.MenuButtonCodes.Contains(ii.Code) ||
            //	 ShowTriggerButtons && SettingsConverter.TriggerButtonCodes.Contains(ii.Code) ||
            //	 ShowShoulderButtons && SettingsConverter.ShoulderButtonCodes.Contains(ii.Code)
            //)
            //{
            //	var nit = on ? NavImageType.Active : NavImageType.Normal;
            //	ImageControl.SetImage(ii.Code, nit, true);
            //}
            //else
            //{
            //	var isAxisCode = SettingsConverter.AxisCodes.Contains(ii.Code);
            //	// Axis status will be displayed as image therefore can hide active button indicator.
            //	ImageControl.SetImage(ii.Code, NavImageType.Active, on && !isAxisCode);
            //}

            if (ii.Label is System.Windows.Controls.ContentControl)
                setColorNormaOrActiveRecord(recording, on, ii);
        }

        SolidColorBrush colorActive = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF42C765");
        SolidColorBrush colorRecord = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFF6B66");
        SolidColorBrush colorNormalPath = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF6699FF");
        SolidColorBrush colorOver = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFCC66");
        SolidColorBrush colorNormalTextBox = System.Windows.Media.Brushes.White;

        void setColorNormaOrActiveRecord(bool record, bool on, ImageInfo ii)
        {
            System.Windows.Controls.TextBox textbox = (System.Windows.Controls.TextBox)ii.Control;
            System.Windows.Shapes.Path path = ii.Path;
            if ((path.Fill as SolidColorBrush).Color == colorRecord.Color) return;
            if ((path.Fill as SolidColorBrush).Color == colorOver.Color) return;

            // Active.
            if (on)
            {
                textbox.Background = path.Fill = colorActive;
            }
            //// Record.
            //else if (record && on)
            //{
            //    textbox.Background = path.Fill = colorRecord;
            //}
            // Normal.
            else
            {
                textbox.Background = colorNormalTextBox;
                path.Fill = colorNormalPath;
            }
        }

        #region ■ IDisposable

        public bool IsDisposing;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposing = true;
                Top = null;
                Front = null;
                LeftThumbAxisStatus = null;
                RightThumbAxisStatus = null;
                LeftTriggerAxisStatus = null;
                RightTriggerAxisStatus = null;
                ImageControl = null;
                // Dispose other.
                markA.Dispose();
                markB.Dispose();
                markC.Dispose();
                markB = null;
                markA = null;
                markC = null;
                Recorder.Dispose();
                Recorder = null;
                locations.Clear();
                locations = null;
            }
        }

        #endregion
    }

}
