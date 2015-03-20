#include "Timer.h"
#include "SDL.h"


Timer::Timer() : m_timeOfLastCall(0.0f)
{
	SDL_Init(SDL_INIT_TIMER);
}


Timer::~Timer()
{
}

float Timer::timeSinceCreation()
{
	return SDL_GetTicks() / 1000.0f;
}

float Timer::timeSinceLastFrame()
{
	float thisTime = timeSinceCreation();
	float deltaTime = thisTime - m_timeOfLastCall;
	m_timeOfLastCall = thisTime;

	return deltaTime;
}
