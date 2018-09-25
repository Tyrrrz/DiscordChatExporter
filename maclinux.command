#!/usr/bin/env bash
#shellcheck disable=SC2086

# DiscordChatExporter CLI Shell - Release 1 (2.7)
# github.com/Tyrrrz/DiscordChatExporter

### Script Steps ###
# Set Directory > Set PATH > Detect OS > Check curl, jq and wget > Check Cli.exe > Check Mono > Display Menu

### Variables ###
# ${Token//\"} (//\" removes the character \, \ escapes it), $Channel
# $Format for file format (-f), $FormatNumber for file format number
# $Filename for filename, $Directory for export directory
# $directoryoptional (-o $directory), $afterdateoptional (--after $afterdate), $beforedateoptional, $dateformatoptional, $grouplimitoptional
#
# $ISBOTYES (Yes - Sets -b), $EXEFOUND for DiscordChatExporter.exe (No)
# $TTYPE for Token type (USER/BOT), $TOKENTYPE (0 for User, 1 for Bot)
#
# $OS for OS detection (Linux/Mac/UNKNOWN), $unameOut for uname command output
# $distro, $distroversion for distro detection
# $LINUX_DEPS, $MAC_DEPS, $FILE_DEPS for Dependencies
#
# Cron-related
# $CRONTIME that has $Minute $Hour $Day $Month $Week, all on cron format
#
# Unused
# $distroversionshort (parses xx.yy.zz to xx.yy)

### Functions ###
# File creation - createsettings (create the same Settings.dat as 2.7 GUI), downloadzip
# Display - exportformat
# Conversion - formattoraw, rawtoformat, rawtottype, ttypetoraw

### Extra Info ###
# Linux Mono auto-install is compatible with Debian 9 and 8, Ubuntu 18, 16 and 14, CentOS 6 and 7, and Fedora 27 and 28
# TTYPE means Token Type. If it's from a BOT or not.

##### Set Script's directory, if unable, abort #####
cd "${0%/*}" || exit
##### Set PATH  so Mono can be found #####
PATH=/Library/Frameworks/Mono.framework/Versions/Current/bin/:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin

##### Detect kernel with uname and set output to $unameOut, parse it and set $OS #####
unameOut="$(uname -s)"
case "${unameOut}" in
    Linux*)     OS=Linux;;
    Darwin*)    OS=Mac;;
    *)          OS=UNKNOWN
esac

# If OS is UNKNOWN, exit
if [[ "$OS" == "UNKNOWN" ]]; then
  echo "Unable to detect Kernel. Exiting..."
  exit
fi

# If OS is Linux
if [[ "$OS" == "Linux" ]]; then
  # Detect distro and version
  # If distro has lsb_release command, like Debian or Ubuntu
if type lsb_release >/dev/null 2>&1; then
    distro=$(lsb_release -si)
    distroversion=$(lsb_release -sr)
#    distroversionshort=$(lsb_release -sr | cut -f1-2 -d ".")
# If distro doesn't have lsb_release but has redhat-release file. CentOS and Fedora.
elif [ -f /etc/redhat-release ]; then
      distro=$(cat < /etc/redhat-release | awk '{print $1;}')
      distroversion=$(cat < /etc/redhat-release | grep -o '[0-9]\+')
  else
    echo "Unable to detect distro version. Exiting..."
  exit
fi
fi

##### Script Dependencies #####
MAC_DEPS="curl jq wget"
# CentOS also requires 'epel-release' so jq can be installed, but it's not on this list so it's not unnecessarily installed on other OSs
LINUX_DEPS="curl jq sudo unzip wget"
# Could be used to verify files by adding '-f FILE1 && -f FILE2', but it will be kept like this so it's future proof
FILE_DEPS="DiscordChatExporter.Cli.exe"

##### Check Functions #####
# Check if github.com is reachable #
# In case users runs script offline, it exits before adding apt keys and trying to install Mono
checkinternet() {
if wget -q --spider https://github.com/; then
    echo "github.com is reachable."
else
    echo "[checkinternet] Unable to reach github.com."
    echo "Please check your internet connection."
    echo "Exiting..."
    exit
fi
}

