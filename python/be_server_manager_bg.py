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

backuptimeCount = 0
updatetimeCount = 0

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
            "username": "BSM",
            "avatar_url": "https://static.wikia.nocookie.net/minecraft_ja_gamepedia/images/a/a9/Birch_Forest_Grass_Block.png"
        }
        response = requests.post(
            URL, json.dumps(send_content), headers=HEADERS
        )
        logging.info(URL, response, response.json["content"])
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
        if config.autobackup and backuptimeCount == config.backupTime:
            backup(config)
        if config.autoupdate and updatetimeCount == config.updateTime:
            asyncio.ensure_future(update(config))
        backuptimeCount += 1
        updatetimeCount += 1
    except Exception as err:
        logging.error(err, exc_info=True)

def loadConfig() -> BSM_Config:
    print("config loading...")
    try:
        setting = json.load(open("setting.json"))
        if os.path.exists(os.path.join(BASE_DIR, "webhook.txt")):
            webhook = open(os.path.join(BASE_DIR, "webhook.txt"), encoding="utf-8").readlines()[0].strip()
        else:
            webhook = None
        config = BSM_Config(setting["name"], setting["location"], setting["update"], setting["backup"], setting["backupTime"], setting["autoupdate"], setting["autobackup"], webhook)
        sendLog(webhook, config)
        return config
    except Exception as err:
        logging.error(err, exc_info=True)


def backup(config: BSM_Config):
    try:
        ZIP = zipfile.ZipFile(os.path.join(config.backup, f"{datetime.datetime.now().strftime('%Y_%m_%d-%H_%M_%S')}.zip"), "w")
        ZIP.write(f"{config.location}/", os.path.basename(config.location))
        ZIP.close()
        logging.info(f"Backup {config.name}")
        global backuptimeCount
        backuptimeCount = 0
    except Exception as err:
        logging.error(err, exc_info=True)


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


async def update(config: BSM_Config):
    try:
        DIR = config.location
        url = f"https://minecraft.azureedge.net/bin-win/bedrock-server-{checkLatestVersion()}.zip"
        if (isinstance(url, Exception)):
            return False

        async def fetch(session, url):
            async with session.get(url) as response:
                return await response.content

        async with aiohttp.ClientSession() as session:
            UpdateData = await fetch(session, url)

        UpdateData = requests.get(url).content
        os.makedirs(os.path.join(BASE_DIR, "\\tmp"), exist_ok=True)

        with open(os.path.join(BASE_DIR, f"\\tmp\\{url.split('/')[-1]}"), mode='wb') as f:
            f.write(UpdateData)

        shutil.copy(
            os.path.join(DIR, "\\permissions.json"),
            os.path.join(BASE_DIR, "\\tmp\\permissions.json")
        )
        shutil.copy(
            os.path.join(DIR, "\\server.properties"),
            os.path.join(BASE_DIR, "\\tmp\\server.properties")
        )
        shutil.copy(
            os.path.join(DIR, "\\allowlist.json"),
            os.path.join(BASE_DIR, "\\tmp\\allowlist.json")
        )
        shutil.copy(
            os.path.join(DIR, "\\whitelist.json"),
            os.path.join(BASE_DIR, "\\tmp\\whitelist.json")
        )
        shutil.copytree(
            os.path.join(DIR, "\\worlds"),
            os.path.join(BASE_DIR, "\\tmp\\worlds")
        )
        shutil.rmtree(DIR)
        shutil.unpack_archive(os.path.join(
            BASE_DIR, f"\\tmp\\{url.split('/')[-1]}"), os.path.join(DIR))
        shutil.copy(
            os.path.join(BASE_DIR, "\\tmp\\permissions.json"),
            os.path.join(DIR, "\\permissions.json")
        )
        shutil.copy(
            os.path.join(BASE_DIR, "\\tmp\\server.properties"),
            os.path.join(DIR, "\\server.properties")
        )
        shutil.copy(
            os.path.join(BASE_DIR, "\\tmp\\allowlist.json"),
            os.path.join(DIR, "\\allowlist.json")
        )
        shutil.copy(
            os.path.join(BASE_DIR, "\\tmp\\whitelist.json"),
            os.path.join(DIR, "\\whitelist.json")
        )
        shutil.copytree(
            os.path.join(BASE_DIR, "\\tmp\\worlds"),
            os.path.join(DIR, "\\worlds")
        )
        shutil.rmtree(
            os.path.join(BASE_DIR, "\\tmp")
        )
        logging.info(f"Update {config.name}")
        global updatetimeCount
        updatetimeCount = 0
    except Exception as err:
        logging.error(err, exc_info=True)


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
