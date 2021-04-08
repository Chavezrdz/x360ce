﻿using JocysCom.ClassLibrary.Controls;
using JocysCom.ClassLibrary.Runtime;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using x360ce.Engine;
using x360ce.Engine.Data;

namespace x360ce.App.Controls
{
	/// <summary>
	/// Interaction logic for PadFootControl.xaml
	/// </summary>
	public partial class PadFootControl : UserControl
	{
		public PadFootControl()
		{
			InitializeComponent();
		}

		UserDevice _UserDevice;
		PadSetting _PadSetting;
		MapTo _MappedTo;

		public void SetBinding(MapTo mappedTo, UserDevice userDevice, PadSetting padSetting)
		{
			_MappedTo = mappedTo;
			_UserDevice = userDevice;
			_PadSetting = padSetting;
			var en = padSetting != null;
			ControlsHelper.SetEnabled(CopyButton, en);
			ControlsHelper.SetEnabled(PasteButton, en);
			ControlsHelper.SetEnabled(LoadButton, en);
			ControlsHelper.SetEnabled(AutoButton, en);
			ControlsHelper.SetEnabled(ClearButton, en);
			ControlsHelper.SetEnabled(ResetButton, en);
		}

		private void GameControllersButton_Click(object sender, RoutedEventArgs e)
		{
			var path = System.Environment.GetFolderPath(Environment.SpecialFolder.System);
			path += "\\joy.cpl";
			ControlsHelper.OpenPath(path);
		}

		private void DxTweakButton_Click(object sender, RoutedEventArgs e)
		{
			FileInfo fi;
			var error = EngineHelper.ExtractFile("DXTweak2.exe", out fi);
			if (error != null)
			{
				MessageBox.Show(error.Message);
				return;
			}
			ControlsHelper.OpenPath(fi.FullName);
		}

		private void CopyButton_Click(object sender, RoutedEventArgs e)
		{
			var text = Serializer.SerializeToXmlString(_PadSetting, null, true);
			Clipboard.SetText(text);
		}

		private void PasteButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var xml = Clipboard.GetText();
				var ps = JocysCom.ClassLibrary.Runtime.Serializer.DeserializeFromXmlString<PadSetting>(xml);
				SettingsManager.Current.LoadPadSettingsIntoSelectedDevice(_MappedTo, ps);
			}
			catch (Exception ex)
			{
				var form = new MessageBoxWindow();
				ControlsHelper.CheckTopMost(form);
				form.ShowDialog(ex.Message);
				return;
			}
		}

		private void LoadButton_Click(object sender, RoutedEventArgs e)
		{
			var form = new Forms.LoadPresetsWindow();
			form.Width = 800;
			form.Height = 400;
			ControlsHelper.CheckTopMost(form);
			form.MainControl.InitForm();
			var result = form.ShowDialog();
			if (result == true)
			{
				var ps = form.MainControl.SelectedItem;
				if (ps != null)
				{
					MainForm.Current.UpdateTimer.Stop();
					SettingsManager.Current.LoadPadSettingsIntoSelectedDevice(_MappedTo, ps);
					MainForm.Current.UpdateTimer.Start();
				}
			}
			form.MainControl.UnInitForm();
		}

		private void AutoButton_Click(object sender, RoutedEventArgs e)
		{
			var ud = _UserDevice;
			if (ud == null)
				return;
			var description = Attributes.GetDescription(_MappedTo);
			var form = new MessageBoxWindow();
			var buttons = MessageBoxButton.YesNo;
			var text = string.Format("Do you want to fill all {0} settings automatically?", description);
			if (ud.Device == null && !TestDeviceHelper.ProductGuid.Equals(ud.ProductGuid))
			{
				text = string.Format("Device is off-line. Please connect device to fill all {0} settings automatically.", description);
				buttons = MessageBoxButton.OK;
			}
			var result = form.ShowDialog(text, "Auto Controller Settings", buttons, MessageBoxImage.Question);
			if (result != MessageBoxResult.Yes)
				return;
			var padSetting = AutoMapHelper.GetAutoPreset(ud);
			// Load created setting.
			SettingsManager.Current.LoadPadSettingsIntoSelectedDevice(_MappedTo, padSetting);
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			SettingsManager.Current.ClearAll(_MappedTo);
		}

		private void ResetButton_Click(object sender, RoutedEventArgs e)
		{
			var description = Attributes.GetDescription(_MappedTo);
			var text = string.Format("Do you really want to reset all {0} settings?", description);
			var form = new MessageBoxWindow();
			var result = form.ShowDialog(text, "Reset Controller Settings", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (result == MessageBoxResult.Yes)
			{
				//MainForm.Current.ReloadXinputSettings();
			}
		}

	}
}