##### File Creation Functions #####
# Get lastest browser_download_url that has .CLI.zip filename. Download, Extract and Delete it
downloadzip() {
  checkinternet
  curl https://api.github.com/repos/Tyrrrz/DiscordChatExporter/releases/latest \
  | grep "browser_download_url.*CLI.zip" \
  | sed -E 's/.*"([^"]+)".*/\1/' \
  | wget -qi -
  echo "Unzipping..."
  if ! unzip -o -qq "DiscordChatExporter.CLI.zip"; then
    echo "[downloadzip] Something went wrong with the unzipping."
  fi
  echo "Cleaning up..."
  if ! rm -Rf "DiscordChatExporter.CLI.zip"; then
  echo "[downloadzip] Unable to delete DiscordChatExporter.CLI.zip."
fi
  echo "Done!"
      }

# Create Settings.dat function
createsettings() {
  echo "Let's create Settings.dat"
  echo "You can edit this later by selecting '[6] Quick Export Settings'"
  echo ""
  echo "Insert your Token:"
  read -r Token
  echo "Is it a Bot Token? [Y/n]"
  read -r -p ""
      if [[ $REPLY =~ ^[Yy]$ ]]; then
      TOKENTYPE=1
      else
      TOKENTYPE=0
      fi
      clear
  exportformat
  formattoraw
clear
echo "Your token is: ${Token//\"}"
echo ""
echo "The Export Format is: $Format"
echo ""
echo "Settings.dat will be created at $PWD"
echo "You can edit this later by selecting '[6] Quick Export Settings'"
echo ""
echo "Press ENTER to confirm or Control+C to quit."
read -r -p ""
echo -e "{
  \"IsAutoUpdateEnabled\": true,
  \"DateFormat\": \"dd-MMM-yy hh:mm tt\",
  \"MessageGroupLimit\": 20,
  \"LastToken\": {
    \"Type\": $TOKENTYPE,
    \"Value\": \"${Token//\"}\"
  },
  \"LastExportFormat\": $RawFormat
}" > Settings.dat
clear
echo "Settings.dat created!"
}

##### Display Functions #####
# Display Export Format Menu function
exportformat() {
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
          echo "[exportformat] Invalid option. Exiting..."
          exit
  fi
}

##### Conversion Functions #####
# Export Format to Raw function
formattoraw() {
  if [[ "$Format" == PlainText ]]; then
          RawFormat=0
  elif [[ "$Format" == HtmlDark ]]; then
          RawFormat=1
  elif [[ "$Format" == HtmlLight ]]; then
          RawFormat=2
  elif [[ "$Format" == Csv ]]; then
          RawFormat=3
  else
    echo "[formattoraw] Invalid option. Exiting..."
    exit
  fi
}

# Raw to Export Format function
rawtoformat() {
  if [[ "$RawFormat" == 0 ]]; then
          Format=PlainText
  elif [[ "$RawFormat" == 1 ]]; then
          Format=HtmlDark
  elif [[ "$RawFormat" == 2 ]]; then
          Format=HtmlLight
  elif [[ "$RawFormat" == 3 ]]; then
          Format=Csv
  else
    echo "[rawtoformat] Invalid option. Exiting..."
    exit
  fi
}

# Token Type to Raw function
ttypetoraw() {
if [[ "$TTYPE" == "USER" ]]; then
TTYPERAW=0
elif [[ "$TTYPE" == "BOT" ]]; then
TTYPERAW=1
else
echo "[ttypetoraw] Invalid option. Exiting..."
exit
fi
}

# Raw to Token Type function
rawtottype() {
if [[ "$TTYPERAW" == 0 ]];then
TTYPE=USER
elif [[ "$TTYPERAW" == 1 ]];then
TTYPE=BOT
else
echo "[rawtottype] Invalid option. Exiting..."
exit
fi
}
#####

# Check if curl is installed
# if [[ -x "$(command -v curl jq wget)" ]]; then
if ! type curl jq wget &>/dev/null; then
  checkinternet
  clear
echo "Some dependencies are missing. Installing..."
# Check if OS is macOS
        if [[ "$OS" == "Mac" ]]; then
          echo "Running macOS"
