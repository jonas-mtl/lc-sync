#include "Core.h"


LCSCore::LCSCore() {}

bool LCSCore::getLCInstallation() {
    HKEY hKey;
    LPCSTR registryPath = "SOFTWARE\\WOW6432Node\\Valve\\Steam";
    LPCSTR valueName = "InstallPath";

    if (RegOpenKeyExA(HKEY_LOCAL_MACHINE, registryPath, 0, KEY_READ, &hKey) != ERROR_SUCCESS) {
        std::cerr << "Error opening registry key." << std::endl;
        return false;
    }

    DWORD dataSize;
    DWORD dataType;

    if (RegQueryValueExA(hKey, valueName, NULL, &dataType, NULL, &dataSize) != ERROR_SUCCESS)
    {
        std::cerr << "Error querying InstallPath value size." << std::endl;
        RegCloseKey(hKey);
        return false;
    }

    char* installPathChar = new char[dataSize];

    if (RegQueryValueExA(hKey, valueName, NULL, &dataType, reinterpret_cast<LPBYTE>(installPathChar), &dataSize) != ERROR_SUCCESS) {
        std::cerr << "Error retrieving InstallPath value." << std::endl;
        return false;
    }

    this->installPath = installPathChar;
    this->installPath += "\\steamapps\\common\\Lethal Company";

    RegCloseKey(hKey);
    delete[] installPathChar;

    return true;
}

std::string LCSCore::downloadAndExtract(std::string url)
{
    std::filesystem::path lcSyncFolder = createLCSyncFolder();
    std::string zipPath = lcSyncFolder.string() + "\\LCSync.zip";

    if (URLDownloadToFileA(NULL, url.c_str(), zipPath.c_str(), 0, NULL) != 0) {
        std::cerr << "Error downloading file from URL." << std::endl;
        return "";
    }

    std::string extractPath = lcSyncFolder.string() + "\\Extracted";
    elz::extractZipE(zipPath, extractPath, "");

    return extractPath;
}

bool LCSCore::syncMods(std::string modString)
{
    std::istringstream ss(modString);
    bool failed{ false };

    this->resetPluginsFolder();

    if (modString.find("@") != std::string::npos) {
        std::string token;
        while (std::getline(ss, token, '@'))
        {
            std::string downloadedModPath = this->downloadAndExtract(token);

            if (downloadedModPath == "") {
                std::cerr << "Mod could not be downloaded." << std::endl;
                return false;
            }

            std::filesystem::path pluginsFolder{};
            this->findPluginsPath(downloadedModPath, &pluginsFolder);
            if (pluginsFolder == "") {
                std::cerr << "Could not find plugin content of " << token << std::endl;
                failed = true;
                continue;
            }
            else {
                this->move(pluginsFolder, this->installPath + "\\BepInEx\\plugins");
                this->moveDLLs(downloadedModPath, this->installPath + "\\BepInEx\\plugins");
            }
        }
    }
    else {
        std::string downloadedModPath = this->downloadAndExtract(modString);

        if (downloadedModPath == "") {
            std::cerr << "Mod could not be downloaded." << std::endl;
            return false;
        }

        std::filesystem::path pluginsFolder{};
        this->findPluginsPath(downloadedModPath, &pluginsFolder);
        if (pluginsFolder == "") {
            std::cerr << "Could not find plugin content of " << modString << std::endl;
            return false;
        }
        else {
            this->move(pluginsFolder, this->installPath + "\\BepInEx\\plugins");
            this->moveDLLs(downloadedModPath, this->installPath + "\\BepInEx\\plugins");
        }

    }
    this->deleteLCSyncFolder();

    std::cout << "\nUp-to-date" << (failed ? " - note that you could be missing some mods, maybe try again" : "") << std::endl;
    return true;
}

bool LCSCore::moveDLLs(std::filesystem::path srcPath, std::filesystem::path targetPath) {
    namespace fs = std::filesystem;

    for (fs::path p : fs::directory_iterator(srcPath)) {
        fs::path destFile = targetPath / p.filename();

        if (fs::is_directory(p)) {
            LCSCore::moveDLLs(p.string().c_str(), destFile.string().c_str());
        }
        else if (p.filename().string().find(".dll") != std::string::npos) {
            fs::rename(p, destFile);
        }
    }

    return true;
}

std::filesystem::path LCSCore::createLCSyncFolder()
{
    std::filesystem::path folderPath = this->installPath + "\\LCSync";
    CreateDirectory(folderPath.c_str(), NULL);

    return folderPath;
}

bool LCSCore::deleteLCSyncFolder()
{
    std::error_code error;
    uintmax_t removed;
    std::filesystem::_Remove_all_dir(this->installPath + "\\LCSync", error, removed);

    if (error.value() != 0) {
        std::cerr << error.message() << std::endl;
        return false;
    }

    return true;
}

