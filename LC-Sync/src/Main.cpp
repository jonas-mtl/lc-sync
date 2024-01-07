#include "LC-Sync.h"


int main() {
    using namespace LCSync;

    if (!setup()) {
        Sleep(4000);
        return 1;
    }

    Sleep(4000);
    return 0;
}