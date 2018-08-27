#!/usr/bin/env bash
#shellcheck disable=SC2154,SC2086,SC2046,SC2003,SC2034
#################################################
#Discord Chat Exporter macOS/Linux CLI Shell 1.0#
# Tested on High Sierra, Debian 9 and Ubuntu 18 #
#        Using DiscordChatExporter 2.6.         #
#################################################

# Set Script's directory, if not able, abort
cd "${0%/*}" || exit
# Set Text Color White and Background Black
printf '\e[0;40;97m'
clear

# Display Menu
cat <<-'ENDCAT'
===============================================
Discord Chat Exporter macOS/Linux CLI Shell 1.0
     github.com/Tyrrrz/DiscordChatExporter
===============================================
For additional export options, run "mono DiscordChatExporter.Cli.exe".

[1] Export using User Token
[2] Export using Bot Token

[3] User Multiple Export (Dark HTML)
[4] Bot Multiple Export (Dark HTML)

[5] Install requirements
[6] Download/Update Chat Exporter

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
    echo "Please download DiscordChatExporter.Cli.exe using option [6] Download"
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
        printf '\e[1;40;91m'
        echo "Error. Make sure Mono is installed and Token/Channel is valid."
        else
        echo "Done!"
        fi

###############  # #
#If reply is 2#  # #
###############  # #
elif [[ "$REPLY" == 2 ]]; then
    if [ ! -f DiscordChatExporter.Cli.exe ]; then
    echo "Please download DiscordChatExporter.Cli.exe using option [6] Download"
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
if ! mono "DiscordChatExporter.Cli.exe" -b $Token -c $Channel; then
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
    echo "Please download DiscordChatExporter.Cli.exe using option [6] Download"
    exit
    fi
    clear
    echo "[3] User Multiple Export (Dark HTML)"
    echo "Export different channels using the same User Token"
    echo "Control+C to quit."
    echo ""
    echo "User Token:"
    read -r Token
    while (( !quit )); do
    echo "Control+C to quit."
    echo "Channel ID:"
    read -r Channel
    mono "DiscordChatExporter.Cli.exe" -t $Token -c $Channel
    done

###############  # #   #
#If reply is 4#  #  # #
###############  #   #
elif [[ "$REPLY" == 4 ]]; then
    if [ ! -f DiscordChatExporter.Cli.exe ]; then
    echo "Please download DiscordChatExporter.Cli.exe using option [6] Download"
    exit
    fi
    clear
    echo "[4] Bot Multiple Export (Dark HTML)"
    echo "Export different channels using the same Bot Token"
    echo "Control+C to quit."
    echo ""
    echo "Bot Token:"
    read -r Token
    while (( !quit )); do
    echo "Control+C to quit."
    echo "Channel ID:"
    read -r Channel
    mono "DiscordChatExporter.Cli.exe" -b $Token -c $Channel
    done

###############  #   #
#If reply is 5#   # #
###############    #

elif [[ "$REPLY" == 5 ]]; then
    clear
    echo "[5] Install requirements"
    echo "Control+C to quit."
    ################  # #
    #If OS is macOS#   #
    ################  # #
    if [ "$(uname)" == "Darwin" ]; then
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
                        # Check if Command Line Tools is installed
                            if ! xcode-select --install; then
                            # If it is installed, try to download and install brew
                            /usr/bin/ruby -e "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/master/install)"
                            # After trying to install brew, try to install mono and wget using brew
                            brew install mono wget
                            echo "Run script again."
                            exit
                            else
                            echo "Install Command Line Tools and run Installation again."
                            fi
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
    elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
            read -r -p "Install Mono and Curl? Make sure you're root. [y/n]"
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                if [ "$EUID" -ne 0 ]
                    then echo "Please run as root."
                    exit
                fi
                distro=$(awk -F= '/^NAME/{print $2}' /etc/os-release)
                    if [[ $distro = *"Debian"* ]]; then
                    echo "Debian root detected. Installing..."
                    apt -y install apt-transport-https dirmngr
                    apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
                    echo "deb https://download.mono-project.com/repo/debian stable-stretch main" | tee /etc/apt/sources.list.d/mono-official-stable.list
                    apt update
                    apt -y install mono-devel curl
                    echo "Mono and Curl were installed. Re-run script"
                    exit
                    elif [[ $distro = *"Ubuntu"* ]]; then
                    echo "Ubuntu root detected. Installing..."
                    sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
                    echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
                    sudo apt update
                    sudo apt -y install mono-devel curl
                    echo "Mono and Curl were installed. Re-run script."
                    exit
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
###############  #   # #
#If reply is 6#   # #  #
###############    #   #
elif [[ "$REPLY" == 6 ]]; then
        ################  # #
        #If OS is macOS#   #
        ################  # #
    if [ "$(uname)" == "Darwin" ]; then
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
    elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
# Check if curl is installed
        if ! type curl; then
# If curl is not installed, exit
            echo "Curl is required. Please install using 'apt install curl'"
            exit
        fi
    fi
    clear
    echo "[6] Download/Update Chat Exporter"
    echo "Control+C to quit."
    echo ""
    echo "The latest DiscordChatExporter.zip.CLI.zip will be downloaded"
    echo "Make sure this script is inside a folder and that dependencies are installed."
    echo "Script path: $PWD"
    echo ""
    echo "Press ENTER to confirm or Control+C to quit."
    read -r ok
# Get browser_download_url file that has .CLI.zip from Github API
        curl https://api.github.com/repos/Tyrrrz/DiscordChatExporter/releases/latest \
        | grep "browser_download_url.*CLI.zip" \
        | sed -E 's/.*"([^"]+)".*/\1/' \
        | wget -qi -
        unzip -o DiscordChatExporter.CLI.zip
# Remove .zip
        rm -rf "DiscordChatExporter.CLI.zip"
        echo "Done!"
    exit
###########################
#If reply is invalid, exit#
###########################
else
    echo "Exiting..."
fi
exit