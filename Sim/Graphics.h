#pragma once
#include <SDL.h>
#include <SDL_image.h>
#include <SDL_ttf.h>

#include <memory>
#include <string>


class Graphics
{
public:
	struct sdl_deleter
	{
		void operator()(SDL_Window *p) const { SDL_DestroyWindow(p); }
		void operator()(SDL_Renderer *p) const { SDL_DestroyRenderer(p); }
		void operator()(SDL_Texture *p) const { SDL_DestroyTexture(p); }
	};

	Graphics(
		int windowWidth, int WindowHeight, 
		const char* windowTitle, 
		int bgR, int bgG, int bgB);
	
	~Graphics();

	SDL_Texture* loadTexture(const std::string &filename);
	void loadFont(int size);

	SDL_Texture* createText(const std::string &message, SDL_Color colour);
	void closeTexture(SDL_Texture* texture);

	void renderTexture(SDL_Texture* texture, SDL_Rect dst, SDL_Rect *clip = nullptr);
	void renderTexture(SDL_Texture* texture, int x, int y, SDL_Rect* clip = nullptr);
	void renderRect(const std::shared_ptr<SDL_Rect> rect, Uint8 r = 255, Uint8 g = 255, Uint8 b = 255, Uint8 a = 255);

	void beginScene();
	void endScene();
	void getWindowSize(int* w, int* h);

private:
	// SDL Control

	SDL_Window* createWindow(char const *title, int x, int y, int w, int h, Uint32 flags);
	SDL_Renderer* createRenderer(int index, Uint32 flags);

	TTF_Font* font_;
	std::unique_ptr<SDL_Window, sdl_deleter> window_;
	std::unique_ptr<SDL_Renderer, sdl_deleter> renderer_;
	int m_backgroundRed;
	int m_backgroundGreen;
	int m_backgroundBlue;

};
