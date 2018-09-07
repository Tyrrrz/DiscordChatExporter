#!/usr/bin/env bash
#shellcheck disable=SC2154,SC2086
#################################################
#  Discord Chat Exporter macOS/Linux CLI Shell  #
#     github.com/Tyrrrz/DiscordChatExporter     #
#################################################

# Detect OS
unameOut="$(uname -s)"
case "${unameOut}" in
    Linux*)     OS=Linux;;
    Darwin*)    OS=Mac;;
    *)          OS=UNKNOWN
esac

# If OS in unknown, ask if user is running Linux
if [[ "$OS" == "UNKNOWN" ]]; then
echo "Not able to detect OS. Are you running Linux? [y/n]"
read -r -p ""
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        OS=Linux
        # If reply is not Yes, abort
        else
        echo "Not able to detect OS. Aborting..."
        exit
        fi
fi

# Set Script's directory, if not able, abort
cd "${0%/*}" || exit
clear

# Display Menu
cat <<-'ENDCAT'
===========================================
Discord Chat Exporter macOS/Linux CLI Shell
   github.com/Tyrrrz/DiscordChatExporter
===========================================
For additional export options, run "mono DiscordChatExporter.Cli.exe".

[1] Export using User Token
[2] Export using Bot Token

[3] Multiple Export

[4] Install requirements
[5] Download/Update Chat Exporter
[6] Schedule Exports with Cron (EXPERT)

[0] Quit
ENDCAT
echo ""
read -r -p ""

###############  #
#If reply is 1#  #
###############  #
if [[ "$REPLY" == 1 ]]; then
    #If CLI.exe is not found
    if [ ! -f DiscordChatExporter.Cli.exe ]; then
    echo "Please download DiscordChatExporter.Cli.exe using option [5] Download"
    exit
    fi
    clear
    echo "[1] Export using User Token"
    echo "Control+C to quit."
    echo ""
    echo "User Token:"
    read -r Token
    clear
    echo ""
    echo "Discord Channel ID:"
    read -r Channel
    clear
    echo "Exporting..."
# If mono fails
if ! mono "DiscordChatExporter.Cli.exe" -t $Token -c $Channel; then
        echo "Error. Make sure Mono is installed and Token/Channel is valid."
        else
        echo "Done!"
        fi

###############  # #
#If reply is 2#  # #
###############  # #
elif [[ "$REPLY" == 2 ]]; then
    if [ ! -f DiscordChatExporter.Cli.exe ]; then
    echo "Please download DiscordChatExporter.Cli.exe using option [5] Download"
    exit
    fi
    clear
    echo "[2] Export using Bot Token"
    echo "Control+C to quit."
    echo ""
    echo "Bot Token:"
    read -r Token
    echo ""
    echo ""
    echo "Discord Channel ID:"
    read -r Channel
    clear
    echo "Exporting..."
if ! mono "DiscordChatExporter.Cli.exe" -t $Token -b -c $Channel; then
        printf '\e[1;40;91m'
        echo "Error. Make sure Mono is installed and Token/Channel is valid."
        else
        echo "Done!"
        fi



###############  # # #
#If reply is 3#  # # #
###############  # # #
elif [[ "$REPLY" == 3 ]]; then
    if [ ! -f DiscordChatExporter.Cli.exe ]; then
    echo "Please download DiscordChatExporter.Cli.exe using option [5] Download"
    exit
    fi
    clear
    echo "[3] Multiple Export"
    echo "Export different channels using the same Token"
    echo "Control+C to quit."
    echo ""
    echo "Token:"
    read -r Token
    echo ""
    echo "Is token from a bot? [y/n]"
    read -r -p ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
        IsBotYes=-b
        fi
    echo ""
    echo ""
cat <<-'ENDCAT'

Export Format
[1] Plaintext TXT
[2] Html Dark Theme
[3] Html Light Theme
[4] CSV

Control+C to quit.
 
ENDCAT
read -r -p ""
if [[ "$REPLY" == 1 ]]; then
        Format=PlainText
elif [[ "$REPLY" == 2 ]]; then
        Format=HtmlDark
