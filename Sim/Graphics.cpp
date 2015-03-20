#include "Graphics.h"

#include <iostream>
#include "res_path.h"

Graphics::Graphics(
	int windowWidth, int WindowHeight,
	const char* windowTitle,
	int bgR, int bgG, int bgB)
{
	SDL_Init(SDL_INIT_VIDEO);
	IMG_Init(IMG_INIT_PNG);
	TTF_Init();

	m_window = SDL_CreateWindow(windowTitle, 100, 100, windowWidth, WindowHeight, SDL_WINDOW_SHOWN);
	m_renderer = SDL_CreateRenderer(m_window, -1, SDL_RENDERER_ACCELERATED | SDL_RENDERER_PRESENTVSYNC);
}


Graphics::~Graphics()
{
	TTF_Quit();
	SDL_Quit();
}


SDL_Texture* Graphics::loadTexture(const std::string &filename)
{
	SDL_Texture *texture = IMG_LoadTexture(m_renderer, filename.c_str());
	if (texture == NULL)
	{
		std::cout << "ERROR: loadTexture " << SDL_GetError() << std::endl;
	}
	return texture;
}

SDL_Texture* Graphics::createText(const std::string &message, const std::string &fontfile, SDL_Color colour, int fontSize)
{
	TTF_Font *font = TTF_OpenFont(fontfile.c_str(), fontSize);

	SDL_Surface *surf = TTF_RenderText_Blended(font, message.c_str(), colour);
	if (surf == nullptr){
		TTF_CloseFont(font);
		return nullptr;
	}

	SDL_Texture *texture = SDL_CreateTextureFromSurface(m_renderer, surf);

	SDL_FreeSurface(surf);
	TTF_CloseFont(font);
	return texture;
}

void Graphics::renderTexture(SDL_Texture *texture, SDL_Rect dst, SDL_Rect *clip)
{
	SDL_RenderCopy(m_renderer, texture, clip, &dst);
}

void Graphics::renderTexture(SDL_Texture* texture, int x, int y, SDL_Rect *clip)
{
	SDL_Rect dst;
	dst.x = x;
	dst.y = y;
	if (clip != nullptr)
	{
		dst.w = clip->w;
		dst.h = clip->h;
	}
	else {
		SDL_QueryTexture(texture, NULL, NULL, &dst.w, &dst.h);
	}
	this->renderTexture(texture, dst, clip);
}

void Graphics::renderText(const std::string &text, int size, int x, int y)
{
	TTF_Font* font = TTF_OpenFont((Resource::getResourcePath("Fonts") + "PoetsenOne-Regular.ttf").c_str(), size);
	if (font == NULL)
	{
		std::cout << "ERROR: TTF_OpenFont " << SDL_GetError() << std::endl;
	}
	SDL_Color fg = { 0, 0, 0 };

	SDL_Surface* surf = TTF_RenderText_Blended(font, text.c_str(), fg);
	if (surf == NULL)
	{
		std::cout << "ERROR: TTF_RenderText_Shaded " << SDL_GetError() << std::endl;
	}

	SDL_Texture* texture = SDL_CreateTextureFromSurface(m_renderer, surf);
	SDL_FreeSurface(surf);
	this->renderTexture(texture, x, y, nullptr);
}

void Graphics::closeTexture(SDL_Texture* texture)
{
	if (!texture){
		return;
	}
	SDL_DestroyTexture(texture);
}

void Graphics::beginScene()
{
	SDL_RenderClear(m_renderer);
}

void Graphics::endScene(){
	SDL_RenderPresent(m_renderer);
}

void Graphics::getWindowSize(int* w, int* h)
{
	SDL_GetWindowSize(m_window, w, h);
}
