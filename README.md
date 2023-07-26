# Sudoku Collective

Sudoku Collective is an open source Web API that is used to learn frontend client side technologies such as [React](https://reactjs.org/) or [Vue.js](https://vuejs.org/). With this API developers will create an app that allows players to play [sudoku](https://en.wikipedia.org/wiki/Sudoku) puzzles and compare their performance against other players. The benefit of using this tool is that once the developer creates their first app they will obtain an understanding of how the API works and will then be better able to compare and understand various frontend technologies like [React](https://reactjs.org/) or [Vue.js](https://vuejs.org/). The API is [fully documented](https://sudokucollective.com/swagger/index.html) so developers can integrate their client apps with the API. The goals are to learn, develop and have fun!

## Requirements

- [.Net 7.0](https://dotnet.microsoft.com/download/dotnet/7.0)
- [PostgreSQL 14](https://www.postgresql.org/download/)
- [Redis Server - version 6.2.7](https://redis.io/download)

For the Redis Cache Server on Windows 10 it is recommended you use [Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/install-win10) in order to install and run Redis through Ubuntu on [Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/install-win10).  The instructions for downloading from the official Ubuntu PPA are contained in the above Redis link.  For Windows 11 you can install and configure a local Redis Server using [Docker](https://www.docker.com/).

## Installation

In the API project you will find a **dummysettings.json** file that is a stand in for the **appsettings.json** file that configures the project.  Additionally you will find dummy files for the **appsettings.Development.json**, **appsettings.Staging.json**, and **appsettings.Production.json** files and **appsettings.Test.json** in the Test project.  Simply rename the **dummysettings.json** to **appsettings.json** and place your value where it states **[Your value here]**.  Following the same process for the respective appsetting environment files.

For the **License** field in **DefaultAdminApp**, **DefaultClientApp**, and **DefaultSandboxApp** you can enter a hexadecimal value, random values can be generated [here](https://www.guidgenerator.com/online-guid-generator.aspx), braces shouldn't be included and you should use hyphens.

Once the above is done run the following command to instantiate the database:

`dotnet ef database update`

Once done the project will be ready to run.

There is also a related [Vue.js](https://vuejs.org/) administrative app which allows you to manage app licenses, [Sudoku Collective Admin Vue](https://github.com/Joseph-Anthony-King/SudokuCollective.Admin).  The installation instructions for that project can be reviewed in its [README](https://github.com/Joseph-Anthony-King/SudokuCollective.Admin/blob/master/README.md) file.
