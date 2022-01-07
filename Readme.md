# LoopMusicPlayer
``LOOPSTART・[LOOPLENGTH,LOOPEND]``  
のタグをもとにループ再生するミュージックプレイヤー  
ループタグがなかった場合、曲全体をループします

## 仕様
### 使用前の注意点
+ Windows  
以下の作業のいずれかをしてください。
  - [こちら](https://github.com/GtkSharp/GtkSharp/wiki/Installing-Gtk-on-Windows)を参照し、MSYS2を用いGTK+3をインストールする
  - [こちら](https://github.com/tschoonj/GTK-for-Windows-Runtime-Environment-Installer)のインストーラーを用い、GTK+3をインストールする
  - [こちら](https://github.com/GtkSharp/Dependencies/blob/master/gtk-3.24.24.zip)のzipファイルをダウンロード&展開し、LoopMusicPlayer.exeと同じ場所に中身を置く

+ MacOS  
以下の作業をしてください
  - [こちら](https://github.com/GtkSharp/GtkSharp/wiki/Installing-Gtk-on-Mac)を参照し、GTK+3をインストールする


### 主な使用方法
* 音声ファイルをD&Dするか、メニューバーの「ファイル→追加」より選択して、リストに追加してください。
* 再生したいファイルをリストから選択し、ダブルクリック/再生ボタンを押してください。

### 注意事項
* D&D時・ファイルの追加時には複数ファイルを選択することができます。
* 再生方法(ストリーミング再生/オンメモリ再生)を切り替えた場合、再び再生する必要があります。
* ループ時に指定されたサンプルから数サンプルずれている可能性があります。(計算式がずさん)

### ループ方法について
+ **単曲再生**  
ユーザーにより選択された一つの曲のみ再生します。

+ **単曲リピート**  
ユーザーにより選択された一つの曲を再生し、ループ範囲をループし続けます。

+ **全曲リピート**  
再生が終了次第、プレイリスト上の次の曲が再生されます。

+ **ランダム再生**  
再生が終了次第、プレイリスト上の曲をランダムで選択し、再生されます。

### ループ回数について
右側の「00/00」は「ループした回数/ループ回数」を表しています。  
\-/+ ボタンでループ回数を増減できます。

ループ回数は「単曲リピート」モード以外の場合、  
「指定された回数のみループし、指定回数以上ループした場合はループ範囲を抜ける」というものです。

## 開発環境(動作確認環境)
OS
* Windows 10(Ver.21H1) (x64)  
* Linux Mint 20.2(Xfce) (x64)

Editor
* Visual Studio Community 2022  
* Visual Studio Code

## 更新履歴
|バージョン |日付(JST) |                                       実装内容                                       |
|:---------:|:--------:|:-------------------------------------------------------------------------------------|
|Ver.0.1.0.0|2021-04-10|初版                                                                                  |
|Ver.0.1.0.1|2021-04-10|シークバーの見た目を修正                                                              |
|Ver.0.1.1.0|2021-04-11|サウンド再生時のバッファサイズを広げ、デバイス依存の不具合が起こりにくいよう修正      |
|Ver.0.2.0.0|2021-06-24|ループ方法の実装                                                                      |
|Ver.0.3.0.0|2021-06-29|オンメモリ再生の実装                                                                  |
|Ver.0.4.0.0|2021-07-03|デコード作業を全てBASSに任せるよう変更 (OGGファイル以外も音声のみは読み込めるように)  |
|Ver.0.4.1.0|2021-10-28|LinuxでWindowが非表示の際に、次の曲に移行できない問題の修正                           |
|Ver.0.4.2.0|2021-11-11|フレームワークを.NET 6に変更                                                          |
|Ver.0.4.2.1|2021-11-11|ストリーミング再生を用いた際に、正常にループできない可能性がある問題の修正(BASSの更新)|
|Ver.0.4.2.2|2021-11-17|プロジェクトの内部を変更したためのバージョン更新                                      |
|Ver.0.4.2.3|2021-11-17|音声ファイル読み込み時に落ちる問題の修正                                              |
|Ver.0.4.2.4|2021-11-21|音声ファイル読み込み時に数サンプル音声が再生されてしまう問題の修正                    |
|Ver.0.4.3.0|2021-12-22|デバイスオープン時の周波数をデバイスに合わせるように変更                              |
|Ver.0.4.3.1|2021-12-23|ストリーミング再生時に正常にループしない可能性がある問題の修正                        |
|Ver.0.4.4.0|2021-12-30|再生デバイス情報表示メニューの追加                                                    |
|Ver.0.5.0.0|2021-12-31|オンメモリ再生の削除                                                                  |
|Ver.0.6.0.0|2021-12-31|Opus・Flac・WavPackが格納されたファイルを読み込めるように                             |
|Ver.0.6.0.1|2022-01-01|「常に最前面に表示」を有効中にバージョン情報を表示した際、操作不能になる問題の修正    |
|Ver.0.6.0.2|2022-01-07|ループ時のサンプル数計算の修正(処理落ち時に次の曲に移行してしまう問題の修正)          |
|Ver.0.6.0.3|2022-01-08|頒布ファイルサイズの削減                                                              |
|Ver.0.6.1.0|2022-01-08|終了時の状態保存の実装                                                                |

## 謝辞
各依存パッケージを作成していただいてる方々に感謝を申し上げます。

また、このソフトはぽかん氏のLooplayを参考にし、作成いたしました。ありがとうございます。

## 作者よりいろいろ
このプログラムを使用し発生した、いかなる不具合・損失に対しても、一切の責任を負いません。

これは元々私自身がLinux環境で作業用BGMを聞くために作ったものです。  
いい感じにループして聞ければいいや程度で作ったやつです。

バグ修正PullRequest・バグ報告Issue大歓迎です！

