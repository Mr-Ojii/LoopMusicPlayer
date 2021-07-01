# LoopMusicPlayer-CSharp
Ogg(Vorbis)を  
``LOOPSTART・[LOOPLENGTH,LOOPEND]``  
のタグをもとにループ再生するミュージックプレイヤー  
ループタグがなかった場合、曲全体をループします

2020/07/02現在、Oggファイル以外のループタグは読み込めません。

## 仕様
### 主な使用方法
* OggファイルをD&Dするか、メニューバーの「ファイル→追加」より選択して、リストに追加してください。
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


### 再生方法について
+ **ストリーミング再生**  
音声ファイルをデコードしながら再生します。  
シーク時に不安定な可能性があります。

+ **オンメモリ再生**  
音声ファイルを先にすべてデコードし、メモリ上に格納してから再生します。  
シーク時には安定して動作しますが、再生開始時にメモリ上への格納待機時間が発生します。

## 開発環境(動作確認環境)
OS
* Windows 10(Ver.21H1) (x64)  
* Linux Mint 20.1(Xfce) (x64)

Editor
* Visual Studio Community 2019  
* Visual Studio Code

## 更新履歴
|バージョン |日付(JST) |                                    実装内容                                    |
|:---------:|:--------:|:-------------------------------------------------------------------------------|
|Ver.0.1.0.0|2021-04-10|初版                                                                            |
|Ver.0.1.0.1|2021-04-10|シークバーの見た目を修正                                                        |
|Ver.0.1.1.0|2021-04-11|サウンド再生時のバッファサイズを広げ、デバイス依存の不具合が起こりにくいよう修正|
|Ver.0.2.0.0|2021-06-24|ループ方法の実装                                                                |
|Ver.0.3.0.0|2021-06-29|オンメモリ再生の実装                                                            |

## 謝辞
各依存パッケージを作成していただいてる方々に感謝を申し上げます。

また、このソフトはぽかん氏のLooplayを参考にし、作成いたしました。ありがとうございます。

## 作者よりいろいろ
このプログラムを使用し発生した、いかなる不具合・損失に対しても、一切の責任を負いません。

これは元々私自身がLinux環境で作業用BGMを聞くために作ったものです。  
いい感じにループして聞ければいいや程度で作ったやつですので、バグだらけです。

環境によってはUIの中に日本語・英語が混じっている可能性があります。  
自分でも気になっているので、いつか修正します。(ごめんね)(Issues #2)

名前にCSharpが付いているのはもともとRustで開発するつもりでしたが、依存パッケージのエラーにより仕方なくC#で書くことにしたからです。  
いつか別言語で書き直すかもしれません。(.NETだとpublishビルドの際にファイルサイズが大きくなるのが嫌なんですよね)

バグ修正PullRequest・バグ報告Issue大歓迎です！

