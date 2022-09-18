import os
import sys
import json
import asyncio
import datetime
import re
import subprocess
import shutil
import traceback
import pip


import aiohttp
import requests
from mcstatus import BedrockServer as mcb
import psutil
import discord
from discord.ext import commands
print("モジュールインポート完了")



# LATEST_URL = "https://minecraft.azureedge.net/bin-win/bedrock-server-XXXXX.zip"
# DOWNLOAD_PAGE = "https://www.minecraft.net/en-us/download/server/bedrock"
# CHECK_VERSION_PLACE = "behavior_packs/vanilla_XXXXX"


def checkLaunching():
    """Check server is launching"""
    for p in psutil.process_iter(attrs=('name', 'pid', 'cmdline')):
        if p.info["name"] == "bedrock_server.exe":
            return True
    return False


def stopServer():
    """Stop server"""
    for p in psutil.process_iter(attrs=('name', 'pid', 'cmdline')):
        if p.info["name"] == "bedrock_server.exe":
            location = os.path.normpath(p.info["cmdline"][0])
            if location == os.path.normpath(f"{DIR}\\bedrock_server.exe"):
                p.terminate()
                return True
            else:
                return False


def startServer():
    """Start server"""
    subprocess.Popen(
        ["start", f"{DIR}\\bedrock_server.exe"],
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        shell=True
    )


def checkExist():
    """Check bedrock server application exisist"""
    return os.path.isfile(DIR + "\\bedrock_server.exe")


def loadSetting():
    """Load and place server setting"""
    with open(DIR + "\\server.properties", "r") as f:
        for j in [i.strip() for i in f.readlines() if i != "\n"]:
            if j[:1] == "#":
                continue
            else:
                arg = j.split("=", 2)
                serverSetting[arg[0]] = arg[1]


def checkLatestVersion():
    """Check latest version of bedrock server"""
    try:
        url = "https://www.minecraft.net/en-us/download/server/bedrock"
        headers = {
            'User-Agent': "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.33 (KHTML, like Gecko) Chrome/90.0.123.212 Safari/537.33"}
        response = requests.get(url, headers=headers)
        return str(re.search(u"https://minecraft.azureedge.net/bin-win/bedrock-server-[0-9.-]+.zip", response.text).group().replace("https://minecraft.azureedge.net/bin-win/bedrock-server-", "").replace(".zip", ""))
    except Exception as err:
        return err


def checkCurrentVersion():
    """Check current version of bedrock server"""
    # Get "version.txt"'s content from DIR directory
    with open(f"{DIR}\\version.txt", "r", encoding="utf-8") as f:
        ver = f.read()
    ver = re.search(
        r"[0-9.]+", ver.strip().replace("\r\n",
        "\n").replace("\n", "")
    ).group()
    return str(ver)


## def backup():
##     try:
##         shutil.make_archive(os.path.join(config.backup, datetime.datetime.now().strftime('%Y_%m_%d-%H_%M_%S')), format="zip", root_dir=config.location)
##         logging.info(f"Backup {config.name}")
##         sendLog(config.webhook, f"サーバー`{config.name}`を`{config.backup}`にバックアップしました。\n次のバックアップは{config.backupTime}分後です。")
##         global backuptimeCount
##         backuptimeCount = 1
##     except Exception as err:
##         logging.error(err, exc_info=True)
##         sendLog(config.webhook, f"サーバー`{config.name}`のバックアップ中にエラーが発生しました。\n詳しくはログを確認してください。")


