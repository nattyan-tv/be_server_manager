import os, sys, json, asyncio, datetime
import time
from mcstatus import MinecraftBedrockServer as mcb

# Check Minecraft server status and show it.

def cls():
    os.system("cls")

def main():
    address, port = sys.argv[1], sys.argv[2]
    while True:
        server = mcb.lookup(f"{address}:{port}")
        status = None
        error = None
        try:
            status = server.status()
        except BaseException as err:
            error = err
        nowtime = datetime.datetime.now()
        # get status
        # get time_
        cls()
        if status != None:
            print(f"""\
{address}:{port}
{nowtime.year}/{nowtime.month}/{nowtime.day} {nowtime.hour}:{nowtime.minute}:{nowtime.second}

Player:{status.players_online}/{status.players_max}
Ping:{int(status.latency*1000)}ms
Gamemode:{status.gamemode}
Version:{status.version.version}

{status.motd}
""")
        else:
            print(f"""\
{address}:{port}
{nowtime.year}/{nowtime.month}/{nowtime.day} {nowtime.hour}:{nowtime.minute}:{nowtime.second}

Offline.
""")
        time.sleep(1)


if __name__ == "__main__":
    main()