import sys, os, re
import requests

def getLatestVersion():
    url = "https://www.minecraft.net/en-us/download/server/bedrock"
    headers = {'User-Agent': "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.33 (KHTML, like Gecko) Chrome/90.0.123.212 Safari/537.33"}
    response = requests.get(url, headers=headers)
    return str(re.search(u"https://minecraft.azureedge.net/bin-win/bedrock-server-[0-9.-]+.zip",response.text).group())

if __name__ == "__main__":
    print(getLatestVersion())