async def updateServer():
    """Update server"""
    try:
        url = f"https://minecraft.azureedge.net/bin-win/bedrock-server-{checkLatestVersion()}.zip"
        if (isinstance(url, Exception)):
            return False

        async def fetch(session, url):
            async with session.get(url) as response:
                return await response.content

        async with aiohttp.ClientSession() as session:
            UpdateData = await fetch(session, url)

        UpdateData = requests.get(url).content
        os.makedirs(os.path.join(sys.path[0], "\\tmp"), exist_ok=True)

        with open(os.path.join(sys.path[0], f"\\tmp\\{url.split('/')[-1]}"), mode='wb') as f:
            f.write(UpdateData)
        shutil.copy(
            os.path.join(DIR, "\\permissions.json"),
            os.path.join(sys.path[0], "\\tmp\\permissions.json")
        )
        shutil.copy(
            os.path.join(DIR, "\\server.properties"),
            os.path.join(sys.path[0], "\\tmp\\server.properties")
        )
        shutil.copy(
            os.path.join(DIR, "\\allowlist.json"),
            os.path.join(sys.path[0], "\\tmp\\allowlist.json")
        )
        shutil.copy(
            os.path.join(DIR, "\\whitelist.json"),
            os.path.join(sys.path[0], "\\tmp\\whitelist.json")
        )
        shutil.copytree(
            os.path.join(DIR, "\\worlds"),
            os.path.join(sys.path[0], "\\tmp\\worlds")
        )
        shutil.rmtree(DIR)
        shutil.unpack_archive(os.path.join(
            sys.path[0], f"\\tmp\\{url.split('/')[-1]}"), os.path.join(DIR))
        shutil.copy(
            os.path.join(sys.path[0], "\\tmp\\permissions.json"),
            os.path.join(DIR, "\\permissions.json")
        )
        shutil.copy(
            os.path.join(sys.path[0], "\\tmp\\server.properties"),
            os.path.join(DIR, "\\server.properties")
        )
        shutil.copy(
            os.path.join(sys.path[0], "\\tmp\\allowlist.json"),
            os.path.join(DIR, "\\allowlist.json")
        )
        shutil.copy(
            os.path.join(sys.path[0], "\\tmp\\whitelist.json"),
            os.path.join(DIR, "\\whitelist.json")
        )
        shutil.copytree(
            os.path.join(sys.path[0], "\\tmp\\worlds"),
            os.path.join(DIR, "\\worlds")
        )
        shutil.rmtree(
            os.path.join(sys.path[0], "\\tmp")
        )
        return True
    except Exception as err:
        return err


setting = json.load(open("setting.json"))[0]
dissetting = json.load(open("discord.json"))
PREFIX, DIR, TOKEN, NAME = setting["botPrefix"], setting["location"], setting["botToken"], setting["name"]
serverSetting = {}

loadSetting()

intents = discord.Intents.all()
intents.typing = False
intents.presences = True
intents.members = True
bot = commands.Bot(command_prefix=PREFIX, intents=intents, help_command=None)

address = "localhost"
ipv4 = serverSetting["server-port"]
ipv6 = serverSetting["server-portv6"]

def check(userid: int) -> bool:
    if str(userid) in dissetting["bot_admins"]:
        return True
    else:
        return False


@bot.event
async def on_ready():
    await bot.change_presence(activity=discord.Game(name=f"{PREFIX} | Minecraft BE", type=1), status=discord.Status.online)
    print(f"""\
DiscordBot Launched...

PREFIX: {PREFIX}
USER: {bot.user.name}#{bot.user.discriminator}""")
    print(f"""Current:[{checkCurrentVersion()}]
Latest:[{checkLatestVersion()}]
NeedUpdate:[{checkCurrentVersion() != checkLatestVersion()}]""")
    print("\n#################################################\n")
    print('このコンソールを閉じると、DiscordBOTは終了します。')
    print("\n#################################################\n")


@bot.event
async def on_command_error(ctx: commands.Context, event: Exception):
    if isinstance(event, discord.ext.commands.CommandNotFound):
        await ctx.send(embed=discord.Embed(title="Error: CommandNotFound", description=f"コマンドが見つかりませんでした。\n`{PREFIX}help`をご確認ください。", color=0xFF0000))
    else:
        await ctx.send(embed=discord.Embed(title="Error", description=f"内部エラーが発生しました。\n```sh\n{str(event)}```\n```sh\n{traceback.format_exc()}```", color=0xFF0000))


@bot.command()
async def status(ctx: commands.Context):
    async with ctx.channel.typing():
        server = mcb.lookup(f"{address}:{ipv4}")
        status = None
        error = None
        try:
            status = server.status()
        except Exception as err:
            error = err
        if status is not None:
            await ctx.reply(
                "Status",
                embed=discord.Embed(
                    title=f"{NAME}",
                    description=f"""\
:white_check_mark: Online

`{status.motd}`
Players:`{status.players_online}/{status.players_max}人`
Ping:`{int(status.latency*1000)}ms`
Gamemode:`{status.gamemode}`
Version:`{status.version.version}`
""",
                    color=0x477a1e
                )
            )
        else:
            await ctx.reply(
                "Status",
                embed=discord.Embed(
                    title=f"{NAME}",
                    description=f"""\
:ng: Offline

```
{error}```
""",
                    color=0x477a1e
                )
            )
        return


