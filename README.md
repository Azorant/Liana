<h1 align="center">Liana</h1>

<h4 align="center">Yet another all-in-one moderation bot.</h4>

<p align="center">
  <a href="https://discord.gg/66dp9gxMZx">
    <img src="https://discordapp.com/api/guilds/918704583717572639/widget.png?style=shield" alt="Discord Server">
  </a>
  <a href="https://github.com/Azorant/Liana/actions">
    <img src="https://img.shields.io/github/actions/workflow/status/Azorant/Liana/docker-publish.yml?label=Build" alt="GitHub Actions">
  </a>
</p>

# Getting Started
Install [Docker](https://docs.docker.com/engine/install/) and [Docker Compose](https://docs.docker.com/compose/install/)

Create `docker-compose.yml` with the following content:

```yaml
version: '3.8'

services:
  liana:
    image: ghcr.io/azorant/liana:latest # or :master
    container_name: liana
    restart: unless-stopped
    environment:
      - DISCORD_INVITE=server invite
      - TOKEN=bot token
      - GUILD_CHANNEL=channel ID for guild events
      - LOG_CHANNEL=channel for logging node and track events
      - DB=server=db;user=root;password=example;database=liana
  db:
    image: mariadb
    container_name: db
    restart: unless-stopped
    environment:
      MARIADB_ROOT_PASSWORD: example
    expose:
      - 3306
```

Run `docker compose up -d` to startup Liana and MariaDB.

That's all!