# Check if User is root
                if [ ! "$EUID" -ne 0 ]; then
                echo "Don't run as root."
                exit
                fi
# If Homebrew is not installed
            if ! type brew &>/dev/null; then
# Ask if user wishes to install it. Also tell about MacPorts or Finx.
            echo "This tool uses Homebrew, which wasn't found."
            echo "If you have MacPorts or Finx, try installing '$MAC_DEPS'."
            echo "Press ENTER to install Homebrew or Control+C to quit."
            read -r -p ""
            echo "You will be prompted for your password."
            sleep 3
            /usr/bin/ruby -e "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/master/install)"
            fi
# Install deps using Homebrew
            brew install $MAC_DEPS
# Check if OS is Linux
        elif [[ "$OS" == Linux ]]; then
# Check if user is root
          if [ "$EUID" -ne 0 ]; then
            clear
            echo "Please run as root to install dependencies."
            exit
          fi
#          if [[ -x "$(command -v apt)" ]]; then
          if type apt &>/dev/null; then
    echo "Running $distro with apt"
    # Update apt
    echo "Updating apt..."
          apt -qq update
    # Install Dependencies
    echo "Installing dependencies..."
          apt -qq -y install $LINUX_DEPS
          clear
          elif [[ $distro == "CentOS" ]]; then
          echo "Running CentOS"
          yum -y install epel-release
          if ! yum -y install $LINUX_DEPS; then
            echo "Something went wrong with 'yum install $LINUX_DEPS'"
            echo "Exiting..."
            exit
          fi
        elif [[ $distro == "Fedora" ]]; then
          echo "Running Fedora"
          if ! dnf -y install $LINUX_DEPS; then
          echo "Something went wrong with 'dnf install $LINUX_DEPS'"
          echo "Exiting..."
          exit
          fi
        else
          echo "You are running Linux, and your distro is $distro version $distroversion."
          echo "But we couldn't install $LINUX_DEPS using apt."
          echo "Please install them with your package manager."
          exit
      fi
    fi
fi
# Check if Mono is installed
if ! type mono &>/dev/null; then
  checkinternet
  clear
  echo "Mono doesn't appear to be installed."
  echo "Installing it will take a while."
  echo "Press ENTER to install or Control+C to quit."
  read -r -p ""
  clear
  echo "This will take a while..."
# If OS is macOS
if [[ "$OS" == "Mac" ]]; then
    if [ ! "$EUID" -ne 0 ]; then
    echo "Don't run as root when installing Mono."
    exit
    fi
brew install mono
# If OS is Linux
elif [[ "$OS" == "Linux" ]]; then
    if [ "$EUID" -ne 0 ]; then
    clear
    echo "Please run as root to install Mono."
    exit
    fi
# If Distro is Debian
    if [[ $distro = *"Debian"* ]]; then
# Try to run raspi-config to check if OS is Raspbian
#        if [[ ! -x "$(command -v raspi-config)" ]]; then
      if [[ $(uname -m) = *"arm"* ]]; then
      echo "CPU architecture is ARM"
      if type raspi-config &>/dev/null; then
        echo "Running Raspbian with ARM CPU"
        # If CPU is arm and OS is Raspbian Stretch 9
                  if [[ $distroversion = "9"* ]]; then
                    echo "Raspbian Stretch 9"
                    sudo apt install apt-transport-https dirmngr
                    sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
                    echo "deb https://download.mono-project.com/repo/debian stable-raspbianstretch main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
       # If CPU is arm and OS is Raspbian Jessie 8
                  elif [[ $distroversion = "8"* ]]; then
                    echo "Raspbian Jessie 8"
                    sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
                    sudo apt install apt-transport-https
                    echo "deb https://download.mono-project.com/repo/debian stable-raspbianjessie main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
                  fi
      else
                    echo "CPU is ARM but distro is not Raspbian. It is $distro."
                    echo "Please follow the instructions at mono-project.com/download"
      fi
      else
     echo "CPU architecture isn't ARM"
   if [[ $distroversion = "9"* ]]; then
     echo "Debian 9"
     apt -y install apt-transport-https dirmngr
     apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
     echo "deb https://download.mono-project.com/repo/debian stable-stretch main" | tee /etc/apt/sources.list.d/mono-official-stable.list