elif [[ "$REPLY" == 3 ]]; then
        Format=HtmlLight
elif [[ "$REPLY" == 4 ]]; then
        Format=Csv
else
        echo "Invalid option. Abort."
        exit
fi
    while (( !quit )); do
    echo "Control+C to quit."
    echo "Channel ID:"
    read -r Channel
    echo "Exporting... This may take a while."
    mono "DiscordChatExporter.Cli.exe" -t $Token $IsBotYes -c $Channel -f $Format
    done


###############  # #   #
#If reply is 4#  #  # #
###############  #   #

elif [[ "$REPLY" == 4 ]]; then
    clear
    echo "[4] Install requirements"
    echo "Control+C to quit."
    ################  # #
    #If OS is macOS#   #
    ################  # #
    if [[ "$OS" == Mac ]]; then
    if [ ! "$EUID" -ne 0 ]; then
    echo "Don't run as root"
    exit
    fi
        # Check if mono is installed
        if ! type mono; then
        # If not installed, prompt for installation using brew
            read -r -p "Install Mono and wget using Brew? [y/n]"
                # If reply is Yes
                if [[ $REPLY =~ ^[Yy]$ ]]; then
                # Try to install mono and wge
                    if ! brew install mono wget; then
                    # If it fails, try to install brew
                        echo "Is Brew installed?"
                        read -r -p "Install Brew? [y/n]"
                        # If reply is Yes
                        if [[ $REPLY =~ ^[Yy]$ ]]; then
                            /usr/bin/ruby -e "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/master/install)"
                            # After trying to install brew, try to install mono and wget using brew
                            brew install mono wget
                            echo "Mono and wget were installed. Run script again."
                            exit
                        else
                # If reply is No
                        echo "Please install Mono mono-project.com/download/"
                        exit
                        fi
                    else
    # If Brew is installed and is able to install Mono and wget
                    echo "Mono and wget were installed."
                    exit
                    fi
# If reply is no
                else
                echo "Please install Mono mono-project.com/download/"
                exit
                fi
        else
# If mono is already installed
### Notice that wget, brew and Command Line Tools won't be checked if mono is installed, since only mono is really necessary to run exporter. But user might not be able to run option [6] ###
        echo "Mono is already installed!"
        fi
    ################  #
    #If OS is Linux#  #
    ################  ###
    elif [[ "$OS" == Linux ]]; then
            read -r -p "Install Mono and Curl? Make sure you're root. [y/n]"
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                if [ "$EUID" -ne 0 ]
                    then echo "Please run as root."
                    exit
                fi
                distro=$(awk -F= '/^NAME/{print $2}' /etc/os-release)
                # If Distro is Debian
                    if [[ $distro = *"Debian"* ]]; then
                    echo "Debian root detected. Installing..."
                    apt -y install apt-transport-https dirmngr
                    apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
                    echo "deb https://download.mono-project.com/repo/debian stable-stretch main" | tee /etc/apt/sources.list.d/mono-official-stable.list
                    apt update
                    apt -y install mono-devel curl
                    echo "Mono and Curl were installed. Re-run script"
                    exit
                    # If Distro is Ubuntu
                    elif [[ $distro = *"Ubuntu"* ]]; then
                    echo "Ubuntu root detected. Installing..."
                    sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
                    echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
                    sudo apt update
                    sudo apt -y install mono-devel curl
                    echo "Mono and Curl were installed. Re-run script."
                    exit
                    # If not able to detect Linux Distro
                    else
                    echo "Failed to detect Linux Distro"
                    echo "Please install using the instructions on mono-project.com/download"
                    exit
            fi
            else
            echo "Canceled."
            exit
        fi
    fi
###############  #   #
#If reply is 5#   # #
###############    #
elif [[ "$REPLY" == 5 ]]; then
        ################  # #
        #If OS is macOS#   #
        ################  # #
    if [[ "$OS" == Mac ]]; then
# Check if wget is installed
        if ! type wget; then
# If not, try to install using brew
            if ! brew install wget; then
