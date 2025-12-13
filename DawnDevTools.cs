using DevInterface;
using UnityEngine;
using Dawn.Components;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;

namespace Dawn {
	public class DawnDevTools {
		public static Time currentTime = Time.Day;

		private static MethodInfo roomSettingsSaveMethod;

		public void Initialize() {
			currentTime = Time.NONE;

			On.DevInterface.Page.ctor += this.On_Page_ctor;
			On.DevInterface.RoomSettingsPage.Signal += this.On_RoomSettingsPage_Signal;
			// On.DevInterface.ObjectsPage.Signal += On_ObjectsPage_Signal;
			// On.DevInterface.SoundPage.Signal += On_SoundPage_Signal;
			// On.DevInterface.MapPage.Signal += On_MapPage_Signal;
			// On.DevInterface.TriggersPage.Signal += On_TriggersPage_Signal;
			// On.DevInterface.DialogPage.Signal += On_DialogPage_Signal;

			On.DevInterface.RoomSettingsPage.Refresh += this.On_RoomSettingsPage_Refresh;
			On.RoomSettings.Save_string_bool += this.On_RoomSettings_Save;
			On.RoomSettings.Load_Timeline += this.On_RoomSettings_Load;

			On.DevInterface.RoomSettingSlider.Refresh += this.On_RoomSettingSlider_Refresh;
			On.DevInterface.RoomSettingSlider.NubDragged += this.On_RoomSettingSlider_NubDragged;
			On.DevInterface.RoomSettingSlider.ClickedResetToInherent += this.On_RoomSettingSlider_ClickedResetToInherit;

			On.DevInterface.DevUI.SwitchPage += this.On_DevUI_SwitchPage;

			roomSettingsSaveMethod = typeof(RoomSettings).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(string), typeof(bool)], null);
		}

		public void Cleanup() {
			On.DevInterface.Page.ctor -= this.On_Page_ctor;
			On.DevInterface.RoomSettingsPage.Signal += this.On_RoomSettingsPage_Signal;
			// On.DevInterface.ObjectsPage.Signal -= On_ObjectsPage_Signal;
			// On.DevInterface.SoundPage.Signal -= On_SoundPage_Signal;
			// On.DevInterface.MapPage.Signal -= On_MapPage_Signal;
			// On.DevInterface.TriggersPage.Signal -= On_TriggersPage_Signal;
			// On.DevInterface.DialogPage.Signal -= On_DialogPage_Signal;

			On.DevInterface.RoomSettingsPage.Refresh -= this.On_RoomSettingsPage_Refresh;
			On.RoomSettings.Save_string_bool -= this.On_RoomSettings_Save;
			On.RoomSettings.Load_Timeline -= this.On_RoomSettings_Load;

			On.DevInterface.RoomSettingSlider.Refresh -= this.On_RoomSettingSlider_Refresh;
			On.DevInterface.RoomSettingSlider.NubDragged -= this.On_RoomSettingSlider_NubDragged;
			On.DevInterface.RoomSettingSlider.ClickedResetToInherent -= this.On_RoomSettingSlider_ClickedResetToInherit;

			On.DevInterface.DevUI.SwitchPage -= this.On_DevUI_SwitchPage;

			roomSettingsSaveMethod = null;
		}

		public static Time GetCurrentTime() {
			return currentTime;
		}

		private void On_DevUI_SwitchPage(On.DevInterface.DevUI.orig_SwitchPage orig, DevUI self, int newPage) {
			orig(self, newPage);

			if (self.activePage is not RoomSettingsPage) {
				currentTime = Time.NONE;
			}
		}

		private void On_RoomSettingSlider_ClickedResetToInherit(On.DevInterface.RoomSettingSlider.orig_ClickedResetToInherent orig, RoomSettingSlider self) {
			RoomSettings backup = self.owner.room.roomSettings;

			if (backup is DawnRoomSettings settings)
				self.owner.room.roomSettings = settings.GetTimeSetting(GetCurrentTime());

			orig(self);

			self.owner.room.roomSettings = backup;
		}

		private void On_RoomSettingSlider_NubDragged(On.DevInterface.RoomSettingSlider.orig_NubDragged orig, RoomSettingSlider self, float nubPos) {
			RoomSettings backup = self.owner.room.roomSettings;

			if (backup is DawnRoomSettings settings)
				self.owner.room.roomSettings = settings.GetTimeSetting(GetCurrentTime());

			orig(self, nubPos);

			self.owner.room.roomSettings = backup;
		}

		private void On_RoomSettingSlider_Refresh(On.DevInterface.RoomSettingSlider.orig_Refresh orig, RoomSettingSlider self) {
			RoomSettings backup = self.owner.room.roomSettings;

			if (backup is DawnRoomSettings settings)
				self.owner.room.roomSettings = settings.GetTimeSetting(GetCurrentTime());

			orig(self);

			self.owner.room.roomSettings = backup;
		}

		private bool On_RoomSettings_Load(On.RoomSettings.orig_Load_Timeline orig, RoomSettings self, SlugcatStats.Timeline timelinePoint) {
			if (self is not DawnRoomSettings settings)
				return orig(self, timelinePoint);

			bool result = false;

			string path = self.filePath;
			if (path == null)
				return false;

			foreach (KeyValuePair<Time, RoomSettings> entry in settings.timeSettings) {
				string specialPath = path.Substring(0, path.LastIndexOf(".")) + "_dawn-" + entry.Key + ".txt"; // `xx_room_settings_dawn-DAY.txt`

				if (entry.Key == Time.NONE)
					specialPath = path; // Store NONE into default path

				entry.Value.filePath = specialPath;
				bool selfResult = entry.Value.Load(timelinePoint);

				if (entry.Key == Time.NONE)
					result = selfResult; // Return result properly
			}

			settings.SetAll();

			return result;
		}

		private void On_RoomSettings_Save(On.RoomSettings.orig_Save_string_bool orig, RoomSettings self, string path, bool saveAsTemplate) {
			orig(self, path, saveAsTemplate);

			if (self is not DawnRoomSettings settings)
				return;

			Dictionary<Time, RoomSettings> newSettings = [];

			foreach (KeyValuePair<Time, RoomSettings> entry in settings.timeSettings) {
				RoomSettings settingsEntry = entry.Value;

				string specialPath = path.Substring(0, path.LastIndexOf(".")) + "_dawn-" + entry.Key + ".txt";
				if (entry.Key == Time.NONE) {
					settingsEntry = settings.CopyMainTo(settingsEntry);
					specialPath = path;
				}

				settingsEntry.filePath = specialPath;
				if (entry.Key != Time.NONE && DawnRoomSettings.EmptySettings(settingsEntry)) {
					File.Delete(specialPath);
				}
				else {
					roomSettingsSaveMethod.Invoke(settingsEntry, [specialPath, false]);
				}

				newSettings[entry.Key] = settingsEntry;
			}

			settings.timeSettings = newSettings;
		}

		private void On_RoomSettingsPage_Refresh(On.DevInterface.RoomSettingsPage.orig_Refresh orig, RoomSettingsPage self) {
			RoomSettings backup = self.RoomSettings;

			if (backup is DawnRoomSettings settings)
				self.owner.room.roomSettings = settings.GetTimeSetting(GetCurrentTime());
			orig(self);

			self.owner.room.roomSettings = backup;
		}

		private void ApplyDawnSignals(Action<Page, DevUISignalType, DevUINode, string> orig, Page self, DevUISignalType type, DevUINode sender, string message) {
			if (type == DevUISignalType.ButtonClick) {
				string idstring = sender.IDstring;
				if (idstring.StartsWith("Dawn_Time_")) {
					currentTime = new Time(idstring.Substring(10), false);
					self.Refresh();
				}
			}

			if (type == DevUISignalType.Create) {
				List<RoomSettings.RoomEffect> effects = self.RoomSettings.effects;

				self.RoomSettings.effects = ((DawnRoomSettings) self.RoomSettings).GetTimeSetting(GetCurrentTime()).effects;
				orig(self, type, sender, message);

				self.RoomSettings.effects = effects;
			}
			else {
				orig(self, type, sender, message);
			}
		}


		private void On_DialogPage_Signal(On.DevInterface.DialogPage.orig_Signal orig, DialogPage self, DevUISignalType type, DevUINode sender, string message) {
			this.ApplyDawnSignals((self, type, sender, message) => orig((DialogPage) self, type, sender, message), self, type, sender, message);
		}

		private void On_TriggersPage_Signal(On.DevInterface.TriggersPage.orig_Signal orig, TriggersPage self, DevUISignalType type, DevUINode sender, string message) {
			this.ApplyDawnSignals((self, type, sender, message) => orig((TriggersPage) self, type, sender, message), self, type, sender, message);
		}

		private void On_MapPage_Signal(On.DevInterface.MapPage.orig_Signal orig, MapPage self, DevUISignalType type, DevUINode sender, string message) {
			this.ApplyDawnSignals((self, type, sender, message) => orig((MapPage) self, type, sender, message), self, type, sender, message);
		}

		private void On_SoundPage_Signal(On.DevInterface.SoundPage.orig_Signal orig, SoundPage self, DevUISignalType type, DevUINode sender, string message) {
			this.ApplyDawnSignals((self, type, sender, message) => orig((SoundPage) self, type, sender, message), self, type, sender, message);
		}

		private void On_ObjectsPage_Signal(On.DevInterface.ObjectsPage.orig_Signal orig, ObjectsPage self, DevUISignalType type, DevUINode sender, string message) {
			this.ApplyDawnSignals((self, type, sender, message) => orig((ObjectsPage) self, type, sender, message), self, type, sender, message);
		}

		private void On_RoomSettingsPage_Signal(On.DevInterface.RoomSettingsPage.orig_Signal orig, RoomSettingsPage self, DevUISignalType type, DevUINode sender, string message) {
			this.ApplyDawnSignals((self, type, sender, message) => orig((RoomSettingsPage) self, type, sender, message), self, type, sender, message);
		}

		private void On_Page_ctor(On.DevInterface.Page.orig_ctor orig, Page self, DevUI owner, string IDstring, DevUINode parentNode, string name) {
			orig(self, owner, IDstring, parentNode, name);

			if (self is not DevInterface.RoomSettingsPage)
				return;

			float x = 100f;
			foreach (string time in Time.values.entries) {
				self.subNodes.Add(new DawnTimeButton(owner, "Dawn_Time_" + time, new Time(time, false), self, new Vector2(x, 710f), 70f, time));
				x += 75f;
			}
		}
	}
}