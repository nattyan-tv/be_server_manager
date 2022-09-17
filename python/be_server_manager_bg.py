import asyncio
import time
import os
import sys
import json
import shutil
import zipfile
import requests
import re
import aiohttp
import schedule
import datetime
import logging
from tendo import singleton

me = singleton.SingleInstance()

backuptimeCount = 1
updatetimeCount = 1

BASE_DIR = os.getcwd()
LOG_PLS = os.path.join(os.getcwd(), "python\\bsm_bg.log")

FORMATTER = '%(asctime)s$%(filename)s$%(lineno)d$%(funcName)s$%(levelname)s:%(message)s'
logging.basicConfig(
    format=FORMATTER,
    filename=LOG_PLS,
    level=logging.INFO,
)


HEADERS = {'Content-Type': 'application/json'}

def sendLog(URL: str | None, content: str):
    if URL is not None:
        send_content = {
            "content": content,
            "username": "Bedrock Server Manager",
            "avatar_url": "https://static.wikia.nocookie.net/minecraft_ja_gamepedia/images/a/a9/Birch_Forest_Grass_Block.png"
        }
        response = requests.post(
            URL, json.dumps(send_content), headers=HEADERS
        )
        logging.info(URL, response)
    else:
        logging.info("ログURLが指定されていないため、ログは送信されませんでした。")


class BSM_Config:
    def __init__(self, name: str, location: str, update: str, backup: str, backupTime: str, autoupdate: bool, autobackup: bool, webhook: str | None):
        self.name = name
        self.location = location
        self.update = update
        self.backup = backup
        self.backupTime = backupTime
        self.autoupdate = autoupdate
        self.autobackup = autobackup
        self.webhook = webhook

def timing():
    try:
        global backuptimeCount
        global updatetimeCount
        time = datetime.datetime.now()
        config = loadConfig()
        if config.autobackup and str(backuptimeCount) == config.backupTime:
            backup(config)
        if config.autoupdate and str(updatetimeCount) == config.update:
            asyncio.ensure_future(update(config))
        backuptimeCount += 1
        updatetimeCount += 1
    except Exception as err:
        logging.error(err, exc_info=True)

def loadConfig() -> BSM_Config:
    print(f"update: {updatetimeCount} - backup: {backuptimeCount}")
    try:
        setting = json.load(open("setting.json"))
        if os.path.exists(os.path.join(BASE_DIR, "webhook.txt")):
            webhook = open(os.path.join(BASE_DIR, "webhook.txt"), encoding="utf-8").readlines()[0].strip()
        else:
            webhook = None
        config = BSM_Config(setting["name"], setting["location"], setting["update"], setting["backup"], setting["backupTime"], setting["autoupdate"], setting["autobackup"], webhook)
        return config
    except Exception as err:
        logging.error(err, exc_info=True)


def backup(config: BSM_Config):
    try:
        shutil.make_archive(os.path.join(config.backup, datetime.datetime.now().strftime('%Y_%m_%d-%H_%M_%S')), format="zip", root_dir=config.location)
        logging.info(f"Backup {config.name}")
        sendLog(config.webhook, f"サーバー`{config.name}`を`{config.backup}`にバックアップしました。\n次のバックアップは{config.backupTime}分後です。")
        global backuptimeCount
        backuptimeCount = 1
    except Exception as err:
        logging.error(err, exc_info=True)
        sendLog(config.webhook, f"サーバー`{config.name}`のバックアップ中にエラーが発生しました。\n詳しくはログを確認してください。")


def checkLatestVersion():
    """Check latest version of bedrock server"""
    try:
        url = "https://www.minecraft.net/en-us/download/server/bedrock"
        headers = {
            'User-Agent': "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.33 (KHTML, like Gecko) Chrome/90.0.123.212 Safari/537.33"}
        response = requests.get(url, headers=headers)
        return str(re.search(u"https://minecraft.azureedge.net/bin-win/bedrock-server-[0-9.-]+.zip", response.text).group().replace("https://minecraft.azureedge.net/bin-win/bedrock-server-", "").replace(".zip", ""))
    except Exception as err:
        logging.error(err, exc_info=True)
        return err


