#pragma once

#include "SDL.h"

class Character;

class Input
{
public:
	Input();
	~Input();

	void update();
	void update(Character& character);
	const Uint8* getInput();
	bool windowClosed();

private:
	SDL_Event m_event;
	bool m_keysHeld[323];
	bool m_windowClosed;
};
