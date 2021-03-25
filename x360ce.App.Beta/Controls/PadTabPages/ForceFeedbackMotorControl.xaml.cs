﻿using System;
using System.Windows.Controls;
using x360ce.Engine;
using x360ce.Engine.Data;

namespace x360ce.App.Controls
{
	/// <summary>
	/// Interaction logic for ForceFeedbackMotorControl.xaml
	/// </summary>
	public partial class ForceFeedbackMotorControl : UserControl
	{
		public ForceFeedbackMotorControl()
		{
			InitializeComponent();
			deadzoneLink = new TrackBarUpDownTextBoxLink(StrengthTrackBar, StrengthUpDown, StrengthTextBox, 0, 100);
			offsetLink = new TrackBarUpDownTextBoxLink(PeriodTrackBar, PeriodUpDown, PeriodTextBox, 0, 100);
			testLink = new TrackBarUpDownTextBoxLink(TestTrackBar, TestUpDown, TestTextBox, 0, 100);
			// fill direction values.
			var effectDirections = (ForceEffectDirection[])Enum.GetValues(typeof(ForceEffectDirection));
			DirectionComboBox.ItemsSource = effectDirections;
		}

		TrackBarUpDownTextBoxLink deadzoneLink;
		TrackBarUpDownTextBoxLink offsetLink;
		TrackBarUpDownTextBoxLink testLink;

		public void SetBinding(PadSetting o, int motor)
		{
			// Unbind first.
			SettingsManager.UnLoadMonitor(DirectionComboBox);
			SettingsManager.UnLoadMonitor(StrengthUpDown);
			SettingsManager.UnLoadMonitor(PeriodUpDown);
			if (o == null)
				return;
			switch (motor)
			{
				case 0:
					MainGroupBox.Header = "Left Motor(Big, Strong, Low-Frequency)";
					break;
				case 1:
					MainGroupBox.Header = "Right Motor (Small, Gentle, High-Frequency)";
					break;
				default:
					break;
			}
			var converter = new Converters.DeadZoneConverter();
			// Set binding.
			SettingsManager.LoadAndMonitor(o, nameof(o.LeftMotorDirection), DirectionComboBox);
			SettingsManager.LoadAndMonitor(o, nameof(o.LeftMotorStrength), StrengthUpDown, null, converter);
			SettingsManager.LoadAndMonitor(o, nameof(o.LeftMotorPeriod), PeriodUpDown, null, converter);
		}
	}
}
