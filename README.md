# IT-KPI-Codewars-Bot
Telegram bot for IT KPI clan on Codewars (https://codewars.com)

https://t.me/itkpi_codewars_bot

### Run mssql server locally
- Make sure [Docker for windows](https://docs.docker.com/docker-for-windows/) is installed
- Make sure linux are enabled containers
- Run `docker-compose up` in the root folder of the repository

See `docker-compose.yml` for the db configuration

To connect you could use the connection string described in `appsettings.Local.json`

##### In debug mode the host creates the db with the necessary schema automatically
##### Integration tests create the db for themself and then cleanup it when they finish
