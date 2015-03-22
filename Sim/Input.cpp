#include "Input.h"
#include "Character.h"

Input::Input()
{
	m_windowClosed = false;

	// Clear all key states
	for (int i = 0; i < 323; i++)
	{
		m_keysHeld[i] = false;
	}
}


Input::~Input()
{
}

const Uint8* Input::getInput()
{
	const Uint8* keys = SDL_GetKeyboardState(NULL);
	return keys;
}

void Input::update()
{
	const Uint8* keysHeld = this->getInput();
	if (keysHeld[SDL_SCANCODE_ESCAPE])
	{
		m_windowClosed = true;
	}
}

void Input::update(Character& character)
{
	const Uint8* keysHeld = this->getInput();

	if (keysHeld[SDL_SCANCODE_A])
	{
		character.moveLeft();
	}
	else if (keysHeld[SDL_SCANCODE_D])
	{
		character.moveRight();
	}
	else if (keysHeld[SDL_SCANCODE_W])
	{
		character.moveUp();
	}
	else if (keysHeld[SDL_SCANCODE_S])
	{
		character.moveDown();
	}
	else
	{
		character.stopMoving();
	}
}

bool Input::windowClosed()
{
	return m_windowClosed;
}
