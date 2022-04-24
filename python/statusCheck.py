import os, sys, json, asyncio, time
import mcstatus

# Check Minecraft server status and show it.

def cls():
    os.system("cls")

def main(address: str, port: int):
    while True:
        # get status
        # get time
        print(f"""\
Minecraft Server {address}:{port}""")

    return


if __name__ == "__main__":
    main(sys.argv[1], int(sys.argv[2]))