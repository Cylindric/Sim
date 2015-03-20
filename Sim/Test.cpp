#include <iostream>
#include <string>
#include <vector>
#include <SDL.h>
#include <SDL_image.h>
#include <SDL_ttf.h>
#include "res_path.h"
#include "cleanup.h"
#include "Graphics.h"
#include "Input.h"
#include "Tile.h"
#include "Sprite.h"
#include "Character.h"
#include "Timer.h"
#include "World.h"
#include "CharacterManager.h"

const int WINDOW_WIDTH = 800;
const int WINDOW_HEIGHT = 400;
const char* WINDOW_TITLE = "SimTest";

const int BACKGROUND_RED = 0;
const int BACKGROUND_GREEN = 0;
const int BACKGROUND_BLUE = 0;

bool g_gameIsRunning = true;
Graphics* g_graphics = NULL;
Timer* g_timer = NULL;
Input* g_input = NULL;
World* g_world = NULL;
CharacterManager* g_charman = NULL;

void handleKeyboardInput();

int main(int, char**)
{

	g_graphics = new Graphics(WINDOW_WIDTH, WINDOW_HEIGHT, WINDOW_TITLE, BACKGROUND_RED, BACKGROUND_GREEN, BACKGROUND_BLUE);
	g_timer = new Timer();
	g_input = new Input();
	g_world = new World(g_graphics);
	g_charman = new CharacterManager(g_graphics, g_world);
	bool updateText = true;

	const int hist = 60;
	float fpshist[hist];
	for (int i = 0; i < hist; i++)
	{
		fpshist[i] = 0;
	}
	int fpsptr = 0;


	while (g_gameIsRunning)
	{
		SDL_PumpEvents();

		float deltaTime = g_timer->timeSinceLastFrame();
		// Calculate FPS, averaged over last 60 frames
		fpshist[fpsptr] = 1.0f / deltaTime;
		fpsptr = (++fpsptr % hist);
		float avg;
		for (int i = 0; i < hist; i++)
		{
			avg += fpshist[i];
		}
		avg = avg / hist;


		char buffer[50];
		sprintf_s(buffer, "FPS: %0.2f", avg);


		g_input->readInput();

		if (g_input->windowClosed())
		{
			g_gameIsRunning = false;
		}

		handleKeyboardInput();

		g_charman->update(deltaTime);
		g_graphics->beginScene();
		g_world->draw();
		g_charman->draw();
		g_graphics->renderText(buffer, 20, 10, WINDOW_HEIGHT-30);
		g_graphics->endScene();
	}


	delete g_timer;
	delete g_charman;
	delete g_world;
	delete g_input;
	delete g_graphics;
	return 0;
}

void handleKeyboardInput()
{
	const Uint8* keysHeld = g_input->getInput();

	if (keysHeld[SDL_SCANCODE_ESCAPE])
	{
		g_gameIsRunning = false;
	}

	g_charman->handleKeyboardInput(keysHeld);
}
