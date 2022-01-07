using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace LoopMusicPlayer
{
    internal class Setting
    {
        public static string SettingFilePath = AppContext.BaseDirectory + "LoopMusicPlayer.json";

        public StSetting SettingStruct;

        public Setting()
        {
            SettingStruct = new StSetting();
            if (File.Exists(SettingFilePath))
            {
                using (var JsonFileStream = File.OpenText(SettingFilePath))
                {
                    SettingStruct = JsonSerializer.Deserialize<StSetting>(JsonFileStream.ReadToEnd());
                }
            }
        }

        public void SaveJson()
        {
            using (var JsonFileStream = File.CreateText(SettingFilePath))
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                JsonFileStream.Write(JsonSerializer.Serialize(SettingStruct, options));
            }
        }

        public class StSetting{
            public bool IsShowGridLine { get; set; } = false;
            public ETimePositionShowMethod TimePositionShowMethod { get; set; } = ETimePositionShowMethod.SeekTime;
            public bool IsWindowKeepAbove { get; set; } = false;
            public ERepeatMethod RepeatMethod { get; set; } = ERepeatMethod.SingleRepeat;
            public double Volume { get; set; } = 1;
        }

        //Enum
        public enum ETimePositionShowMethod
        {
            ElapsedTime,
            SeekTime,
            RemainingTime,
        }

        public enum ERepeatMethod
        {
            SinglePlay,
            SingleRepeat,
            AllRepeat,
            RandomPlay,
        }
    }
}
