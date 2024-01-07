#include "LC-Sync.h"

namespace LCSync {
    bool setup() {
        LCSCore* lcSync = new LCSCore();

        if (!lcSync->getLCInstallation() || lcSync->checkIfLCRunning()) return false;

        // Install BepInEx if missing
        if (!lcSync->checkFiles(lcSync->installPath, LCSyncConstants::bepInExFiles))
        {
            std::string bepInExExtraction = lcSync->downloadAndExtract(LCSyncConstants::bepInExURL);
            lcSync->move(bepInExExtraction + "\\BepInExPack", lcSync->installPath);
            lcSync->deleteLCSyncFolder();
        }

        // Install LCApi if missing
        std::vector<std::string> plDir = { "plugins" };
        if (!lcSync->checkFiles((lcSync->installPath + "\\BepInEx"), plDir))
        {
            std::string LCApiExtraction = lcSync->downloadAndExtract(LCSyncConstants::lcApiURL);
            lcSync->move(LCApiExtraction + "\\BepInEx", lcSync->installPath + "\\BepInEx");
            lcSync->deleteLCSyncFolder();
        }

        lcSync->savedKey = lcSync->readKeyFile();
        int selectedOption = 0;

        while (true) {
            lcSync->showMenu(selectedOption);

            char key = _getch();

            switch (key) {
            case 72: // Up arrow key
                selectedOption = (selectedOption - 1 + 4) % 4;
                break;
            case 80: // Down arrow key
                selectedOption = (selectedOption + 1) % 4;
                break;
            case 13: // Enter key
                switch (selectedOption) {
                case 0:
                    if (lcSync->savedKey != "") {
                        long status{};
                        std::string modString = lcSync->requestLCData(lcSync->savedKey, &status);
                        lcSync->syncMods(modString);
                        Sleep(4000);
                        exit(0);
                    }

                    LCSync::sync(lcSync);
                    break;
                case 1:
                    LCSync::createModpack(lcSync);
                    break;
                case 2:
                    LCSync::editModpack(lcSync);
                    break;
                case 3:
                    if (lcSync->deleteInstallation()) {
                        std::cout << "\nInstallation was successfully removed!" << std::endl;
                        lcSync->deleteKeyFile();
                        Sleep(4000);
                        exit(0);
                    }
                    else {
                        Sleep(4000);
                        exit(1);
                    }
                    break;
                }
                break;
            default:
                break;
            }
        }

        

        return true;
    }

    void sync(LCSCore *lcSync) {
        std::string modString{};

        while (true) {
            std::string key{};
            system("cls");
            lcSync->printLogo();

            std::cout << "Please enter your key: " << std::endl;
            std::getline(std::cin, key);

            long status{};

            modString = lcSync->requestLCData(key, &status);

            if (status != 200) {
                std::cout << "Wrong key!" << std::endl;
                Sleep(1500);
            }
            else {
                lcSync->savedKey = key;
                lcSync->storeKeyFile(key);
                break;
            }
        }

        lcSync->syncMods(modString);
    }

    void createModpack(LCSCore* lcSync) {
        std::string modString{""};

        bool exit{ false };
        while (!exit) {
            std::string modUrl{};
            system("cls");
            lcSync->printLogo();

            std::cout << "Paste a modURL you want to add, press [ENTER] to upload: " << std::endl;
            std::getline(std::cin, modUrl);

            if (modUrl.length() == 0) {
                lcSync->syncMods(modString);
                std::string key = lcSync->createSourceBin(modString);
                lcSync->savedKey = key;
                lcSync->storeKeyFile(key);
                break;
            }

            if (!(modUrl.find("https://") != std::string::npos))
            {
                std::cerr << "Invalid URL!" << std::endl;
                Sleep(1500);
                continue;
            }

            if (modString.length() == 0) {
                modString += modUrl;
            }
            else {
                modString += "@" + modUrl;
            }
        }
    }

    void editModpack(LCSCore* lcSync) {
        system("cls");
        lcSync->printLogo();

        while (true) {
            long statusCode;
            std::string requestedModString = lcSync->requestLCData(lcSync->savedKey, &statusCode);

            std::cout << "Current mods: " << std::endl;

            std::istringstream ss(requestedModString);
            if (requestedModString.find("@") != std::string::npos) {
                std::string token;

                while (std::getline(ss, token, '@'))
                {
                    std::cout << "- " << token << std::endl;
                }
            }
            else {
                std::cout << "- " << requestedModString << std::endl;
            }

            std::cout << "\n\"rm [link]\" to remove mod" << std::endl;
            std::cout << "\"add [link]\" to add mod\n" << std::endl;

            std::string cmd{};
            std::getline(std::cin, cmd);

            if (cmd.find("rm") != std::string::npos || cmd.find("add") != std::string::npos)
            {
                if (cmd.find(" ") != std::string::npos && cmd.find("https://") != std::string::npos) {
                    std::string arg;
                    std::string op;
                    std::string newKey;
                    std::string url;

                    std::istringstream ssCmd(cmd);
                    while (std::getline(ssCmd, arg, ' '))
                    {
                        if (arg == "rm" || arg == "add") op = arg;
                        else url = arg;
                    }

                    if (requestedModString.find(url) != std::string::npos)
                    {
                        system("cls");
                        lcSync->printLogo();
                        std::cerr << "Mod already installed\n" << std::endl;
                        continue;
                    }

                    if (op == "rm") {
                        //remove
                        requestedModString = lcSync->removeUrlFromModstring(requestedModString, url);

                        if (!lcSync->syncMods(requestedModString)) {
                            Sleep(2000);
                            exit(0);
                        }
                        newKey = lcSync->createSourceBin(requestedModString);
                    }
                    else if (op == "add") {
                        //add
                        requestedModString += "@" + url;

                        if (!lcSync->syncMods(requestedModString)) {
                            Sleep(2000);
                            exit(0);
                        }
                        newKey = lcSync->createSourceBin(requestedModString);
                    }

                    lcSync->savedKey = newKey;
                    lcSync->deleteKeyFile();
                    lcSync->storeKeyFile(newKey);
                    
                    exit(0);
                }
                else {
                    system("cls");
                    lcSync->printLogo();
                    std::cerr << "Invalid command\n" << std::endl;
                    continue;
                }
            }
            else {
                system("cls");
                lcSync->printLogo();
                std::cerr << "Invalid command\n" << std::endl;
                continue;
            }
        }
    }
}