# If Debian version is 8
     elif [[ $distroversion = "8"* ]]; then
     echo "Debian 8"
     sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
     sudo apt install apt-transport-https
     echo "deb https://download.mono-project.com/repo/debian stable-jessie main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
# If unable to detect distro version
     else
     echo "Unable to detect $distro version."
     echo "Please follow the instructions at mono-project.com/download"
     exit
     fi
       echo "Keys to $OS $distro were added. Trying to install Mono..."
       apt update
       # Won't be using -qq since mono takes time to install, so the user knows something is happening
       if ! apt -y install mono-devel; then
       echo "Something went wrong with 'apt install mono-devel'"
       echo "Please follow the instructions at mono-project.com/download"
       fi
     fi
# If OS is Ubuntu
    elif [[ $distro = *"Ubuntu"* ]]; then
# If Ubuntu version is 18
      if [[ $distroversion = "18"* ]]; then
      echo "Ubuntu 18"
      sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
      echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
# If Ubuntu version is 16
      elif [[ $distroversion = "16"* ]]; then
       echo "Ubuntu 16"
       sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
       sudo apt install apt-transport-https
       echo "deb https://download.mono-project.com/repo/ubuntu stable-xenial main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
      elif [[ $distroversion = "14"* ]]; then
       echo "Ubuntu 14"
       sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
       sudo apt install apt-transport-https
       echo "deb https://download.mono-project.com/repo/ubuntu stable-trusty main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
      else
# If unable to detect distro version
      clear
      echo "Unable to detect $distro version."
      echo "Please follow the instructions at mono-project.com/download"
      exit
     fi
echo "Keys to $OS $distro were added. Installing Mono..."
echo "Please be aware this will take a while."
sleep 3
apt update
# Won't be using -qq since mono takes time to install, so the user knows something is happening
if ! apt -y install mono-devel; then
echo "Something went wrong with 'apt install mono-devel'"
echo "Please follow the instructions at mono-project.com/download"
echo "Exiting..."
exit
fi
# If distro is CentOS
elif [[ $distro = *"CentOS"* ]]; then
# If CentOS is version 7
  if [[ $distroversion = "7"* ]]; then
    echo "CentOS 7"
    rpm --import "https://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
    su -c 'curl https://download.mono-project.com/repo/centos7-stable.repo | tee /etc/yum.repos.d/mono-centos7-stable.repo'
# If CentOS is version 6
  elif [[ $distroversion = "6"* ]]; then
    echo "CentOS 6"
    rpm --import "https://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
    su -c 'curl https://download.mono-project.com/repo/centos6-stable.repo | tee /etc/yum.repos.d/mono-centos6-stable.repo'
# Is CentOS version doesn't match any of the above
  else
    clear
    echo "CentOS version is $distroversion."
    echo "Please follow the instructions at mono-project.com/download"
    echo "Exiting..."
    exit
  fi
  if ! yum -y install mono-devel; then
    echo "Something went wrong with 'yum install mono-devel'"
    echo "Please follow the instructions at mono-project.com/download"
    echo "Exiting..."
    exit
  fi

# If distro is Fedora
elif [[ $distro = *"Fedora"* ]]; then
  if [[ $distroversion = "28"* ]]; then
  echo "Fedora 28"
  echo "As of late-2018, Mono's website doesn't have install instructions for Fedora 28. Attempting to install with v27 parameters..."
  sleep 3
  rpm --import "https://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
  su -c 'curl https://download.mono-project.com/repo/centos7-stable.repo | tee /etc/yum.repos.d/mono-centos7-stable.repo'
  dnf update
  if ! dnf -y install mono-devel; then
    echo "Something went wrong with 'dnf install mono-devel'"
    echo "Please follow the instructions at mono-project.com/download"
    echo "Exiting..."
    exit
  fi
# If Fedora is version 27
elif [[ $distroversion = "27"* ]]; then
  echo "Fedora 27"
  rpm --import "https://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
  su -c 'curl https://download.mono-project.com/repo/centos7-stable.repo | tee /etc/yum.repos.d/mono-centos7-stable.repo'
  dnf update
  if ! dnf -y install mono-devel; then
    echo "Something went wrong with 'dnf install mono-devel'"
    echo "Please follow the instructions at mono-project.com/download"
    echo "Exiting..."
    exit
  fi