async def update(config: BSM_Config):
    VERSION = "[未取得]"
    CURRENT = "[未取得]"
    try:
        DIR = config.location
        VERSION = checkLatestVersion()
        CURRENT = open(os.path.join(config.location, "version.txt"), encoding="utf-8").readlines()[0].strip()
        if CURRENT == VERSION:
            sendLog(config.webhook, f"サーバー`{config.name}`のアップデート結果: アップデートの必要はありません。\n現在バージョン: `v{CURRENT}`\n最新バージョン: `v{VERSION}`")
            return
        else:
            sendLog(config.webhook, f"サーバー`{config.name}`のアップデート結果: アップデートの必要があります。アップデートを開始します...\n現在バージョン: `v{CURRENT}`\n最新バージョン: `v{VERSION}`")
        url = f"https://minecraft.azureedge.net/bin-win/bedrock-server-{VERSION}.zip"
        if (isinstance(url, Exception)):
            return False

        async def fetch(session: aiohttp.ClientSession, url):
            async with session.get(url) as response:
                return response.content

        async with aiohttp.ClientSession() as session:
            UpdateData = await fetch(session, url)

        UpdateData = requests.get(url).content

        os.makedirs(os.path.join(BASE_DIR, "temp"), exist_ok=True)
        os.makedirs(os.path.join(BASE_DIR, "temp\\update"), exist_ok=True)
        with open(os.path.join(BASE_DIR, f"temp\\update\\{url.split('/')[-1]}"), mode='wb') as f:
            f.write(UpdateData)

        def copy(origin: str, back: bool = False, folder: bool = False) -> None:
            if not folder:
                if not back:
                    if os.path.exists(os.path.join(DIR, origin)):
                        shutil.copy(
                            os.path.join(DIR, origin),
                            os.path.join(BASE_DIR, f"temp\\")
                        )
                else:
                    if os.path.exists(os.path.join(BASE_DIR, f"temp\\{origin}")):
                        shutil.copy(
                            os.path.join(BASE_DIR, f"temp\\{origin}"),
                            DIR
                        )
            else:
                if not back:
                    if os.path.exists(os.path.join(DIR, origin)):
                        shutil.copytree(
                            os.path.join(DIR, origin),
                            os.path.join(BASE_DIR, f"temp\\{origin}")
                        )
                else:
                    if os.path.exists(os.path.join(BASE_DIR, f"temp\\{origin}")):
                        shutil.copytree(
                            os.path.join(BASE_DIR, f"temp\\{origin}"),
                            os.path.join(DIR, origin)
                        )

        copy("permissions.json")
        copy("server.properties")
        copy("allowlist.json")
        copy("whitelist.json")
        copy("worlds", folder=True)
        shutil.rmtree(DIR)
        shutil.unpack_archive(os.path.join(
            BASE_DIR,
            f"temp\\update\\{url.split('/')[-1]}"),
            os.path.join(DIR)
        )
        copy("permissions.json", back=True)
        copy("server.properties", back=True)
        copy("allowlist.json", back=True)
        copy("whitelist.json", back=True)
        copy("worlds", back=True, folder=True)

        with open(os.path.join(DIR, "version.txt"), mode="w") as f:
            f.write(VERSION)

        logging.info(f"Update {config.name}")
        sendLog(config.webhook, f"サーバー`{config.name}`のアップデート結果: アップデートを完了させました。\n現在バージョン: `v{CURRENT}`\n最新バージョン: `v{VERSION}`")
        global updatetimeCount
        updatetimeCount = 1
    except Exception as err:
        logging.error(err, exc_info=True)
        sendLog(config.webhook, f"サーバー`{config.name}`のアップデート結果: エラーにより中断されました。\nバージョン: `v{VERSION}`")


async def main():
    print(f"BE Server Manager Background Service")
    logging.info("BE Server Manager Background Service")
    print(BASE_DIR, os.getcwd(), LOG_PLS)
    schedule.every(1).minutes.do(timing)
    while True:
        schedule.run_pending()
        await asyncio.sleep(1)


if __name__ == "__main__":
    asyncio.run(main())
