# GoFile-DL
Simple Tool to download GoFile albums with Password support, written in C#.

# Setup
Head to https://github.com/sim0n00ps/GoFile-DL/releases and download the latest release.

Extract the zip and you should have 3 files, GoFile DL.exe, Config.json and Links.txt.

You should be able to double click GoFile DL.exe and a new GoFile account will be created, you can check if this was successful by checking config.json, `Token` and `SiteToken` should both have values.

You can also use your own GoFile API token if you already have a GoFile acccount by heading to https://gofile.io/myProfile and copying the API token found at the bottom of the page into Config.json, set the `Token` value to your API token!

# Usage
Once the Config is set up, double click GoFile DL.exe and you have 3 options:

### Download From Single URL
This option allows you to enter a single GoFile link e.g https://gofile.io/d/xxxxxx and the content will be downloaded to the `Downloads` folder.

### Batch Download URLS
This option allows you to download from multiple GoFile links that should be put Links.txt, each link should be put on a new line e.g:

![image](https://github.com/sim0n00ps/GoFile-DL/assets/132307467/d763ea2b-5dcd-4ab4-9b88-d101096f4ccf)

### Exit
Exit will kill the program.

# Donations
If you would like to donate then here is a link to my ko-fi page https://ko-fi.com/sim0n00ps. Donations are not required but are very much appreciated:)
