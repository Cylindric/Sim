#include "Input.h"


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

void Input::readInput()
{
	//if (SDL_PollEvent(&m_event))
	//{
	//	if (m_event.type == SDL_QUIT)
	//	{
	//		m_windowClosed = true;
	//	}

	//	if (m_event.type == SDL_KEYDOWN)
	//	{
	//		m_keysHeld[m_event.key.keysym.sym] = true;
	//	}

	//	if (m_event.type == SDL_KEYUP)
	//	{
	//		m_keysHeld[m_event.key.keysym.sym] = false;
	//	}
	//}
}

const Uint8* Input::getInput()
{
	const Uint8* keys = SDL_GetKeyboardState(NULL);
	return keys;
}

bool Input::windowClosed()
{
	return m_windowClosed;
}
