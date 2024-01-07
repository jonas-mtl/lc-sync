#pragma once

#include "core/Core.h"
#include "Constatns.h"

namespace LCSync {

	bool setup();
	void sync(LCSCore* lcSync);
	void createModpack(LCSCore* lcSync);
	void editModpack(LCSCore* lcSync);
}