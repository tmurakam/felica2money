FeliCa2Moneyビルド手順
======================

ソースコード
------------

最新版のソースコードは、github で提供しています。

 https://github.com/tmurakam/felica2money

git で取り出してください。

必要環境
--------

ビルドには以下の環境が必要です。

 * Visual Studio 2010 Professional以上
 * .NET Framework 2.0 以降
 * felicalib (http://felicalib.tmurakam.org/)

ビルド手順
----------

src/FeliCa2Money.sln を開いてビルドしてください。
実行ディレクトリには、felicalib.dll ファイルを入れておく必要があります。

Setup.exe の作成
----------------

Setup.exe を作成するには、Inno Setup (http://www.jrsoftware.org/isinfo.php)
が必要です。定義ファイルは、src/Release ディレクトリ以下にあります。
