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

try:
    import aiohttp
    import requests
    from mcstatus import MinecraftBedrockServer as mcb
    import psutil
    import discord
    from discord.ext import commands
    print("モジュールインポート完了")
except Exception as err:
    try:
        print(f"Install modules...", file=sys.stderr)
        pip.main(['install', '-r', f'{os.getcwd()}/python/requirements.txt'])
        import aiohttp
        import requests
        from mcstatus import MinecraftBedrockServer as mcb
        import psutil
        import discord
        from discord.ext import commands
        print("モジュールインポート完了")
    except Exception as err:
        print(f"An error has occurred.\n{err}\n\n開発者にお問い合わせください。")
        os._exit(1)


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
            p.terminate()


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


setting = json.load(open("setting.json"))
PREFIX, DIR, TOKEN, NAME = setting["botPrefix"], setting["location"], setting["botToken"], setting["name"]
serverSetting = {}

loadSetting()

intents = discord.Intents.all()
intents.typing = False
intents.presences = True
intents.members = True
bot = commands.Bot(command_prefix=PREFIX, intents=intents, help_command=None)

address = "localhost"
ipv4 = serverSetting["server_port"]
ipv6 = serverSetting["server_portv6"]


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
    print("\n#################################################")


@bot.event
async def on_command_error(ctx: commands.Context, event: Exception):
    if isinstance(event, discord.ext.commands.ArgumentParsingError):
        await ctx.send(embed=discord.Embed(title="Error: ArgumentParsingError", description=f"コマンドの引数指定方法がおかしいです。", color=0xFF0000))
    elif isinstance(event, discord.ext.commands.CommandNotFound):
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
        if status != None:
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
    if checkExist():
        if checkLaunching():
            await ctx.reply(embed=discord.Embed(title="Error", description=f"既にサーバーは実行されています。", color=0xff0000))
        else:
            startServer()
            await ctx.reply(embed=discord.Embed(title="Success", description=f"サーバーを開始しました。", color=0x477a1e))
    else:
        await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーがインストールされていません。", color=0xff0000))
    return


@bot.command()
async def stop(ctx: commands.Context):
    if checkLaunching():
        stopServer()
        await ctx.reply(embed=discord.Embed(title="Success", description=f"サーバーを停止しました。", color=0x477a1e))
    else:
        await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーは実行されていません。", color=0xff0000))
    return


@bot.command()
async def restart(ctx: commands.Context):
    if checkExist():
        if checkLaunching():
            message = await ctx.reply(embed=discord.Embed(title="Wait...", description=f"サーバーを停止しています...", color=0x477a1e))
            stopServer()
            await message.edit(embed=discord.Embed(title="Wait...", description=f"サーバーを停止しました。\nサーバー起動準備を行っています...", color=0x477a1e))
            await asyncio.sleep(5)
            startServer()
            await message.edit(embed=discord.Embed(title="Success", description=f"サーバーを再起動しました。", color=0x477a1e))
        else:
            await ctx.reply(embed=discord.Embed(title="Attention", description=f"サーバーが実行されていませんでした。\n`{PREFIX}start`でサーバーを実行することができます。", color=0x477a1e))
    else:
        await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーがインストールされていません。", color=0xff0000))
    return


@bot.command()
async def update(ctx: commands.Context):
    args = ctx.message.content.split(" ")
    if len(args) == 1:
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
    elif args[1] == "force":
        if checkExist():
            updateServer()
            await ctx.reply(embed=discord.Embed(title="Success", description=f"サーバーを強制更新しました。\nバージョン:`{checkLatestVersion()}`", color=0x477a1e))
        else:
            await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーがインストールされていません。", color=0xff0000))
    return


@bot.command()
async def whitelist(ctx: commands.Context, commandType: str, name: str):
    if checkExist():
        if commandType == "add":
            whitelist = list
            if os.path.isfile(f"{DIR}/whitelist.json"):
                whitelist = json.load(open(f"{DIR}\\whitelist.json"))
            else:
                whitelist = []
            whitelist.append({"ignoresPlayerLimit": False, "name": name})
            json.dump(whitelist, open(f"{DIR}\\whitelist.json", "w"), indent=4)
            await ctx.reply(embed=discord.Embed(title="Success", description=f"`{name}`をホワイトリストに追加しました。", color=0x477a1e))
        elif commandType == "del":
            whitelist = list
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
            await ctx.reply(embed=discord.Embed(title="Error", description=f"`{commandType}`は存在しないコマンド種類です。\n`{PREFIX}whitelist [add/del] [ユーザー名]`", color=0xff0000))
        return
    else:
        await ctx.reply(embed=discord.Embed(title="Error", description=f"サーバーがインストールされていません。", color=0xff0000))
    return


@bot.command()
async def backup(ctx: commands.Context):
    return


@bot.command()
async def reload(ctx: commands.Context):
    setting = json.load(open("setting.json"))
    global PREFIX, DIR, TOKEN, NAME
    PREFIX, DIR, TOKEN, NAME = setting["botPrefix"], setting["location"], setting["botToken"], setting["name"]
    loadSetting()
    await ctx.reply(embed=discord.Embed(title="Success", description=f"設定を再読み込みしました。", color=0x477a1e))


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
`{PREFIX}backup`: サーバーのバックアップを作成します。
`{PREFIX}reload`: 設定ファイルなどの変更をDiscordBOTに適応させます。
`{PREFIX}help`: このヘルプを表示します。""",
            color=0x477a1e
        )
    )

bot.run(TOKEN)
