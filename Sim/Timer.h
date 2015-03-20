#pragma once

class Timer
{
public:
	Timer();
	~Timer();

	float timeSinceCreation();
	float timeSinceLastFrame();

private:
	float m_timeOfLastCall;
};