bool LCSCore::resetPluginsFolder() {
    namespace fs = std::filesystem;

    for (fs::path p : fs::directory_iterator(this->installPath + "\\BepInEx\\plugins")) {
        if (fs::is_directory(p)) {
            if (p.filename() != "Bundles"){
                std::error_code error;
                uintmax_t removed;
                std::filesystem::_Remove_all_dir(this->installPath + "\\BepInEx\\plugins\\" + p.filename().string(), error, removed);

                if (error.value() != 0) {
                    std::cerr << error.message() << std::endl;
                    return false;
                }
            }
            else {
                continue;
            }

        }
        else {
            if (p.filename() != "LC_API.dll") {
                if (!std::filesystem::remove(this->installPath + "\\BepInEx\\plugins\\" + p.filename().string())) {
                    std::cerr << "Error removing file!" << std::endl;
                    return false;
                }
            }
        }
    }

    return true;
}

void LCSCore::move(std::filesystem::path src, std::filesystem::path dst) {
    namespace fs = std::filesystem;

    for (fs::path p : fs::directory_iterator(src)) {
        fs::path destFile = dst / p.filename();

        if (fs::is_directory(p)) {
            fs::create_directory(destFile);
            LCSCore::move(p.string().c_str(), destFile.string().c_str());
        }
        else {
            fs::rename(p, destFile);
        }
    }
}

bool LCSCore::checkFiles(std::filesystem::path path, std::vector<std::string>& fileNames) {
    namespace fs = std::filesystem;

    for (const auto& f : fileNames) {
        bool found{ false };

        for (fs::path p : fs::directory_iterator(path)) {
            if (p.filename().string() == f) {
                found = true;
            }
        }

        if (!found) return false;
    }

    return true;
}

void LCSCore::findPluginsPath(std::filesystem::path srcPath, std::filesystem::path* pluginsPath) {
    namespace fs = std::filesystem;
    std::filesystem::path targetPath{ "" };

    for (fs::path p : fs::directory_iterator(srcPath)) {

        if (!(p.filename().string().find(".") != std::string::npos)) {
            // is dir
            if (p.filename().string() == "Plugins" || p.filename().string() == "plugins") {
                *pluginsPath = srcPath / p.filename();
                break;
            }
            else {
                this->findPluginsPath(p.string(), pluginsPath);
            }
        }
    }
}

std::string LCSCore::requestLCData(std::string key, long* statusCode) {
    std::string url = "https://cdn.sourceb.in/bins/" + key + "/0";
    cpr::Response r = cpr::Get(cpr::Url{ url });

    *statusCode = r.status_code;

    return r.text;
}

bool LCSCore::deleteInstallation() {
    for (const auto& file : LCSyncConstants::bepInExFiles) {
        if (file.find(".") != std::string::npos) {
            // Remove files
            if (!std::filesystem::remove(this->installPath + "\\" + file)) {
                std::cerr << "Error removing file " << file << std::endl;
            }
        }
        else {
            // Remove dirs
            std::error_code error;
            uintmax_t removed;
            std::filesystem::_Remove_all_dir(this->installPath + "\\" + file, error, removed);

            if (error.value() != 0) {
                std::cerr << error.message() << std::endl;
                return false;
            }
        }
    }

    return true;
}

bool LCSCore::checkIfLCRunning()
{
    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

    if (snapshot == INVALID_HANDLE_VALUE) {
        std::cerr << "Error creating process snapshot." << std::endl;
        return false;
    }

    PROCESSENTRY32W processEntry; // Use PROCESSENTRY32W for wide characters
    processEntry.dwSize = sizeof(PROCESSENTRY32W);

    if (Process32FirstW(snapshot, &processEntry)) {
        do {
            if (_wcsicmp(processEntry.szExeFile, L"Lethal Company.exe") == 0) {
                CloseHandle(snapshot);
                std::cerr << "Please close Lethal Company before running LCSync";
                return true; 
            }
        } while (Process32NextW(snapshot, &processEntry));
    }

    CloseHandle(snapshot);
    return false;
}

void LCSCore::printLogo() 
{
    SetConsoleOutputCP(CP_UTF8);
    std::array<std::string, 10> logo = { u8" ██▓     ▄████▄    ██████▓██   ██▓ ███▄    █  ▄████▄  ",
                                         u8"▓██▒    ▒██▀ ▀█  ▒██    ▒ ▒██  ██▒ ██ ▀█   █ ▒██▀ ▀█  ",
                                         u8"▒██░    ▒▓█    ▄ ░ ▓██▄    ▒██ ██░▓██  ▀█ ██▒▒▓█    ▄ ",
                                         u8"▒██░    ▒▓▓▄ ▄██▒  ▒   ██▒ ░ ▐██▓░▓██▒  ▐▌██▒▒▓▓▄ ▄██▒",
                                         u8"░██████▒▒ ▓███▀ ░▒██████▒▒ ░ ██▒▓░▒██░   ▓██░▒ ▓███▀ ░",
                                         u8"░ ▒░▓  ░░ ░▒ ▒  ░▒ ▒▓▒ ▒ ░  ██▒▒▒ ░ ▒░   ▒ ▒ ░ ░▒ ▒  ░",
                                         u8"░ ░ ▒  ░  ░  ▒   ░ ░▒  ░ ░▓██ ░▒░ ░ ░░   ░ ▒░  ░  ▒   ",
                                         u8"  ░ ░   ░        ░  ░  ░  ▒ ▒ ░░     ░   ░ ░ ░        ",
                                         u8"    ░  ░░ ░            ░  ░ ░              ░ ░ ░      ",
                                         u8"        ░                 ░ ░                ░        "};


    const char* RED_TEXT = "\033[1;31m";
    const char* RESET_COLOR = "\033[0m";

    for (const auto& line : logo) {
        std::cout << RED_TEXT << line << std::endl;
    }

    std::cout << RESET_COLOR << "\nV 3.2.2 | By Trauubensaft\n" << std::endl;
}