# If Fedora version doesn't match any of the above
else
  clear
  echo "Fedora version is $distroversion."
  echo "Please follow the instructions at mono-project.com/download"
  echo "Exiting..."
  exit
fi
   else
# If $distro doesn't match any of the above
    clear
    echo "Unable to detect $OS distro."
    echo "Please follow the instructions at mono-project.com/download"
    echo "Exiting..."
    exit
   fi
 fi
fi

# Check if required files exist
if [[ ! ( -f $FILE_DEPS ) ]]; then
  clear
    echo "Some files are missing."
    echo "curl and wget will be used to download them."
    echo "Make sure this script is inside a folder."
    echo "Script's path: $PWD"
    echo ""
    echo "Press ENTER to confirm or Control+C to quit."
    read -r -p ""
    downloadzip
fi

# Display Menu
clear
cat <<-'ENDCAT'
===========================================
Discord Chat Exporter macOS/Linux CLI Shell
   github.com/Tyrrrz/DiscordChatExporter
===========================================

[1] Quick Export
[2] Custom Export
[3] Batch Export

[4] Lists

[5] Check For Update
[6] Quick Export Settings
ENDCAT
if [[ "$OS" == Linux ]]; then
  if type crontab &>/dev/null; then
  echo ""
	echo "[7] Schedule Exports with Cron (EXPERT)"
  fi
fi
echo ""
echo "[0] Quit"
echo ""
read -r -p ""

###############  #
#If reply is 1#  #
###############  #
if [[ "$REPLY" == 1 ]]; then
    clear
    echo "[1] Quick Export"
    echo "Control+C to quit."
    if [[ ! ( -f Settings.dat ) ]]; then