# If not able to install using brew, exit
            echo "Could not install wget using Brew. wget is required."
            exit
            fi
        fi
    ################  #
    #If OS is Linux#  #
    ################  ###
    elif [[ "$OS" == Linux ]]; then
# Check if curl is installed
        if ! type curl; then
# If curl is not installed, exit
            echo "Curl is required. Please install using 'apt install curl'"
            exit
        fi
    fi
    clear
    echo "[5] Download/Update Chat Exporter"
    echo "Control+C to quit."
    echo ""
    echo "The latest DiscordChatExporter.zip.CLI.zip will be downloaded"
    echo "Make sure this script is inside a folder and that dependencies are installed."
    echo "Script path: $PWD"
    echo ""
    echo "Press ENTER to confirm or Control+C to quit."
    read -r -p ""
# Get browser_download_url file that has .CLI.zip from Github API
        curl https://api.github.com/repos/Tyrrrz/DiscordChatExporter/releases/latest \
        | grep "browser_download_url.*CLI.zip" \
        | sed -E 's/.*"([^"]+)".*/\1/' \
        | wget -qi -
        unzip -o "DiscordChatExporter.CLI.zip"
# Remove .zip
        rm -rf "DiscordChatExporter.CLI.zip"
        echo "Done!"
    exit
    
###############  #   # #
#If reply is 6#   # #  #
###############    #   #

elif [[ "$REPLY" == 6 ]]; then
 # Check if cron.sh exists on script's directory
 if [ -f cron.sh ]; then
 echo "cron.sh already exists. Have you run this tool before?"
 echo "To schedule more exports: duplicate and edit cron.sh accordingly, then run 'crontab -e' as root and edit the file."
 echo "More info about editing cron.sh here:"
 echo "https://github.com/Tyrrrz/DiscordChatExporter/wiki"
 fi
 
 if [ "$EUID" -ne 0 ]
   then echo "Please run as root."
   exit
 fi
     if [ ! -f DiscordChatExporter.Cli.exe ]; then
    echo "Please download DiscordChatExporter.Cli.exe using option [5] Download"
    exit
    fi
    
 
cat <<-'ENDCAT'

Wel'll be using Crontab to schedule the exports.
We don't take any responsibility and we aren't liable for any damage caused through incorrect use of this tool.
If you're not sure how to use Crontab, exit with CONTROL+C.

More info and examples here:
https://github.com/Tyrrrz/DiscordChatExporter/wiki

Contrab time format:
* * * * *

*    *    *    *    *
MIN HOUR DAY MONTH DOW

Please specify:
ENDCAT
 echo "Minute (0-59)"
 read -r Minute
 echo "Hour (0-23)"
 read -r Hour
 echo "Day (1-31)"
 read -r Day
 echo "Month (1-12)"
 read -r Month
 echo "Day of Week (0-6 or SUN, MON...)"
 read -r Week
 echo ""
 echo "$Minute $Hour $Day $Month $Week"
 if [[ -z $Minute || -z $Hour || -z $Day || -z $Month || -z $Week ]]; then
 echo "Cannot be blank. Aborting..."
 exit
 fi

 echo "You can verify it at https://crontab.guru/"
read -r -p "Is the time right? [y/n]"
 
 if [[ ! $REPLY =~ ^[Yy]$ ]]; then
 echo "Exiting..."
 exit
 fi
 clear
echo "Insert Token:"
    read -r Token
    clear
echo "Is token from a bot? [y/n]"
    read -r -p ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
        TTYPE=BOT
        else
        TTYPE=USER
        fi
        clear
echo "Discord Channel ID:"
    read -r Channel
    
    clear
    
cat <<-'ENDCAT'

Export Format
[1] Plaintext TXT
[2] Html Dark Theme
[3] Html Light Theme
[4] CSV

Control+C to quit.
 
ENDCAT
read -r -p ""
if [[ "$REPLY" == 1 ]]; then
        Format=PlainText
elif [[ "$REPLY" == 2 ]]; then
        Format=HtmlDark
elif [[ "$REPLY" == 3 ]]; then
        Format=HtmlLight
