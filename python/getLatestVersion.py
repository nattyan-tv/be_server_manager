import sys, os, re
import requests

if __name__ == "__main__":
    if len(sys.argv) == 1:
        print("Unknown arg.")
    elif sys.argv[1] == "-getLatest":
        url = "https://www.minecraft.net/en-us/download/server/bedrock"
        headers = {'User-Agent': "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.33 (KHTML, like Gecko) Chrome/90.0.123.212 Safari/537.33"}
        response = requests.get(url, headers=headers)
        print(re.search(u"https://minecraft.azureedge.net/bin-win/bedrock-server-[0-9.-]+.zip",response.text).group())
else:
    print("Unknown arg.")