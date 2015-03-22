#include "Graphics.h"

#include <iostream>
#include "res_path.h"

using namespace std;


Graphics::Graphics(
	int windowWidth, int WindowHeight,
	const char* windowTitle,
	int bgR, int bgG, int bgB)
{
	SDL_Init(SDL_INIT_VIDEO);
	IMG_Init(IMG_INIT_PNG);
	TTF_Init();
	window_ = unique_ptr<SDL_Window, Graphics::sdl_deleter>(createWindow(windowTitle, 100, 100, windowWidth, WindowHeight, SDL_WINDOW_SHOWN), Graphics::sdl_deleter());
	renderer_ = unique_ptr<SDL_Renderer, sdl_deleter>(createRenderer(-1, SDL_RENDERER_ACCELERATED | SDL_RENDERER_PRESENTVSYNC), Graphics::sdl_deleter());
}


Graphics::~Graphics()
{
	TTF_Quit();
	SDL_Quit();
}

SDL_Window* Graphics::createWindow(char const *title, int x, int y, int w, int h, Uint32 flags)
{
	return SDL_CreateWindow(title, x, y, w, h, flags);
}

SDL_Renderer* Graphics::createRenderer(int index, Uint32 flags)
{
	return SDL_CreateRenderer(&*window_, index, flags);
}

SDL_Texture* Graphics::loadTexture(const std::string &filename)
{
	SDL_Texture* texture = IMG_LoadTexture(&*renderer_, filename.c_str());
	if (texture == NULL)
	{
		std::cout << "ERROR: loadTexture " << SDL_GetError() << std::endl;
	}
	return texture;
}

SDL_Texture* Graphics::createText(const std::string &message, const std::string &fontfile, SDL_Color colour, int fontSize)
{
	TTF_Font* font = TTF_OpenFont(fontfile.c_str(), fontSize);

	SDL_Surface* surf = TTF_RenderText_Blended(font, message.c_str(), colour);
	if (surf == nullptr){
		TTF_CloseFont(font);
		return nullptr;
	}

	auto texture = SDL_CreateTextureFromSurface(&*renderer_, &*surf);

	SDL_FreeSurface(&*surf);
	TTF_CloseFont(&*font);
	return texture;
}

void Graphics::renderTexture(SDL_Texture *texture, SDL_Rect dst, SDL_Rect *clip)
{
	SDL_RenderCopy(&*renderer_, texture, clip, &dst);
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

	SDL_Texture* texture = SDL_CreateTextureFromSurface(&*renderer_, surf);
	SDL_FreeSurface(surf);
	this->renderTexture(texture, x, y, nullptr);
}

void Graphics::renderRect(const shared_ptr<SDL_Rect> rect, Uint8 r, Uint8 g, Uint8 b, Uint8 a)
{
	SDL_SetRenderDrawColor(&*renderer_, r, g, b, a);
	SDL_RenderDrawRect(&*renderer_, &*rect);
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
	SDL_RenderClear(&*renderer_);
}

void Graphics::endScene(){
	SDL_RenderPresent(&*renderer_);
}

void Graphics::getWindowSize(int* w, int* h)
{
	SDL_GetWindowSize(&*window_, w, h);
}
