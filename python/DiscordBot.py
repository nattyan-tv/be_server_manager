import os, sys, json, asyncio, datetime
from mcstatus import MinecraftBedrockServer as mcb
import requests
import discord
from discord.ext import commands

intents = discord.Intents.all()
intents.typing = False
intents.presences = True
intents.members = True
bot = commands.Bot(command_prefix="m!", intents=intents, help_command=None)
bot.remove_command("help")

name, address, port = ["ark","dinosaur.f5.si","19132"]

@bot.event
async def on_ready():
    print("DiscordBot Launched...")

@bot.command()
async def status(ctx: discord.command.Context):
    async with ctx.channel.typing():
        server = mcb.lookup(f"{address}:{port}")
        status = None
        error = None
        try:
            status = server.status()
        except BaseException as err:
            error = err
        nowtime = datetime.datetime.now()
        if status != None:
            await ctx.reply(
                "Status",
                embed=discord.Embed(
                    title=f"{name}",
                    description=f"""\
:white_check_mark: Online
`{status.motd}`
Players:`{status.players_online}/{status.players_max}人`
Ping:`{int(status.latency*1000)}ms`
Gamemode:`{status.gamemode}`
Version:`{status.version.version}`
""",
                    color=0x477a1e,
                    inline=False
                )
            )
        else:
            await ctx.reply(
                "Status",
                embed=discord.Embed(
                    title=f"{name}",
                    description=f"""\
:ng: Offline
""",
                    color=0x477a1e,
                    inline=False
                )
            )
        return

@bot.command()
async def start(ctx: discord.command.Context):
    return

@bot.command()
async def stop(ctx: discord.command.Context):
    return

@bot.command()
async def restart(ctx: discord.command.Context):
    return

@bot.command()
async def update(ctx: discord.command.Context):
    return

bot.run("a")