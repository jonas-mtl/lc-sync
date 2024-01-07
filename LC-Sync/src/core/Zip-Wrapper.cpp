#include "Zip-Wrapper.h"


namespace elz
{
    std::string _resolvePath(const std::string& entry)
    {
        std::array<char, 512> buf = {};
        const int32_t err = mz_path_resolve(entry.c_str(), buf.data(), buf.size());
        if (err != MZ_OK)
        {
            throw zip_exception("error on resolving path of entry : '" + entry + "'");
        }
        return buf.data();
    }

    void _extractFile(ziputils::unzipper& zipFile, const path& filename, const path& target, const std::string& password = "")
    {
        zipFile.openEntry(filename.string().c_str());
        std::ofstream wFile;
        wFile.open(target.string(), std::ios_base::binary | std::ios_base::out);
        try
        {
            const std::string dumped = zipFile.dump();
            wFile.write(dumped.c_str(), static_cast<std::streamsize>(dumped.size()));
        }
        catch (const ziputils::dump_error& e)
        {
            throw zip_exception("exception occurred when extracting file '" + filename.string() + "' : " + std::string(e.what()));
        }
        wFile.close();
    }

    void extractZipE(const path& archive, const path& target, const std::string& password)
    {
        ziputils::unzipper zipFile;
        bool openResult = zipFile.open(archive.string().c_str());
        if (!openResult)
        {
            std::cerr << "\nERROR: Downloaded file could not be unpacked. Check download url!" << std::endl;
            Sleep(2000);
            exit(0);
        }

        for (const std::string& filename : zipFile.getFilenames())
        {
            std::string real_path = _resolvePath(filename);
            std::filesystem::path currentDir = target / std::filesystem::path(real_path).parent_path();

            std::filesystem::create_directories(currentDir);
            std::filesystem::path currentFile = target / real_path;

            _extractFile(zipFile, filename, currentFile.string(), password);
        }
    }

    void extractFile(const path& archive, const path& file_in_archive, const path& target, std::string out_filename, const std::string& password)
    {
        ziputils::unzipper zipFile;
        zipFile.open(archive.string().c_str());
        out_filename = (out_filename.empty() ? file_in_archive.string() : out_filename);
        std::filesystem::create_directories(target);
        const std::string real_path = _resolvePath(file_in_archive.string());
        _extractFile(zipFile, file_in_archive.string(), target / real_path);
    }
} 
