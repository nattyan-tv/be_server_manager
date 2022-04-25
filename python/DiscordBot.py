import os, sys, json, asyncio, datetime
from mcstatus import BedrockStatus as mcb
import requests, webhook
import discord

intents = nextcord.Intents.all()
intents.typing = False
intents.presences = True
intents.members = True
bot = commands.Bot(command_prefix=PREFIX, intents=intents, help_command=None)
bot.remove_command("help")



@bot.event
async def on_ready():
    print("DiscordBot Launched...")

@bot.command()
async def info(ctx: discord.command.Context):
    print(ctx.message.author)