std::string LCSCore::extractKeyFromJson(const std::string jsonResponse) {
    std::size_t keyStart = jsonResponse.find("\"key\":\"");
    if (keyStart != std::string::npos) {
        keyStart += 7; 
        std::size_t keyEnd = jsonResponse.find("\"", keyStart);
        if (keyEnd != std::string::npos) {
            return jsonResponse.substr(keyStart, keyEnd - keyStart);
        }
    }
    return "";
}

std::string LCSCore::createSourceBin(const std::string modString) {
    std::filesystem::path lcSyncFolder = this->createLCSyncFolder().string();
    std::filesystem::path filePath = lcSyncFolder / "mods.txt";

    std::ofstream outputFile(filePath);

    if (!outputFile.is_open()) {
        std::cerr << "Error opening the file!" << std::endl;
        return ""; // Return an empty string indicating an error
    }

    outputFile << modString;  // Use the provided modString instead of static content
    outputFile.close();

    // Build the JSON payload manually
    std::stringstream jsonPayload;
    jsonPayload << "{\"files\": [{\"content\": \"" << modString << "\", \"languageId\": 33}]}";
    // Replace 33 with the actual language ID (integer) that corresponds to the language of the content

    cpr::Response r = cpr::Post(cpr::Url{ "https://sourceb.in/api/bins" },
        cpr::Body{ jsonPayload.str() },
        cpr::Header{ {"Content-Type", "application/json"} });

    this->deleteLCSyncFolder();

    if (r.status_code == 200) {
        // Extract the key from the JSON response
        std::string key = extractKeyFromJson(r.text);
        return key;
    }
    else {
        std::cerr << "Error in the POST request. Status code: " << r.status_code << std::endl;
    }

    return ""; 
}

bool LCSCore::storeKeyFile(std::string key) {
    std::filesystem::path filePath = this->installPath + "\\.lcsync";

    std::ofstream outputFile(filePath);

    if (!outputFile.is_open()) {
        std::cerr << "Error opening key file!" << std::endl;
        return false;
    }

    outputFile << key; 
    outputFile.close();
    return true;
}

std::string LCSCore::readKeyFile() {
    std::filesystem::path filePath = this->installPath + "\\.lcsync";

    if (!std::filesystem::exists(filePath)) {
        std::cerr << "File does not exist: " << filePath << std::endl;
        return ""; 
    }

    std::ifstream inputFile(filePath);

    if (!inputFile.is_open()) {
        std::cerr << "Error opening the file: " << filePath << std::endl;
        return ""; 
    }

    std::string content((std::istreambuf_iterator<char>(inputFile)), std::istreambuf_iterator<char>());
    inputFile.close();

    return content;
}

bool LCSCore::deleteKeyFile() {
    if (!std::filesystem::remove(this->installPath + "\\" + ".lcsync")) {
        std::cerr << "Error removing key file!" << std::endl;
        return false;
    }

    return true;
}

void LCSCore::showMenu(int selectedOption) {
    system("cls");
    this->printLogo();

    std::cout << "Menu:\n";
    std::cout << (selectedOption == 0 ? "> " : "  ") << "Sync Mods" + ( this->savedKey != "" ? "(key found: " + this->savedKey + ")\n" : "\n");
    std::cout << (selectedOption == 1 ? "> " : "  ") << "Create Modpack\n";
    std::cout << (selectedOption == 2 ? "> " : "  ") << "Edit Modpack" + (this->savedKey != "" ? "(key found: " + this->savedKey + ")\n" : "\n");
    std::cout << (selectedOption == 3 ? "> " : "  ") << "Unmod\n";
}

std::string LCSCore::removeUrlFromModstring(const std::string modString, const std::string url) {
    std::string token{};
    std::istringstream ss(modString);
    std::string newModString{};

    while (std::getline(ss, token, '@'))
    {
        if (token != url) {
            if (newModString.length() != 0) {
                newModString += "@" + token;
            }
            else {
                newModString += token;

            }
        }
    }

    return newModString;
}