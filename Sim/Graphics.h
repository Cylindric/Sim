#pragma once

#include <string>
#include <SDL.h>
#include <SDL_image.h>
#include <SDL_ttf.h>


class Graphics
{
public:
	Graphics(
		int windowWidth, int WindowHeight, 
		const char* windowTitle, 
		int bgR, int bgG, int bgB);
	
	~Graphics();

	SDL_Texture* loadTexture(const std::string &filename);
	SDL_Texture* createText(const std::string &message, const std::string &fontfile, SDL_Color colour, int fontSize);
	void closeTexture(SDL_Texture* texture);

	void renderTexture(SDL_Texture *tex, SDL_Rect dst, SDL_Rect *clip = nullptr);
	void renderTexture(SDL_Texture* texture, int x, int y, SDL_Rect* clip = nullptr);
	void renderText(const std::string &text, int size, int x, int y);

	void beginScene();
	void endScene();
	void getWindowSize(int* w, int* h);

private:
	SDL_Window* m_window;
	SDL_Renderer* m_renderer;
	int m_backgroundRed;
	int m_backgroundGreen;
	int m_backgroundBlue;

};