@bot.command()
async def start(ctx: commands.Context):
    if check(ctx.author.id):
        if checkExist():
            if checkLaunching():
                await ctx.reply(embed=discord.Embed(title="Error", description=f"既にサーバーは実行されています。", color=0xff0000))
            else:
                startServer()
                await ctx.reply(embed=discord.Embed(title="Success", description=f"サーバーを開始しました。", color=0x477a1e))
        else:
            await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーがインストールされていません。", color=0xff0000))
    else:
        await ctx.reply(embed=discord.Embed(title="Forbidden", description="あなたはこのコマンドを実行できません。\nMinecraftサーバーの管理者にお問い合わせください。", color=0xff0000))


@bot.command()
async def stop(ctx: commands.Context):
    if check(ctx.author.id):
        if checkLaunching():
            result = stopServer()
            if result:
                await ctx.reply(embed=discord.Embed(title="Success", description=f"サーバーを停止しました。", color=0x477a1e))
            else:
                await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバー停止ができませんでした。", color=0xff0000))
        else:
            await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーは実行されていません。", color=0xff0000))
    else:
        await ctx.reply(embed=discord.Embed(title="Forbidden", description="あなたはこのコマンドを実行できません。\nMinecraftサーバーの管理者にお問い合わせください。", color=0xff0000))


@bot.command()
async def restart(ctx: commands.Context):
    if check(ctx.author.id):
        if checkExist():
            if checkLaunching():
                message = await ctx.reply(embed=discord.Embed(title="Wait...", description=f"サーバーを停止しています...", color=0x477a1e))
                stopServer()
                await message.edit(embed=discord.Embed(title="Wait...", description=f"サーバーを停止しました。\nサーバー起動準備を行っています...", color=0x477a1e))
                await asyncio.sleep(5)
                await message.edit(embed=discord.Embed(title="Wait...", description=f"サーバーを起動しています...", color=0x477a1e))
                startServer()
                await message.edit(embed=discord.Embed(title="Success", description=f"サーバーを再起動しました。", color=0x477a1e))
            else:
                await ctx.reply(embed=discord.Embed(title="Attention", description=f"サーバーが実行されていませんでした。\n`{PREFIX}start`でサーバーを実行することができます。", color=0x477a1e))
        else:
            await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーがインストールされていません。", color=0xff0000))
    else:
        await ctx.reply(embed=discord.Embed(title="Forbidden", description="あなたはこのコマンドを実行できません。\nMinecraftサーバーの管理者にお問い合わせください。", color=0xff0000))


@bot.command()
async def update(ctx: commands.Context, update_type: str = None):
    if check(ctx.author.id):
        if update_type is None:
            await ctx.reply(embed=discord.Embed(title="Service Temporarily Unavailable", description=f"BOTからの手動アップデート操作は現在テスト段階です。\n確実に動作しませんが、`{PREFIX}update force`でアップデート操作を行うことが出来ます。"))
            return
            if checkExist():
                if checkLatestVersion() != checkCurrentVersion():
                    if checkLaunching():
                        await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーが実行されているためアップデート出来ません。\n強制的にアップデートするには`{PREFIX}update force`と送信してください。", color=0xff0000))
                    else:
                        updateServer()
                        await ctx.reply(embed=discord.Embed(title="Success", description=f"サーバーを更新しました。\nバージョン:`{checkLatestVersion()}`", color=0x477a1e))
                else:
                    await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーは最新のバージョンです。\n強制的にアップデートするには`{PREFIX}update force`と送信してください。", color=0xff0000))
            else:
                await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーがインストールされていません。", color=0xff0000))
        elif update_type == "force":
            if checkExist():
                updateServer()
                await ctx.reply(embed=discord.Embed(title="Success", description=f"サーバーを強制更新しました。\nバージョン:`{checkLatestVersion()}`", color=0x477a1e))
            else:
                await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーがインストールされていません。", color=0xff0000))
        else:
            await ctx.reply(embed=discord.Embed(title="Error", description="コマンドに渡された引数が異常です。", color=0xff0000))
    else:
        await ctx.reply(embed=discord.Embed(title="Forbidden", description="あなたはこのコマンドを実行できません。\nMinecraftサーバーの管理者にお問い合わせください。", color=0xff0000))


