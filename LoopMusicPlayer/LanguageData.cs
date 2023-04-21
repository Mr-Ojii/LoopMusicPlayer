using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMusicPlayer;

internal class LanguageData
{
    public readonly static LanguageData Default = new LanguageData();

    public string LanguageName { get; set; } = "日本語(Default)";
    public CLanguage Data { get; set; } = new CLanguage();
    
    public class CLanguage
    {
        public string File { get; set; } = "ファイル";
        public string View { get; set; } = "表示";
        public string Setting { get; set; } = "設定";
        public string Others { get; set; } = "その他";
        public string AddFile { get; set; } = "追加";
        public string DeleteFile { get; set; } = "削除";
        public string ClearFile { get; set; } = "クリア";
        public string Quit { get; set; } = "終了";
        public string GridLine { get; set; } = "グリッドライン";
        public string ElapsedTime { get; set; } = "経過時間";
        public string SeekTime { get; set; } = "シーク時間";
        public string RemainingTime { get; set; } = "残り時間";
        public string WindowOnTop { get; set; } = "常に最前面に表示";
        public string SinglePlay { get; set; } = "単曲再生";
        public string SingleRepeat { get; set; } = "単曲リピート";
        public string AllRepeat { get; set; } = "全曲リピート";
        public string RandomPlay { get; set; } = "ランダム再生";
        public string Preference { get; set; } = "設定";
        public string DeviceInfo { get; set; } = "再生デバイス情報";
        public string About { get; set; } = "LoopMusicPlayerについて";
    }
}
