#pragma once

#include <Windows.h>
#include <iostream>
#include <urlmon.h>
#include <Shlwapi.h>
#include <algorithm>
#include <sstream>
#include <filesystem>
#include <process.h>
#include <Tlhelp32.h>
#include <winbase.h>
#include <cpr/cpr.h>
#include <conio.h>

#include "../Constatns.h"
#include "Zip-Wrapper.h"

#pragma comment(lib, "urlmon.lib")


class LCSCore
{
public:
	std::string installPath{""};
	std::string savedKey{""};
	bool processWasRunning{ false };

	LCSCore();


	std::string downloadAndExtract(std::string url);
	std::string requestLCData(std::string key, long* statusCode);
	std::string createSourceBin(const std::string modString);
	std::string removeUrlFromModstring(const std::string modString, const std::string url);

	bool deleteLCSyncFolder();
	bool deleteInstallation();
	bool getLCInstallation();
	bool checkFiles(std::filesystem::path path, std::vector<std::string>& fileNames);
	bool moveDLLs(std::filesystem::path srcPath, std::filesystem::path targetPath);
	bool syncMods(std::string modString);

	bool storeKeyFile(std::string key);
	std::string readKeyFile();
	bool deleteKeyFile();

	void printLogo();
	bool resetPluginsFolder();
	void move(std::filesystem::path src, std::filesystem::path dst);
	void findPluginsPath(std::filesystem::path srcPath, std::filesystem::path* pluginsPath);
	void showMenu(int selectedOption);
	bool checkIfLCRunning();

private:
	std::filesystem::path createLCSyncFolder();
	std::string extractKeyFromJson(const std::string jsonResponse);
};

