# Config Discord Bot for BE-Server-Manager
このノートでは、Bedrock Server ManagerのためのDiscord BOTの設定方法などについて紹介していきます。

## 設定方法
1. [Discord Developer Portal](https://discordapp.com/developers/applications/me)から、Discord BOTのTOKENを発行してきます。  
TOKENの発行方法などについては[こちら](https://cod-sushi.com/discord-py-token/)をご確認ください。
2. サーバーマネージャーを起動して、画面上部のメニューの`各種設定`から`DiscordBOT設定`を開きます。
3. `DiscordBOTを有効にする`にチェックを入れて、`BOTのTOKEN`に、先ほどのTOKENを入力します。  
(これを正しく入力しないと、Discord BOTが動作しません。)
4. BOTのプリフィックス（コマンドの先頭に来るもの）を決めて、`BOTのプリフィックス`に入力します。
5. BOTを使用するDiscordサーバーのサーバーIDを、コンマ区切りで`SlashCommandのサーバー`に入力します。  
(これを正しく入力しないと、スラッシュコマンドが登録されない又は、登録まで時間がかかります。)
6. BOTの操作権を与えるユーザーのIDを、コンマ区切りで`コマンド操作権があるユーザー`に入力します。  
(これを正しく入力しないと、BOTを誰も操作できなくなります。)  

なお、下2つの設定(`SlashCommandのサーバー`/`コマンド操作権があるユーザー`)はDiscord上でも変更できます。

## 起動してみる
1. DiscordBOTをサーバーに追加していない場合は[こちら](https://techmel.net/discord-bot-intro/#2_bot)をご参考に、サーバーに招待してください。
2. サーバーマネージャーを起動して、画面上部のメニューの`ファイル`から`DiscordBOTの起動`を開きます。
3. コンソール画面が表示され、しばらくたつと画面に`DiscordBot launched...`と表示されます。  
設定が間違っていなければDiscordにてBOTがオンラインになっています。
4. スラッシュコマンドが表示されない場合は、DiscordBOTの設定が間違っているか、ファイアーウォール・ウイルス対策ソフトによって通信がブロックされている可能性があります。

## コマンド一覧
コマンドはスラッシュコマンド及びコンテキストコマンドに対応しており、構造自体はスラッシュコマンドもコンテキストコマンドも同じです。  
プリフィックスは設定によって変えられるため、省略します。  

<details><summary>コンテキストコマンドとスラッシュコマンドの違い</summary>

`[コマンドプリフィックス][コマンド名]`(例: `m!start`)のような`メッセージを送信するコマンド`がコンテキストコマンドです。  
`/[コマンド名]`(例: `/start`)のような`スラッシュコマンドを送信するコマンド`がスラッシュコマンドです。
</details>

コンテキストコマンド|スラッシュコマンド|概要|コマンド操作権がなくても実行可能
---|---|---|---
`start`|`/start`|サーバーを起動します。|
`stop`|`/stop`|サーバーを停止します。|
`restart`|`/restart`|サーバーを再起動します。|
`update`|`/update`|サーバーを更新します。|
`status`|`/status`|サーバーの状態を表示します。|〇
`whitelist [ユーザーID]`|`/whitelist [ユーザーID]`|ホワイトリストに追加します。|
`backup`|`/backup`|サーバーのバックアップを作成します。|

## 不具合があったら...
[nattyan-tv/be_server_manager](https://github.com/nattyan-tv/be_server_manager)にIssueを立てたり、Pull Requestを送ってください。  
どうしようもない人は、作成者に直接聞いてください。
