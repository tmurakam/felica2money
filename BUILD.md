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

src/FeliCa2Money.sln を開いてください。このソリューションには、
FeliCa2Money と FeliCa2Money.test の２つのプロジェクトが含まれています。
前者が FeliCa2Money 本体、後者は単体テストです。

FeliCa2Money を実行するためには、実行ディレクトリに felicalib.dll ファイル
を入れておく必要があります。

単体テストをビルド・実行するためには、NUnit が必要です。
FeliCa2Money.test の参照設定で、NUnit に含まれる nunit.framework を指定
する必要があります。テストを行うには、NUnit から FeliCa2Money.test.dll を
開いて Run してください。

Setup.exe の作成
----------------

Setup.exe を作成するには、Inno Setup (http://www.jrsoftware.org/isinfo.php)
が必要です。定義ファイルは、src/Release ディレクトリ以下にあります。