elif [[ "$REPLY" == 4 ]]; then
        Format=Csv
else
        echo "Invalid option. Abort."
        exit
fi
    
clear
echo ""
echo "Put \ before spaces"
echo "E.g. /home/user/My\ Documents/Discord\ Exports"
echo ""
echo "Export path without quotes:"
echo""
    read -r Directory
    clear
if [ ! -d "$Directory" ]; then
echo "Directory doesn't exist."
echo "Would you like me to create it at $Directory? [y/n]"
read -r -p ""
if [[ $REPLY =~ ^[Yy]$ ]]; then
mkdir -p $Directory
fi
fi
  echo "Filename:"
  read -r Filename
  clear
    
echo "Token: $Token"
echo "$TTYPE"
echo "Channel ID: $Channel"
echo "EXE is at: $PWD/DiscordChatExporter.Cli.exe"
echo "Export path is: $Directory"
echo "Export file name is: $Filename"
echo "Export format is: $Format"
read -r -p "Is that right? [y/n]"
 if [[ ! $REPLY =~ ^[Yy]$ ]]; then
 echo "Exiting..."
 exit
 fi
 clear
read -r -p "Create cron.sh with the data above and script at $PWD? [y/n]"
 if [[ ! $REPLY =~ ^[Yy]$ ]]; then
 echo "Exiting..."
 exit
 fi

###################################Create file###################################
echo -e "#!/bin/bash
# Info: https://github.com/Tyrrrz/DiscordChatExporter
TOKEN=$Token
TOKENTYPE=$TTYPE
CHANNEL=$Channel
EXEPATH=$PWD
FILENAME=$Filename
EXPORTDIRECTORY=$Directory
EXPORTFORMAT=$Format
# Available export formats: PlainText, HtmlDark, HtmlLight, Csv"  > cron.sh

cat >> cron.sh <<'ENDCAT'

cd $EXEPATH || exit

if [[ "$TOKENTYPE" == "BOT" ]]; then
ISBOTYES=-b
fi

PATH=/Library/Frameworks/Mono.framework/Versions/Current/bin/:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin
CURRENTTIME=`date +"%Y-%m-%d-%H-%M-%S"`
mono DiscordChatExporter.Cli.exe -t $TOKEN $ISBOTYES -c $CHANNEL -f $EXPORTFORMAT -o exporttmp

if [[ "$EXPORTFORMAT" == "PlainText" ]]; then
        mv "exporttmp" $EXPORTDIRECTORY/$FILENAME-$CURRENTTIME.txt
        
elif [[ "$EXPORTFORMAT" == "Html"* ]]; then
        mv "exporttmp" $EXPORTDIRECTORY/$FILENAME-$CURRENTTIME.html

elif [[ "$EXPORTFORMAT" == "Csv" ]]; then
        mv "exporttmp" $EXPORTDIRECTORY/$FILENAME-$CURRENTTIME.csv
else
exit
fi
exit
ENDCAT
#################################################################################

echo "cron.sh created!"
echo "Setting 755 permission to it..."
if ! chmod 755 cron.sh; then
echo "Something went wrong when setting the permission."
echo "Try to set it by running 'chmod 755 cron.sh' as root."
echo "Run 'crontab -e' as root and paste this at the end of the file:"
echo "$Minute $Hour $Day $Month $Week $PWD/cron.sh >/var/log/discordchatexporter.log 2>&1" 
fi
clear
read -r -p "/var/spool/cron/crontabs/root will be edited. Is that ok? [y/n]"
 if [[ $REPLY =~ ^[Yy]$ ]]; then
(crontab -l 2>/dev/null; echo "$Minute $Hour $Day $Month $Week $PWD/cron.sh >/tmp/discordexporter.log 2>/tmp/discordexportererror.log") | crontab -
clear
 echo "Schedule should now be set up."
 echo "Check by running 'crontab -l' as root."
 echo "Exiting..."
 exit
 else 
 echo "Exiting..."
 exit
fi

###########################
#If reply is invalid, exit#
###########################
else
    echo "Exiting..."
fi
exit
