#pragma once

#include <Windows.h>
#include <bcrypt.h>
#ifdef _MSC_VER
#pragma comment(lib, "bcrypt.lib")
#endif

#include <minizip-ng/mz.h>
#include <minizip-ng/mz_os.h>

#include <array>
#include <fstream>
#include <iostream>

#include <string>
#include <vector>

#include "fswrapper.h"
#include "Unzipper.h"


namespace elz
{
    class zip_exception : public std::runtime_error
    {
    public:
        explicit zip_exception(const std::string& error);
        ~zip_exception() override = default;
    };

    inline zip_exception::zip_exception(const std::string& error) : std::runtime_error(error)
    {
    }

    using path = std::filesystem::path;

    void extractZipE(const path& archive, const path& target = ".", const std::string& password = "");
    void extractFile(const path& archive, const path& fileInArchive, const path& target = ".", std::string outFilename = "", const std::string& password = "");
}