@bot.command()
async def whitelist(ctx: commands.Context, command_type: str = None, name: str = None):
    if check(ctx.author.id):
        if checkExist():
            if command_type is None or name is None:
                await ctx.reply(embed=discord.Embed(title="Error", description=f"次の引数が足りません。\n{(lambda x: '`commandType`(`add/del`)' if x is None else '')(command_type)}\n{(lambda x: '`name`(`ユーザー名`)' if x is None else '')(name)}", color=0xff0000))
                return
            if command_type == "add":
                whitelist = []
                if os.path.isfile(f"{DIR}/whitelist.json"):
                    whitelist = json.load(open(f"{DIR}\\whitelist.json"))
                else:
                    whitelist = []
                whitelist.append({"ignoresPlayerLimit": False, "name": name})
                json.dump(whitelist, open(f"{DIR}\\whitelist.json", "w"), indent=4)
                await ctx.reply(embed=discord.Embed(title="Success", description=f"`{name}`をホワイトリストに追加しました。", color=0x477a1e))
            elif command_type == "del":
                whitelist = []
                if os.path.isfile(f"{DIR}/whitelist.json"):
                    whitelist = json.load(open(f"{DIR}\\whitelist.json"))
                else:
                    await ctx.reply(embed=discord.Embed(title="Error", description=f"ホワイトリストが存在しません。", color=0xff0000))
                    return
                for i in whitelist:
                    if i["name"] == name:
                        whitelist.remove(i)
                        json.dump(whitelist, open(
                            f"{DIR}\\whitelist.json", "w"), indent=4)
                        await ctx.reply(embed=discord.Embed(title="Success", description=f"`{name}`をホワイトリストから削除しました。", color=0x477a1e))
                        return
                await ctx.reply(embed=discord.Embed(title="Error", description=f"`{name}`はホワイトリストに存在しません。", color=0xff0000))
            else:
                await ctx.reply(embed=discord.Embed(title="Error", description=f"`{command_type}`は存在しないコマンド種類です。\n`{PREFIX}whitelist [add/del] [ユーザー名]`", color=0xff0000))
        else:
            await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーがインストールされていません。", color=0xff0000))
    else:
        await ctx.reply(embed=discord.Embed(title="Forbidden", description="あなたはこのコマンドを実行できません。\nMinecraftサーバーの管理者にお問い合わせください。", color=0xff0000))


@bot.command()
async def backup(ctx: commands.Context):
    await ctx.reply(embed=discord.Embed(title="Service Temporarily Unavailable", description="BOTからの手動バックアップ操作は現在実装準備中です。", color=0xff0000))

@bot.command()
async def reload(ctx: commands.Context):
    if check(ctx.author.id):
        global dissetting, setting
        setting = json.load(open("setting.json"))[0]
        dissetting = json.load(open("discord.json"))
        global PREFIX, DIR, TOKEN, NAME
        PREFIX, DIR, TOKEN, NAME = setting["botPrefix"], setting["location"], setting["botToken"], setting["name"]
        loadSetting()
        await ctx.reply(embed=discord.Embed(title="Success", description=f"設定を再読み込みしました。", color=0x477a1e))
    else:
        await ctx.reply(embed=discord.Embed(title="Forbidden", description="あなたはこのコマンドを実行できません。\nMinecraftサーバーの管理者にお問い合わせください。", color=0xff0000))


@bot.command()
async def help(ctx: commands.Context):
    await ctx.reply(
        embed=discord.Embed(
            title="Help",
            description=f"""\
`{PREFIX}start`: サーバーを起動します。
`{PREFIX}stop`: サーバーを停止します。
`{PREFIX}restart`: サーバーを再起動します。
`{PREFIX}update`: サーバーを更新します。
`{PREFIX}update force`: サーバーを強制更新します。
`{PREFIX}whitelist [add/del] [ユーザー名]`: サーバーのホワイトリストの設定を行います。
~~`{PREFIX}backup`: サーバーのバックアップを作成します。~~
`{PREFIX}reload`: 設定ファイルなどの変更をDiscordBOTに適応させます。
`{PREFIX}help`: このヘルプを表示します。""",
            color=0x477a1e
        )
    )

bot.run(TOKEN)
