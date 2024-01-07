#pragma once

#include <iostream>
#include <vector>

#if !defined(LCSYNC_CONSTANTS_H)
#define LCSYNC_CONSTANTS_H 

namespace LCSyncConstants
{
	constexpr const char* bepInExURL = "https://thunderstore.io/package/download/BepInEx/BepInExPack/5.4.2100/";
	inline std::vector<std::string> bepInExFiles = { "winhttp.dll", "doorstop_config.ini", "BepInEx" };

	constexpr const char* lcApiURL = "https://thunderstore.io/package/download/2018/LC_API/3.3.0/";
}

#endif
