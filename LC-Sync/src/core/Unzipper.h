#pragma once

#include <iostream>
#include <string>
#include <vector>


#include <minizip-ng/mz_compat.h>

namespace ziputils
{
    class dump_error : public std::runtime_error
    {
    public:
        dump_error();
        ~dump_error() override = default;
    };

    class unzipper
    {
    public:
        unzipper();
        ~unzipper();

        bool open(std::string_view filename);
        void close();
        bool isOpen();

        bool openEntry(const std::string_view filename);
        void closeEntry();
        [[nodiscard]] bool isOpenEntry() const;
        [[nodiscard]] unsigned int getEntrySize() const;

        const std::vector<std::string>& getFilenames();
        const std::vector<std::string>& getFolders();

        unzipper& operator>>(std::ostream& os);
        [[nodiscard]] std::string dump() const;

    private:
        void readEntries();

    private:
        unzFile            zipFile_;
        bool            entryOpen_;

        std::vector<std::string> files_;
        std::vector<std::string> folders_;
    };
};