createsettings
clear
    fi
    Token="$(jq -r '.LastToken | .Value' Settings.dat )"
    TTYPERAW="$(jq -r '.LastToken | .Type' Settings.dat )"
    rawtottype
    if [[ "$TTYPE" == BOT ]]; then
      ISBOTYES=-b
    fi
    echo "Token: ${Token//\"}"
    echo "Token Type: $TTYPE"
    echo ""
    while true ; do
    echo "Control+C to quit."
    echo ""
    echo "Channel ID:"
    read -r Channel
    echo "Exporting..."
    if ! mono "DiscordChatExporter.Cli.exe" export -t ${Token//\"} $ISBOTYES -c $Channel -f $Format; then
            echo "[Quick Export] Something went wrong."
            echo "Exiting..."
            exit
            else
            echo ""
            fi
    done
    exit

###############  # #
#If reply is 2#  # #
###############  # #
elif [[ "$REPLY" == 2 ]]; then
    clear
    echo "[2] Custom Export"
    echo "Control+C to quit."
    echo ""
    echo "Token:"
    read -r Token
    echo ""
    echo "Is it a Bot Token? [Y/n]"
    read -r -p ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
          ISBOTYES=-b
        fi
        clear
    echo "Discord Channel ID:"
    read -r Channel
    clear
    exportformat
    clear
    echo "Output file path"
    echo "Leave it blank for $PWD"
    read -r Directory
    clear
    echo "Limit to messages sent after this date (MM-dd-yyyy)"
    echo "Leave it blank to skip"
    read -r afterdate
    clear
    echo "Limit to messages sent before this date (MM-dd-yyyy)"
    echo "Leave it blank to skip"
    read -r beforedate
    clear
    echo "Date format used in output"
    echo "More info about date formats at:"
    echo "https://github.com/Tyrrrz/DiscordChatExporter/wiki/Troubleshooting"
    echo "Leave it blank to default (dd-MMM-yyyy hh:mm tt)"
    read -r dateformat
    clear
    echo "Message group limit"
    echo "Leave it blank to unlimited"
    read -r grouplimit
    clear
    if [[ -n $Directory ]]; then
      directoryoptional="-o $Directory"
    fi
    if [[ -n $afterdate ]]; then
      afterdateoptional="--after $afterdate"
    fi
    if [[ -n $beforedate ]]; then
      beforedateoptional="--before $beforedate"
    fi
    if [[ -n $dateformat ]]; then
      dateformatoptional="--dateformat $dateformat"
    fi
    if [[ -n $grouplimit ]]; then
      grouplimitoptional="--grouplimit $grouplimit"
    fi
    echo "Exporting..."
    if ! mono DiscordChatExporter.Cli.exe export -t ${Token//\"} $ISBOTYES -c $Channel -f $Format $directoryoptional $afterdateoptional $beforedateoptional $dateformatoptional $grouplimitoptional; then
        echo "[Custom Export] Something went wrong."
        echo "Exiting..."
        exit
    else
        exit
    fi

###############  # # #
#If reply is 3#  # # #
###############  # # #
elif [[ "$REPLY" == 3 ]]; then
    clear
    echo "[3] Batch Export"
    echo "Export different channels using the same Token"
    echo "Control+C to quit."
    echo ""
    echo "Token:"
    read -r Token
    echo ""
    echo "Is it a Bot Token? [Y/n]"
    read -r -p ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
        ISBOTYES=-b
        fi
    echo ""
    echo ""
    clear
exportformat
clear
cat <<-'ENDCAT'
   Insert Channel IDs separated by lines
When finished, press ENTER, then Control+D
==========================================

ENDCAT
cat > batchexport.tmp
echo ""
echo "====================================="
echo "Exporting..."
while read -r Channel
do
    if ! mono DiscordChatExporter.Cli.exe export -t ${Token//\"} $ISBOTYES -c $Channel -f $Format; then
      echo "Something went wrong while trying to batch export."
    fi
done <batchexport.tmp
echo "Cleaning up..."
if ! rm -Rf "batchexport.tmp"; then
echo "[Batch Export] Unable to delete batchexport.tmp."
fi
echo "Done!"
exit

###############  # #   #
#If reply is 4#  #  # #
###############  #   #
elif [[ "$REPLY" == 4 ]]; then
clear
cat <<-'ENDCAT'
Get Lists

[1] Channel List
[2] DM List
[3] Guild List

[0] Quit

ENDCAT
read -r -p ""
if [[ "$REPLY" == 1 ]]; then
  clear
  echo "[1] Get the list of channels in the given guild"
  echo "Control+C to quit."
  echo ""
  echo "Token:"
  read -r Token
  echo ""
  echo "Is it a Bot Token? [Y/n]"
  read -r -p ""
    if [[ $REPLY =~ ^[Yy]$ ]]; then
    ISBOTYES=-b
    fi
    echo "Guild ID:"
    read -r guildid
    clear
  mono DiscordChatExporter.Cli.exe channels -t ${Token//\"} $ISBOTYES -g $guildid
  exit
elif [[ "$REPLY" == 2 ]]; then
  clear
echo "[2] Get the list of direct message channels"
echo "User Token only."
echo "Control+C to quit."
echo ""
echo "Token:"
read -r Token
echo ""
clear
mono DiscordChatExporter.Cli.exe dm -t ${Token//\"}
exit
elif [[ "$REPLY" == 3 ]]; then
  clear
echo "[3] Get the list of accessible guilds"
echo "Control+C to quit."
echo ""
echo "Token:"
read -r Token
echo ""
echo "Is it a Bot Token? [Y/n]"
read -r -p ""
  if [[ $REPLY =~ ^[Yy]$ ]]; then
  ISBOTYES=-b
  fi
  clear
mono DiscordChatExporter.Cli.exe guilds -t ${Token//\"} $ISBOTYES
exit
else
echo "Exiting..."
exit
fi

###############  #   #
#If reply is 5#   # #
###############    #
elif [[ "$REPLY" == 5 ]]; then
clear
  echo "[5] Check for Update"
  echo ""
  echo "You are running"
  mono DiscordChatExporter.Cli.exe --version
  latestversion=$(curl -s https://api.github.com/repos/Tyrrrz/DiscordChatExporter/releases/latest \
          | grep "tag_name" \
          | sed -E 's/.*"([^"]+)".*/\1/')
  echo ""
  echo "DiscordChatExporter $latestversion is the latest version."
  echo ""
  echo "Would you like to download it? [Y/n]"
  read -r -p ""
    if [[ $REPLY =~ ^[Yy]$ ]]; then
      downloadzip
    else
echo "DiscordChatExporter won't be updated."
echo "Exiting..."
exit
    fi

###############  #   # #
#If reply is 6#   # #  #
###############    #   #
elif [[ "$REPLY" == 6 ]]; then
  clear
  if [[ ! ( -f Settings.dat ) ]]; then
  createsettings
  fi
while true ; do
  Token="$(jq -r '.LastToken | .Value' Settings.dat )"
  TTYPERAW="$(jq -r '.LastToken | .Type' Settings.dat )"
  RawFormat="$(jq -r '.LastExportFormat' Settings.dat )"
  rawtottype
  rawtoformat
  clear
  echo "Select Option to Edit"
  echo "[1] Token: ${Token//\"}"
  echo "[2] Token Type: $TTYPE"
  echo "[3] Export Format: $Format"
  echo ""
  echo "[0] Quit"
  read -r -p ""
    if [[ "$REPLY" == 1 ]]; then
      echo "Current Token is: ${Token//\"}"
      echo "Insert New Value:"
      read -r Token
      echo "Is it a Bot Token? [Y/n]"
      read -r -p ""
          if [[ $REPLY =~ ^[Yy]$ ]]; then
            TTYPERAW=1
          else
            TTYPERAW=0
          fi
      jq --arg Token "${Token//\"}" '.LastToken.Value = $Token' Settings.dat > Settings.dat.tmp && mv Settings.dat.tmp Settings.dat
      jq --arg TTYPERAW "$TTYPERAW" '.LastToken.Type = $TTYPERAW' Settings.dat > Settings.dat.tmp && mv Settings.dat.tmp Settings.dat
    elif [[ "$REPLY" == 2 ]]; then
      echo "Is it a Bot Token? [Y/n]"
      read -r -p ""
          if [[ $REPLY =~ ^[Yy]$ ]]; then
            TTYPERAW=1
          else
            TTYPERAW=0
          fi
      jq --arg TTYPERAW "$TTYPERAW" '.LastToken.Type = $TTYPERAW' Settings.dat > Settings.dat.tmp && mv Settings.dat.tmp Settings.dat
elif [[ "$REPLY" == 3 ]]; then
  clear
  exportformat
  formattoraw
  jq --arg RawFormat "$RawFormat" '.LastExportFormat = $RawFormat' Settings.dat > Settings.dat.tmp && mv Settings.dat.tmp Settings.dat
else
echo "Exiting..."
exit
fi
done
exit

###############  #   # # #
#If reply is 7#   # #  # #
###############    #   # #
elif [[ "$REPLY" == 7 ]]; then
  clear
# If OS is macOS
  if [[ "$OS" == "Mac" ]]; then
    echo "Using Crontab is not recommended on macOS"
    echo "Please follow the proper instructions here:"
    echo "https://github.com/Tyrrrz/DiscordChatExporter/wiki"
    echo "Proceed at your own risk."
  fi
 # If cron.sh exists on script's directory
 if [ -f cron.sh ]; then
 echo "cron.sh already exists. Have you run this tool before?"
 echo "To schedule more exports: duplicate and/or edit cron.sh accordingly, then run 'crontab -e' as root and edit the file."
 echo "More info about editing cron.sh here:"
 echo "https://github.com/Tyrrrz/DiscordChatExporter/wiki"
 echo "Exiting..."
 exit
 fi
 # Check if user is root
 if [ "$EUID" -ne 0 ]
   then echo "Please run as root to create cron task."
   exit
 fi

# Display Crontab Assistant Menu
cat <<-'ENDCAT'
We'll be using Crontab to schedule the exports.
We don't take any responsibility and we aren't liable for any damage caused through incorrect use of this tool.
If you're not sure how to use Crontab, exit with CONTROL+C.

More info and examples here:
https://github.com/Tyrrrz/DiscordChatExporter/wiki

Crontab time format:

* * * * *
| | | | +----- Day of the Week (0 - 7)
| | | +------- Month (1 - 12)
| | +--------- Day (0 - 31)
| +----------- Hour (0 - 23)
+------------- Min (0 - 59)

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
 echo "Cannot be blank. Exiting..."
 exit
 fi
 echo "Verify at https://crontab.guru/"
read -r -p "Is the time right? [Y/n]"
# If reply is not yes
 if [[ ! $REPLY =~ ^[Yy]$ ]]; then
 echo "Exiting..."
 exit
 fi
 CRONTIME="$Minute $Hour $Day $Month $Week"
 clear
echo "Insert Token:"
    read -r Token
echo "Is it a Bot Token? [Y/n]"
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
exportformat
clear
echo ""
echo "Put \ before spaces"
echo "E.g. /home/user/My\ Documents/Discord\ Exports"
echo ""
echo "Export path without quotes:"
echo ""
read -r Directory
clear
# If directory doesn't exist, ask to create it
if [ ! -d "$Directory" ]; then
echo "Directory doesn't exist."
echo "Create it at $Directory? [Y/n]"
read -r -p ""
# If reply is yes, create it
if [[ $REPLY =~ ^[Yy]$ ]]; then
mkdir -p "$Directory"
# If reply isn't yes, exit
else
echo "Please create $Directory"
exit
fi
fi
  echo "Filename:"
  read -r Filename
  clear
echo "Token: ${Token//\"}"
echo "$TTYPE"
echo "Channel ID: $Channel"
echo "EXE is at: $PWD/DiscordChatExporter.Cli.exe"
echo "Export path: $Directory"
echo "Export file name: $Filename"
echo "Export format: $Format"
read -r -p "Is that ok? [Y/n]"
 if [[ ! $REPLY =~ ^[Yy]$ ]]; then
 echo "Exiting..."
 exit
 fi
 clear
read -r -p "Create cron.sh with the data above and script at $PWD? [Y/n]"
 if [[ ! $REPLY =~ ^[Yy]$ ]]; then
 echo "Exiting..."
 exit
 fi

################################# Create file #################################
echo -e "#!/bin/bash
# Info: https://github.com/Tyrrrz/DiscordChatExporter/wiki
TOKEN=${Token//\"}
TOKENTYPE=$TTYPE
CHANNEL=$Channel
EXEPATH=$PWD
FILENAME=${Filename//\"}
EXPORTDIRECTORY=${Directory//\"}
EXPORTFORMAT=$Format
# Available export formats: PlainText, HtmlDark, HtmlLight, Csv"  > cron.sh

cat >> cron.sh <<'ENDCAT'

cd $EXEPATH || exit

if [[ "$TOKENTYPE" == "BOT" ]]; then
ISBOTYES=-b
fi

PATH=/Library/Frameworks/Mono.framework/Versions/Current/bin/:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin

mono DiscordChatExporter.Cli.exe export -t ${TOKEN//\"} $ISBOTYES -c $CHANNEL -f $EXPORTFORMAT -o exporttmp

CURRENTTIME=`date +"%Y-%m-%d-%H-%M-%S"`

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
###############################################################################

echo "cron.sh created!"
echo "Setting 755 permission to it..."
if ! chmod 755 cron.sh; then
echo "Unable to set permission to cron.sh"
echo "Try running 'chmod 755 cron.sh' as root."
echo "Then run 'crontab -e' as root and, at the end of the file, paste this:"
echo "$CRONTIME $PWD/cron.sh >/var/log/discordchatexporter.log 2>/tmp/discordexportererror.log"
exit
fi
clear
read -r -p "/var/spool/cron/crontabs/root will be edited. Is that ok? [Y/n]"
 if [[ $REPLY =~ ^[Yy]$ ]]; then
(crontab -l 2>/dev/null; echo "$CRONTIME $PWD/cron.sh >/tmp/discordexporter.log 2>/tmp/discordexportererror.log") | crontab -
clear
 echo "Schedule should now be set up."
 echo "Check by running 'crontab -l' as root."
 else
   echo "Reverting changes and exiting..."
   rm -Rf "cron.sh"
 exit
fi

###########################
#If reply is invalid, exit#
###########################
else
    echo "Exiting..."
    exit
fi
exit
